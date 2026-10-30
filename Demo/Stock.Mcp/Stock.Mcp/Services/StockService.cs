namespace Stock.Mcp.Services;

/// <summary>
/// Coordinates stock-control validation and response shaping for MCP tools.
/// </summary>
/// <param name="database">SQLite stock database.</param>
public class StockService(StockDatabase database)
{
    private static StockError Error(string message) => new(message);

    private static string NormalizeProductCode(string productCode) =>
        productCode.Trim().ToUpperInvariant();

    private static object ProductResponse(Product product) => new
    {
        productCode = product.ProductCode,
        description = product.Description,
        costPrice = product.CostPrice,
        sellPrice = product.SellPrice,
        quantityStock = product.QuantityStock
    };

    private static string? Validate(Product product)
    {
        if (string.IsNullOrWhiteSpace(product.Description))
            return "Description is required.";
        if (product.CostPrice < 0)
            return "Cost price must be greater than or equal to zero.";
        if (product.SellPrice < 0)
            return "Sell price must be greater than or equal to zero.";
        if (product.QuantityStock < 0)
            return "Quantity stock must be greater than or equal to zero.";
        return null;
    }

    /// <summary>
    /// Searches products by code or description and optionally filters by maximum stock.
    /// </summary>
    /// <param name="query">Optional product code or description search text.</param>
    /// <param name="maxStock">Optional exclusive upper stock threshold.</param>
    /// <returns>A response object containing the count and matching products, or an error object.</returns>
    public async Task<object> SearchProducts(string? query = null, int? maxStock = null)
    {
        if (maxStock < 0)
            return Error("Max stock must be greater than or equal to zero.");

        var products = await database.SearchProducts(query, maxStock);
        return new
        {
            count = products.Count,
            products = products.Select(ProductResponse).ToList()
        };
    }

    /// <summary>
    /// Creates a new stock product after validating required fields and prices.
    /// </summary>
    /// <param name="productCode">Unique product code.</param>
    /// <param name="description">Product description.</param>
    /// <param name="costPrice">Product cost price.</param>
    /// <param name="sellPrice">Product selling price.</param>
    /// <param name="quantityStock">Initial stock quantity.</param>
    /// <returns>The created product response, or an error object.</returns>
    public async Task<object> CreateProduct(
        string productCode,
        string description,
        decimal costPrice,
        decimal sellPrice,
        int quantityStock = 0)
    {
        if (string.IsNullOrWhiteSpace(productCode))
            return Error("Product code is required.");

        var product = new Product
        {
            ProductCode = NormalizeProductCode(productCode),
            Description = description?.Trim(),
            CostPrice = costPrice,
            SellPrice = sellPrice,
            QuantityStock = quantityStock
        };

        if (Validate(product) is { } validationError)
            return Error(validationError);

        return await database.AddProduct(product)
            ? ProductResponse(product)
            : Error($"Product '{product.ProductCode}' already exists.");
    }

    /// <summary>
    /// Adds, removes, or sets stock for an existing product.
    /// </summary>
    /// <param name="productCode">Product code to update.</param>
    /// <param name="quantity">Quantity to add, remove, or set.</param>
    /// <param name="mode">The stock update mode.</param>
    /// <returns>A stock update response, or an error object.</returns>
    public async Task<object> UpdateStock(string productCode, int quantity, StockUpdateMode mode)
    {
        if (!Enum.IsDefined(mode))
            return Error("Stock update mode must be Add, Remove, or Set.");
        if (quantity < 0)
            return Error("Quantity must be greater than or equal to zero. Use mode Remove to reduce stock.");
        if (string.IsNullOrWhiteSpace(productCode))
            return Error("Product code is required.");

        var result = await database.UpdateStock(NormalizeProductCode(productCode), quantity, mode);
        if (result.Product is not { } product)
            return Error(result.Error!);

        return new
        {
            productCode = product.ProductCode,
            description = product.Description,
            mode = mode.ToString(),
            previousStock = result.PreviousStock,
            quantity,
            quantityStock = product.QuantityStock
        };
    }

    /// <summary>
    /// Records a product sale and reduces stock.
    /// </summary>
    /// <param name="productCode">Product code sold.</param>
    /// <param name="quantitySold">Quantity sold.</param>
    /// <param name="date">Optional sale date; defaults to now.</param>
    /// <returns>A sale response containing remaining stock, or an error object.</returns>
    public async Task<object> RecordSale(string productCode, int quantitySold, DateTimeOffset? date = null)
    {
        if (quantitySold <= 0)
            return Error("Quantity sold must be greater than zero.");

        if (string.IsNullOrWhiteSpace(productCode))
            return Error("Product code is required.");

        var saleDate = date ?? DateTimeOffset.Now;

        var result = await database.RecordSale(NormalizeProductCode(productCode), quantitySold, saleDate);
        if (result.Product is not { } product)
            return Error(result.Error!);

        return new
        {
            productCode = product.ProductCode,
            description = product.Description,
            quantitySold,
            saleDate = saleDate.ToString("O"),
            quantityStock = product.QuantityStock
        };
    }

    /// <summary>
    /// Gets sales history for a product over an optional date range.
    /// </summary>
    /// <param name="productCode">Product code to report on.</param>
    /// <param name="fromDate">Optional inclusive start date.</param>
    /// <param name="toDate">Optional inclusive end date.</param>
    /// <returns>A sales history response containing totals and sale records, or an error object.</returns>
    public async Task<object> GetSalesHistory(
        string productCode,
        DateTimeOffset? fromDate = null,
        DateTimeOffset? toDate = null)
    {
        if (string.IsNullOrWhiteSpace(productCode))
            return Error("Product code is required.");

        var normalizedCode = NormalizeProductCode(productCode);
        var product = await database.GetProduct(normalizedCode);
        if (product is null)
            return Error($"Product '{normalizedCode}' was not found.");

        if (fromDate is not null && toDate is not null && fromDate > toDate)
            return Error("From date must be earlier than or equal to to date.");

        var sales = await database.GetSalesHistory(normalizedCode, fromDate, toDate);
        return new
        {
            productCode = product.ProductCode,
            description = product.Description,
            fromDate = fromDate?.ToString("O"),
            toDate = toDate?.ToString("O"),
            totalQuantitySold = sales.Sum(s => s.QuantitySold),
            count = sales.Count,
            sales = sales.Select(s => new
            {
                id = s.Id,
                date = s.Date.ToString("O"),
                productCode = s.ProductCode,
                quantitySold = s.QuantitySold
            }).ToList()
        };
    }
}
