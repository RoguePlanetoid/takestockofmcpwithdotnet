namespace Stock.Mcp.Models;

/// <summary>
/// A business failure that the tool adapter exposes as an MCP error.
/// </summary>
/// <param name="Error">The message explaining why the operation failed.</param>
public sealed record StockError(string Error);
