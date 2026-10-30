namespace Stock.Mcp.Tools;

/// <summary>
/// Provides the MCP tool names exposed by the stock-control demo server.
/// </summary>
public static class StockToolNames
{
    /// <summary>
    /// MCP tool name for product search.
    /// </summary>
    public const string SearchProducts = "search_products";

    /// <summary>
    /// MCP tool name for product creation.
    /// </summary>
    public const string CreateProduct = "create_product";

    /// <summary>
    /// MCP tool name for stock updates.
    /// </summary>
    public const string UpdateStock = "update_stock";

    /// <summary>
    /// MCP tool name for sale recording.
    /// </summary>
    public const string RecordSale = "record_sale";

    /// <summary>
    /// MCP tool name for sales history.
    /// </summary>
    public const string GetSalesHistory = "get_sales_history";

    /// <summary>
    /// Gets all MCP tool names exposed by the demo server.
    /// </summary>
    public static readonly string[] All =
    [
        SearchProducts,
        CreateProduct,
        UpdateStock,
        RecordSale,
        GetSalesHistory
    ];
}
