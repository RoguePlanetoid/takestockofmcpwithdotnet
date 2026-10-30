namespace Stock.Mcp.Configuration;

/// <summary>
/// Configuration options for the stock-control demo server.
/// </summary>
public class StockOptions
{
    /// <summary>
    /// Gets or sets the SQLite database path.
    /// </summary>
    public string DatabasePath { get; set; } = "stock.db";
}
