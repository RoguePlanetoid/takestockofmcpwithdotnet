namespace Stock.Mcp.Models;

/// <summary>
/// Represents a stock-controlled product in the demo catalogue.
/// </summary>
public class Product
{
    /// <summary>
    /// Gets or sets the unique product code, such as BP123.
    /// </summary>
    [Required]
    [StringLength(32)]
    public string? ProductCode { get; set; }

    /// <summary>
    /// Gets or sets the human-readable product description.
    /// </summary>
    [Required]
    [StringLength(255)]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the product cost price.
    /// </summary>
    [Range(0, double.MaxValue)]
    public decimal CostPrice { get; set; }

    /// <summary>
    /// Gets or sets the product selling price.
    /// </summary>
    [Range(0, double.MaxValue)]
    public decimal SellPrice { get; set; }

    /// <summary>
    /// Gets or sets the current quantity in stock.
    /// </summary>
    [Range(0, int.MaxValue)]
    public int QuantityStock { get; set; }
}
