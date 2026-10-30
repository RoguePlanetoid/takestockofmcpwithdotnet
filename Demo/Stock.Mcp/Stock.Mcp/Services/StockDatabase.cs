namespace Stock.Mcp.Services;

/// <summary>
/// Provides SQLite data access for stock products and sales.
/// </summary>
public class StockDatabase(IOptions<StockOptions> options)
{
    private readonly string connection = new SqliteConnectionStringBuilder
    {
        DataSource = options.Value.DatabasePath
    }.ToString();

    private async Task<SqliteConnection> Open()
    {
        var conn = new SqliteConnection(connection);
        await conn.OpenAsync();
        return conn;
    }

    private static Product ReadProduct(SqliteDataReader r) => new()
    {
        ProductCode = r.GetString(0),
        Description = r.GetString(1),
        CostPrice = Convert.ToDecimal(r.GetDouble(2)),
        SellPrice = Convert.ToDecimal(r.GetDouble(3)),
        QuantityStock = r.GetInt32(4)
    };

    private static Sale ReadSale(SqliteDataReader r) => new()
    {
        Id = r.GetInt32(0),
        Date = DateTimeOffset.Parse(r.GetString(1)),
        ProductCode = r.GetString(2),
        QuantitySold = r.GetInt32(3)
    };

    /// <summary>
    /// Creates the demo database if it does not already exist.
    /// </summary>
    public async Task CreateDatabase()
    {
        await using var conn = await Open();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS Products (
                ProductCode TEXT PRIMARY KEY,
                Description TEXT NOT NULL,
                CostPrice REAL NOT NULL,
                SellPrice REAL NOT NULL,
                QuantityStock INTEGER NOT NULL DEFAULT 0
            );

            CREATE TABLE IF NOT EXISTS Sales (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Date TEXT NOT NULL,
                ProductCode TEXT NOT NULL,
                QuantitySold INTEGER NOT NULL,
                FOREIGN KEY (ProductCode) REFERENCES Products(ProductCode)
            );
            """;
        await cmd.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Adds a product to the catalogue.
    /// </summary>
    /// <param name="product">The product to add.</param>
    /// <returns><see langword="true"/> when the product was inserted.</returns>
    public async Task<bool> AddProduct(Product product)
    {
        await using var conn = await Open();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            INSERT INTO Products (ProductCode, Description, CostPrice, SellPrice, QuantityStock)
            VALUES (@ProductCode, @Description, @CostPrice, @SellPrice, @QuantityStock)
            ON CONFLICT(ProductCode) DO NOTHING;
            """;
        cmd.Parameters.AddWithValue("@ProductCode", product.ProductCode);
        cmd.Parameters.AddWithValue("@Description", product.Description);
        cmd.Parameters.AddWithValue("@CostPrice", product.CostPrice);
        cmd.Parameters.AddWithValue("@SellPrice", product.SellPrice);
        cmd.Parameters.AddWithValue("@QuantityStock", product.QuantityStock);
        return await cmd.ExecuteNonQueryAsync() == 1;
    }

    /// <summary>
    /// Gets a product by product code.
    /// </summary>
    /// <param name="productCode">The normalized product code to find.</param>
    /// <returns>The matching product, or <see langword="null"/> when no product exists.</returns>
    public async Task<Product?> GetProduct(string productCode)
    {
        await using var conn = await Open();
        return await GetProduct(conn, null, productCode);
    }

    private static async Task<Product?> GetProduct(
        SqliteConnection conn, SqliteTransaction? transaction, string productCode)
    {
        await using var cmd = conn.CreateCommand();
        cmd.Transaction = transaction;
        cmd.CommandText = """
            SELECT ProductCode, Description, CostPrice, SellPrice, QuantityStock
            FROM Products
            WHERE ProductCode = @ProductCode;
            """;
        cmd.Parameters.AddWithValue("@ProductCode", productCode);
        await using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? ReadProduct(reader) : null;
    }

    /// <summary>
    /// Searches products by code or description and optionally filters by maximum stock.
    /// </summary>
    /// <param name="query">Optional product code or description search text.</param>
    /// <param name="maxStock">Optional exclusive upper stock threshold for low-stock searches.</param>
    /// <returns>The matching products ordered by product code.</returns>
    public async Task<List<Product>> SearchProducts(string? query, int? maxStock)
    {
        await using var conn = await Open();
        await using var cmd = conn.CreateCommand();

        cmd.CommandText = """
            SELECT ProductCode, Description, CostPrice, SellPrice, QuantityStock
            FROM Products
            WHERE (@Query IS NULL
                OR ProductCode LIKE @Query COLLATE NOCASE
                OR Description LIKE @Query COLLATE NOCASE)
              AND (@MaxStock IS NULL OR QuantityStock < @MaxStock)
            ORDER BY ProductCode;
            """;
        var queryText = string.IsNullOrWhiteSpace(query) ? null : query.Trim();
        cmd.Parameters.AddWithValue("@Query", queryText is null ? DBNull.Value : $"%{queryText}%");
        cmd.Parameters.AddWithValue("@MaxStock", maxStock is null ? DBNull.Value : maxStock.Value);

        await using var reader = await cmd.ExecuteReaderAsync();
        var products = new List<Product>();
        while (await reader.ReadAsync())
            products.Add(ReadProduct(reader));
        return products;
    }

