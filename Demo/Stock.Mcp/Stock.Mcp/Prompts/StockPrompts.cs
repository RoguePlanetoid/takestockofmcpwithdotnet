namespace Stock.Mcp.Prompts;

/// <summary>
/// Reusable stock-control prompt templates.
/// </summary>
[McpServerPromptType]
public class StockPrompts
{
    private static readonly Assembly assembly = typeof(StockPrompts).Assembly;
    private static readonly string prefix = $"{typeof(StockPrompts).Namespace}.Templates.";

    private static string? Read(string name)
    {
        using var stream = assembly.GetManifestResourceStream($"{prefix}{name}");
        if (stream is null)
            return null;
        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }

    private static string Output(string fileName, params (string Key, string Value)[] values)
    {
        var template = Read(fileName);
        if (string.IsNullOrWhiteSpace(template))
            return $"Prompt template {fileName} not found.";
        foreach (var (key, value) in values)
            template = template.Replace($"{{{{{key}}}}}", value);
        return template;
    }

    /// <summary>
    /// Gets the general stock-control assistant prompt.
    /// </summary>
    [McpServerPrompt(Name = "stock_assistant", Title = "Stock assistant")]
    [Description("General stock-control assistant behaviour")]
    public string StockAssistant() =>
        Output("stock_assistant.md");

    /// <summary>
    /// Gets the stock demo setup prompt.
    /// </summary>
    [McpServerPrompt(Name = "setup_demo", Title = "Set up demo")]
    [Description("Creates the sample stock products used in the live demo")]
    public string SetupDemoStock() =>
        Output("setup_demo.md");

    /// <summary>
    /// Gets guidance for resolving products and validating requests before tools are called.
    /// </summary>
    [McpServerPrompt(Name = "before_calling_tools", Title = "Before calling stock tools")]
    [Description("Guidance for resolving products and validating requests before calling stock tools")]
    public string BeforeCallingTools() =>
        Output("before_calling_tools.md");

    /// <summary>
    /// Gets the stock controller persona prompt.
    /// </summary>
    [McpServerPrompt(Name = "stock_controller_persona", Title = "Stock controller persona")]
    [Description("Persona prompt for stock accuracy, low-stock checks, and operational next actions")]
    public string StockControllerPersona() =>
        Output("stock_controller_persona.md");

    /// <summary>
    /// Gets the sales administrator persona prompt.
    /// </summary>
    [McpServerPrompt(Name = "sales_admin_persona", Title = "Sales admin persona")]
    [Description("Persona prompt for sale recording, availability checks, and sales history")]
    public string SalesAdminPersona() =>
        Output("sales_admin_persona.md");

    /// <summary>
    /// Gets the warehouse manager persona prompt.
    /// </summary>
    [McpServerPrompt(Name = "warehouse_manager_persona", Title = "Warehouse manager persona")]
    [Description("Persona prompt for goods received, damaged stock, and stock counts")]
    public string WarehouseManagerPersona() =>
        Output("warehouse_manager_persona.md");

    /// <summary>
    /// Gets guidance for collecting product creation details before calling the create tool.
    /// </summary>
    [McpServerPrompt(Name = "create_product", Title = "Create product")]
    [Description("Guidance for collecting required product creation fields before calling create_product")]
    public string CreateProductPrompt(
        [Description("The product description or request to turn into a product")] string request) =>
        Output("create_product.md", ("request", request));

    /// <summary>
    /// Gets guidance for summarising sales history for a product.
    /// </summary>
    [McpServerPrompt(Name = "sales_history", Title = "Sales history")]
    [Description("Guidance for summarising sales history over an optional date range")]
    public string SalesHistory([Description("The product code to report on")] string productCode) =>
        Output("sales_history.md", ("productCode", productCode));

    /// <summary>
    /// Gets response-formatting guidance for demo-friendly output.
    /// </summary>
    [McpServerPrompt(Name = "expected_output", Title = "Expected stock output")]
    [Description("Guidance for concise, business-friendly stock-control response formatting")]
    public string ExpectedOutput() =>
        Output("expected_output.md");

    /// <summary>
    /// Gets guidance for explaining common stock-control error paths.
    /// </summary>
    [McpServerPrompt(Name = "error_paths", Title = "Stock error paths")]
    [Description("Guidance for clear stock-control error handling")]
    public string ErrorPaths() =>
        Output("error_paths.md");

    /// <summary>
    /// Gets guidance for producing a business-facing stock summary.
    /// </summary>
    [McpServerPrompt(Name = "executive_stock_summary", Title = "Executive stock summary")]
    [Description("Create a short business-friendly stock-control summary")]
    public string ExecutiveStockSummary() =>
        Output("executive_stock_summary.md");
}
