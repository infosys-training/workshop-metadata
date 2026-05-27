using SampleApi.DTOs.Requests;
using SampleApi.DTOs.Responses;
using SampleApi.Entities.Models;
using SampleApi.Repository.Interfaces;
using SampleApi.Services.Interfaces;
using SampleApi.Services.ResponseShaping;

namespace SampleApi.Services.Implementations;

/// <summary>
/// Service implementation for product operations.
/// </summary>
public class ProductService : IProductService
{
    private readonly IRepository<Product> _repository;
    private readonly IResponseShaperFactory<Product> _responseShaperFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductService"/> class.
    /// </summary>
    /// <param name="repository">The product repository.</param>
    /// <param name="responseShaperFactory">The response shaper factory.</param>
    public ProductService(
        IRepository<Product> repository,
        IResponseShaperFactory<Product> responseShaperFactory)
    {
        _repository = repository;
        _responseShaperFactory = responseShaperFactory;
    }

    /// <inheritdoc />
    public async Task<ProductResponseFull?> GetByIdAsync(int id)
    {
        var product = await _repository.GetByIdAsync(id);
        if (product is null)
        {
            return null;
        }

        return MapToFullResponse(product);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<object>> GetAllAsync(string consumerKey)
    {
        var products = await _repository.GetAllAsync();
        var shaper = _responseShaperFactory.GetShaper(consumerKey);
        return shaper.ShapeCollection(products);
    }

    /// <inheritdoc />
    public async Task<ProductResponseFull> CreateAsync(CreateProductRequest request)
    {
        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            CategoryId = request.CategoryId,
            CreatedDate = DateTime.UtcNow
        };

        var created = await _repository.AddAsync(product);
        return MapToFullResponse(created);
    }

    /// <inheritdoc />
    public async Task<ProductResponseFull> UpdateAsync(int id, UpdateProductRequest request)
    {
        var product = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Product with ID {id} not found.");

        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.CategoryId = request.CategoryId;
        product.UpdatedDate = DateTime.UtcNow;

        await _repository.UpdateAsync(product);
        return MapToFullResponse(product);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(int id)
    {
        var product = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Product with ID {id} not found.");

        await _repository.DeleteAsync(id);
    }

    private static ProductResponseFull MapToFullResponse(Product product)
    {
        return new ProductResponseFull
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name,
            CreatedBy = product.CreatedBy,
            CreatedDate = product.CreatedDate,
            UpdatedBy = product.UpdatedBy,
            UpdatedDate = product.UpdatedDate
        };
    }
}
