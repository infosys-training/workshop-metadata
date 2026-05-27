using SampleApi.Entities.Base;

namespace SampleApi.Entities.Models;

/// <summary>
/// Represents a product in the system.
/// </summary>
public class Product : BaseEntity
{
    /// <summary>
    /// Gets or sets the name of the product.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the product.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the price of the product.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Gets or sets the foreign key to the category.
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    /// Gets or sets the category this product belongs to.
    /// </summary>
    public Category? Category { get; set; }
}
