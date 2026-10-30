namespace Stock.Mcp.Models;

/// <summary>
/// Describes how an update should change the current stock level.
/// </summary>
public enum StockUpdateMode
{
    /// <summary>
    /// Increase stock by the supplied quantity.
    /// </summary>
    Add,

    /// <summary>
    /// Decrease stock by the supplied quantity.
    /// </summary>
    Remove,

    /// <summary>
    /// Replace stock with the supplied quantity.
    /// </summary>
    Set
}
