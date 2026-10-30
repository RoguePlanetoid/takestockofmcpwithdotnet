
namespace Stock.Mcp.Resources;

/// <summary>
/// Markdown sample resources for stock-control demos.
/// </summary>
[McpServerResourceType]
public class StockResources
{
    private const string extension = ".md";
    private static readonly Assembly assembly = typeof(StockResources).Assembly;
    private static readonly string prefix = $"{typeof(StockResources).Namespace}.Samples.";

    private static readonly Dictionary<string, string> resources =
        assembly.GetManifestResourceNames()
            .Where(name => name.StartsWith(prefix, StringComparison.Ordinal))
            .ToDictionary(
                name => name[prefix.Length..^extension.Length],
                name => name,
                StringComparer.OrdinalIgnoreCase);

    private static string Read(string name)
    {
        using var stream = assembly.GetManifestResourceStream(name) ?? 
            throw new McpProtocolException("The embedded stock sample could not be loaded.", McpErrorCode.InternalError);
        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// Gets the index of embedded stock sample resource URIs.
    /// </summary>
    [McpServerResource(UriTemplate = "stock://samples", Name = "stock_samples_index_markdown",
        Title = "Stock samples index (Markdown)", MimeType = "text/markdown")]
    [Description("List stock-control sample resources")]
    public string SamplesIndex()
    {
        if (resources.Count == 0)
            throw new McpProtocolException("No embedded stock samples were found.", McpErrorCode.InternalError);

        var sampleNames = resources.Keys.OrderBy(name => name, StringComparer.OrdinalIgnoreCase);
        return "# Stock samples\n\n" + string.Join(
            Environment.NewLine,
            sampleNames.Select(name => $"- stock://samples/{name}"));
    }

    /// <summary>
    /// Gets an embedded stock sample resource by name.
    /// </summary>
    /// <param name="name">Sample name with or without the .md extension.</param>
    /// <returns>The sample Markdown content.</returns>
    /// <exception cref="McpProtocolException">The sample is unknown or cannot be loaded.</exception>
    [McpServerResource(UriTemplate = "stock://samples/{name}", Name = "stock_sample_markdown",
        Title = "Stock sample (Markdown)", MimeType = "text/markdown")]
    [Description("Get a stock-control sample markdown resource")]
    public string SampleMarkdown([Description("Sample name from embedded resources, with or without .md")] string name)
    {
        var sampleName = name.EndsWith(extension, StringComparison.OrdinalIgnoreCase)
            ? name[..^extension.Length]
            : name;
        if (!resources.TryGetValue(sampleName, out var resourceName))
            throw new McpProtocolException(
                $"Unknown sample '{name}'. Use stock://samples to list available samples.",
                McpErrorCode.InvalidParams);

        return Read(resourceName);
    }
}
