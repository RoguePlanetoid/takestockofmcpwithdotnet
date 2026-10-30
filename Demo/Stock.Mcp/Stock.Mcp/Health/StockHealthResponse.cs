namespace Stock.Mcp.Health;

/// <summary>
/// Gets the health-check options used by the stock-control demo server.
/// </summary>
public class StockHealthResponse
{
    /// <summary>
    /// Gets the health-check options used by the stock-control demo server.
    /// </summary>
    public static HealthCheckOptions Options { get; } = new()
    {
        ResponseWriter = Write
    };

    private static Task Write(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        return context.Response.WriteAsJsonAsync(new
        {
            status = report.Status.ToString(),
            name = "Stock.Mcp",
            mcpEndpoint = "/mcp",
            transport = "HTTP",
            database = "SQLite",
            tools = StockToolNames.All
        });
    }
}
