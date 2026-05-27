using SampleApi.DTOs.Requests;
using SampleApi.DTOs.Responses;

namespace SampleApi.Services.Interfaces;

/// <summary>
/// Service interface for product operations.
/// </summary>
public interface IProductService
{
    /// <summary>
    /// Retrieves a product by its identifier.
    /// </summary>
    /// <param name="id">The product identifier.</param>
    /// <returns>The full product response if found; otherwise, null.</returns>
    Task<ProductResponseFull?> GetByIdAsync(int id);

    /// <summary>
    /// Retrieves all products, shaped according to the consumer key.
    /// </summary>
    /// <param name="consumerKey">The consumer key determining response shape.</param>
    /// <returns>A collection of shaped product responses.</returns>
    Task<IEnumerable<object>> GetAllAsync(string consumerKey);

    /// <summary>
    /// Creates a new product.
    /// </summary>
    /// <param name="request">The create product request.</param>
    /// <returns>The created product as a full response.</returns>
    Task<ProductResponseFull> CreateAsync(CreateProductRequest request);

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    /// <param name="id">The product identifier.</param>
    /// <param name="request">The update product request.</param>
    /// <returns>The updated product as a full response.</returns>
    Task<ProductResponseFull> UpdateAsync(int id, UpdateProductRequest request);

    /// <summary>
    /// Deletes a product by its identifier.
    /// </summary>
    /// <param name="id">The product identifier.</param>
    Task DeleteAsync(int id);
}
