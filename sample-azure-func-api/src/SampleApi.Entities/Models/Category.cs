using SampleApi.Entities.Base;

namespace SampleApi.Entities.Models;

/// <summary>
/// Represents a product category in the system.
/// </summary>
public class Category : BaseEntity
{
    /// <summary>
    /// Gets or sets the name of the category.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the category.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the collection of products belonging to this category.
    /// </summary>
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
