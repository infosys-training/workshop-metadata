namespace SampleApi.Services.ResponseShaping;

/// <summary>
/// Factory interface for creating response shapers based on consumer identity.
/// </summary>
/// <typeparam name="TEntity">The entity type to shape.</typeparam>
public interface IResponseShaperFactory<TEntity>
{
    /// <summary>
    /// Gets the appropriate response shaper for the given consumer key.
    /// </summary>
    /// <param name="consumerKey">The consumer key identifying the API consumer.</param>
    /// <returns>The response shaper configured for the consumer.</returns>
    IResponseShaper<TEntity> GetShaper(string consumerKey);
}
