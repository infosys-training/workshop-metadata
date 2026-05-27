using SampleApi.DTOs.Responses;
using SampleApi.Entities.Models;

namespace SampleApi.Services.ResponseShaping;

/// <summary>
/// Response shaper that returns a limited product response with fewer fields.
/// </summary>
public class LimitedResponseShaper : IResponseShaper<Product>
{
    /// <inheritdoc />
    public object Shape(Product entity)
    {
        return new ProductResponseLimited
        {
            Id = entity.Id,
            Name = entity.Name,
            Price = entity.Price,
            CategoryName = entity.Category?.Name
        };
    }

    /// <inheritdoc />
    public IEnumerable<object> ShapeCollection(IEnumerable<Product> entities)
    {
        return entities.Select(Shape);
    }
}
