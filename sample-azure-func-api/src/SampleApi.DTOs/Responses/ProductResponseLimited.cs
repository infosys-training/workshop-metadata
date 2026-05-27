namespace SampleApi.DTOs.Responses;

/// <summary>
/// Limited product response DTO containing only essential product fields.
/// </summary>
public class ProductResponseLimited
{
    /// <summary>
    /// Gets or sets the product identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the product name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the product price.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Gets or sets the category name.
    /// </summary>
    public string? CategoryName { get; set; }
}
