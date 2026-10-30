namespace Stock.Mcp.Models;

/// <summary>
/// Represents a recorded product sale that reduces stock.
/// </summary>
public class Sale
{
    /// <summary>
    /// Gets or sets the database-generated sale identifier.
    /// </summary>
    public int? Id { get; set; }

    /// <summary>
    /// Gets or sets the sale date.
    /// </summary>
    [Required]
    public DateTimeOffset Date { get; set; }

    /// <summary>
    /// Gets or sets the product code sold.
    /// </summary>
    [Required]
    [StringLength(32)]
    public string? ProductCode { get; set; }

    /// <summary>
    /// Gets or sets the quantity sold.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int QuantitySold { get; set; }
}
