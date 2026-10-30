var builder = WebApplication.CreateBuilder(args);

// Keep demo settings in appsettings.json so the database path can be changed without code.
builder.Services.Configure<StockOptions>(builder.Configuration.GetSection("Stock"));
builder.Services.AddHealthChecks();

// Add the MCP services: the transport to use (http) and the tools to register.
builder.Services
    .AddScoped<StockDatabase>()
    .AddScoped<StockService>()
    .AddScoped<StockPrompts>()
    .AddScoped<StockResources>()
    .AddMcpServer(options =>
    {
        options.ServerInfo = new Implementation
        {
            Name = "Stock",
            Version = "1.0.0",
            Description = "Model Context Protocol (MCP) Stock Demo"            
        };
    })
    .WithHttpTransport(options =>
    {
        // Stateless mode is recommended for servers that don't need
        // server-to-client requests like sampling or elicitation.
        // See https://csharp.sdk.modelcontextprotocol.io/concepts/transports/transports.html for details.
        options.Stateless = true;
    })
    .WithTools<StockTools>()
    .WithPrompts<StockPrompts>()
    .WithResources<StockResources>();

var app = builder.Build();

// Create the SQLite tables on startup so the demo works from a clean folder.
using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<StockDatabase>().CreateDatabase();
}

// /health is the normal ASP.NET Core health endpoint with demo-friendly JSON.
app.MapHealthChecks("/health", StockHealthResponse.Options);
app.MapMcp("/mcp");

app.Run();
