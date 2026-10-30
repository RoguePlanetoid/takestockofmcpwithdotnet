namespace Stock.Mcp.Tools;

/// <summary>
/// MCP tools for stock-control product, stock, and sales operations.
/// </summary>
/// <param name="service">Stock service.</param>
public class StockTools(StockService service)
{
    private static async Task<CallToolResult> ToToolResult(Task<object> operation)
    {
        var value = await operation;
        return new CallToolResult
        {
            IsError = value is StockError,
            Content = [new TextContentBlock { Text = JsonSerializer.Serialize(value, JsonSerializerOptions.Web) }]
        };
    }

    /// <summary>
    /// Searches products by product code or description, with optional low-stock filtering.
    /// </summary>
    [McpServerTool(Name = StockToolNames.SearchProducts)]
    [Description("Search products by code or description; results include current stock. Demo descriptions are English: translate description queries if needed and use short terms. No automatic translation or plural handling. If no matches, try a singular form, synonym, or broader term. Use only returned products and resolve ambiguity before changing stock. Use maxStock for low stock.")]
    public Task<CallToolResult> SearchProducts(
        [Description("Product code (do not translate) or short catalogue term, such as 'brake pad'. Leave empty when filtering by stock only.")] string? query = null,
        [Description("Only return products with stock less than this value. Use for low-stock searches.")] int? maxStock = null) =>
        ToToolResult(service.SearchProducts(query, maxStock));

    /// <summary>
    /// Creates a new stock product.
    /// </summary>
    [McpServerTool(Name = StockToolNames.CreateProduct)]
    [Description("Create a new product. Product code, description, cost price, and sell price are required.")]
    public Task<CallToolResult> CreateProduct(
        [Description("Unique product code, for example BP123.")] string productCode,
        [Description("Product description, for example Premium Brake Pad.")] string description,
        [Description("Product cost price.")] decimal costPrice,
        [Description("Product sell price.")] decimal sellPrice,
        [Description("Initial stock quantity.")] int quantityStock = 0) =>
        ToToolResult(service.CreateProduct(productCode, description, costPrice, sellPrice, quantityStock));

    /// <summary>
    /// Updates stock by adding, removing, or setting a quantity.
    /// </summary>
    [McpServerTool(Name = StockToolNames.UpdateStock)]
    [Description("Update stock by adding to it, removing from it, or setting it to an exact level.")]
    public Task<CallToolResult> UpdateStock(
        [Description("Product code to update, for example BP123.")] string productCode,
        [Description("Quantity to add, remove, or set. Must be zero or greater.")] int quantity,
        [Description("Stock update mode: Add, Remove, or Set.")] StockUpdateMode mode) =>
        ToToolResult(service.UpdateStock(productCode, quantity, mode));

    /// <summary>
    /// Records a product sale and reduces available stock.
    /// </summary>
    [McpServerTool(Name = StockToolNames.RecordSale)]
    [Description("Record a product sale and reduce stock.")]
    public Task<CallToolResult> RecordSale(
        [Description("Product code sold, for example BP123.")] string productCode,
        [Description("Quantity sold.")] int quantitySold,
        [Description("Sale date. Defaults to now if omitted.")] DateTimeOffset? date = null) =>
        ToToolResult(service.RecordSale(productCode, quantitySold, date));

    /// <summary>
    /// Gets sales history for a product, optionally constrained by date range.
    /// </summary>
    [McpServerTool(Name = StockToolNames.GetSalesHistory)]
    [Description("Get sales history for a product over an optional date range.")]
    public Task<CallToolResult> GetSalesHistory(
        [Description("Product code, for example BP123.")] string productCode,
        [Description("Start date. Optional.")] DateTimeOffset? fromDate = null,
        [Description("End date. Optional.")] DateTimeOffset? toDate = null) =>
        ToToolResult(service.GetSalesHistory(productCode, fromDate, toDate));
}