    /// <summary>
    /// Reads, validates, and updates stock in one transaction.
    /// </summary>
    /// <param name="productCode">The normalized product code to update.</param>
    /// <param name="quantity">The quantity to add, remove, or set.</param>
    /// <param name="mode">The stock update mode.</param>
    /// <returns>The updated product and previous stock, or a business error.</returns>
    public async Task<(Product? Product, int PreviousStock, string? Error)> UpdateStock(
        string productCode, int quantity, StockUpdateMode mode)
    {
        await using var conn = await Open();
        // Take the write lock before reading so concurrent changes cannot use stale stock.
        await using var transaction = conn.BeginTransaction(deferred: false);
        var product = await GetProduct(conn, transaction, productCode);
        if (product is null)
            return (null, 0, $"Product '{productCode}' was not found.");

        var previousStock = product.QuantityStock;
        var newStock = mode switch
        {
            StockUpdateMode.Add => (long)previousStock + quantity,
            StockUpdateMode.Remove => (long)previousStock - quantity,
            StockUpdateMode.Set => quantity,
            _ => throw new ArgumentOutOfRangeException(nameof(mode))
        };
        if (newStock < 0)
            return (null, previousStock,
                $"Cannot remove {quantity}; only {previousStock} of {product.ProductCode} {product.Description} are in stock.");
        if (newStock > int.MaxValue)
            return (null, previousStock, $"Stock cannot exceed {int.MaxValue}.");

        await SetStock(conn, transaction, productCode, (int)newStock);
        await transaction.CommitAsync();
        product.QuantityStock = (int)newStock;
        return (product, previousStock, null);
    }

    /// <summary>
    /// Records a sale and reduces stock in one transaction.
    /// </summary>
    /// <param name="productCode">The normalized product code sold.</param>
    /// <param name="quantitySold">The quantity sold.</param>
    /// <param name="date">The sale date.</param>
    /// <returns>
    /// The updated product, or a business error when the sale cannot be recorded.
    /// </returns>
    public async Task<(Product? Product, string? Error)> RecordSale(
        string productCode, int quantitySold, DateTimeOffset date)
    {
        await using var conn = await Open();
        await using var transaction = conn.BeginTransaction(deferred: false);
        var product = await GetProduct(conn, transaction, productCode);
        if (product is null)
            return (null, $"Product '{productCode}' was not found.");
        if (product.QuantityStock < quantitySold)
            return (null,
                $"Cannot sell {quantitySold} of {product.ProductCode} {product.Description} because only {product.QuantityStock} are in stock.");

        await using (var insert = conn.CreateCommand())
        {
            insert.Transaction = transaction;
            insert.CommandText = """
                INSERT INTO Sales (Date, ProductCode, QuantitySold)
                VALUES (@Date, @ProductCode, @QuantitySold);
                """;
            insert.Parameters.AddWithValue("@Date", date.ToString("O"));
            insert.Parameters.AddWithValue("@ProductCode", productCode);
            insert.Parameters.AddWithValue("@QuantitySold", quantitySold);
            await insert.ExecuteNonQueryAsync();
        }

        product.QuantityStock -= quantitySold;
        await SetStock(conn, transaction, productCode, product.QuantityStock);

        await transaction.CommitAsync();
        return (product, null);
    }

    private static async Task SetStock(
        SqliteConnection conn, SqliteTransaction transaction, string productCode, int quantityStock)
    {
        await using var cmd = conn.CreateCommand();
        cmd.Transaction = transaction;
        cmd.CommandText = """
            UPDATE Products
            SET QuantityStock = @QuantityStock
            WHERE ProductCode = @ProductCode;
            """;
        cmd.Parameters.AddWithValue("@ProductCode", productCode);
        cmd.Parameters.AddWithValue("@QuantityStock", quantityStock);
        await cmd.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Gets sales history for a product over an optional date range.
    /// </summary>
    /// <param name="productCode">The normalized product code to report on.</param>
    /// <param name="fromDate">Optional inclusive start date.</param>
    /// <param name="toDate">Optional inclusive end date.</param>
    /// <returns>Matching sales ordered from newest to oldest.</returns>
    public async Task<List<Sale>> GetSalesHistory(string productCode, DateTimeOffset? fromDate, DateTimeOffset? toDate)
    {
        await using var conn = await Open();
        await using var cmd = conn.CreateCommand();

        cmd.CommandText = """
            SELECT Id, Date, ProductCode, QuantitySold
            FROM Sales
            WHERE ProductCode = @ProductCode
              AND (@FromDate IS NULL OR julianday(Date) >= julianday(@FromDate))
              AND (@ToDate IS NULL OR julianday(Date) <= julianday(@ToDate))
            ORDER BY julianday(Date) DESC, Id DESC;
            """;
        cmd.Parameters.AddWithValue("@ProductCode", productCode);
        cmd.Parameters.AddWithValue("@FromDate", fromDate is null ? DBNull.Value : fromDate.Value.ToString("O"));
        cmd.Parameters.AddWithValue("@ToDate", toDate is null ? DBNull.Value : toDate.Value.ToString("O"));

        await using var reader = await cmd.ExecuteReaderAsync();
        var sales = new List<Sale>();
        while (await reader.ReadAsync())
            sales.Add(ReadSale(reader));
        return sales;
    }
}
