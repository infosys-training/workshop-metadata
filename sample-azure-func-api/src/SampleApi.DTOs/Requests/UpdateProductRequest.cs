using System.ComponentModel.DataAnnotations;

namespace SampleApi.DTOs.Requests;

/// <summary>
/// Request DTO for updating an existing product.
/// </summary>
public class UpdateProductRequest
{
    /// <summary>
    /// Gets or sets the product name. Required, maximum 200 characters.
    /// </summary>
    [Required(ErrorMessage = "Product name is required.")]
    [StringLength(200, ErrorMessage = "Product name cannot exceed 200 characters.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the product description. Maximum 1000 characters.
    /// </summary>
    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the product price. Must be greater than zero.
    /// </summary>
    [Required(ErrorMessage = "Price is required.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
    public decimal Price { get; set; }

    /// <summary>
    /// Gets or sets the category identifier. Required.
    /// </summary>
    [Required(ErrorMessage = "Category ID is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Category ID must be a positive integer.")]
    public int CategoryId { get; set; }
}
