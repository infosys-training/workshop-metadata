namespace SampleApi.Services.ResponseShaping;

/// <summary>
/// Interface for shaping entity responses based on consumer requirements.
/// </summary>
/// <typeparam name="TEntity">The entity type to shape.</typeparam>
public interface IResponseShaper<in TEntity>
{
    /// <summary>
    /// Shapes a single entity into a consumer-specific response.
    /// </summary>
    /// <param name="entity">The entity to shape.</param>
    /// <returns>The shaped response object.</returns>
    object Shape(TEntity entity);

    /// <summary>
    /// Shapes a collection of entities into consumer-specific responses.
    /// </summary>
    /// <param name="entities">The entities to shape.</param>
    /// <returns>A collection of shaped response objects.</returns>
    IEnumerable<object> ShapeCollection(IEnumerable<TEntity> entities);
}
