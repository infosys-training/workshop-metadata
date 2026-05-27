using SampleApi.DTOs.Responses;
using SampleApi.Entities.Models;

namespace SampleApi.Services.ResponseShaping;

/// <summary>
/// Response shaper that returns the complete product response with all fields.
/// </summary>
public class FullResponseShaper : IResponseShaper<Product>
{
    /// <inheritdoc />
    public object Shape(Product entity)
    {
        return new ProductResponseFull
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            Price = entity.Price,
            CategoryId = entity.CategoryId,
            CategoryName = entity.Category?.Name,
            CreatedBy = entity.CreatedBy,
            CreatedDate = entity.CreatedDate,
            UpdatedBy = entity.UpdatedBy,
            UpdatedDate = entity.UpdatedDate
        };
    }

    /// <inheritdoc />
    public IEnumerable<object> ShapeCollection(IEnumerable<Product> entities)
    {
        return entities.Select(Shape);
    }
}
