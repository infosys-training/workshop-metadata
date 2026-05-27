using Microsoft.Extensions.Configuration;
using SampleApi.Entities.Models;

namespace SampleApi.Services.ResponseShaping;

/// <summary>
/// Factory that creates response shapers based on consumer configuration.
/// New consumers are added via configuration entries — no code changes needed
/// for existing response levels.
/// </summary>
public class ResponseShaperFactory : IResponseShaperFactory<Product>
{
    private readonly IConfiguration _configuration;
    private readonly Dictionary<string, Func<IResponseShaper<Product>>> _shaperRegistry;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResponseShaperFactory"/> class.
    /// </summary>
    /// <param name="configuration">The application configuration.</param>
    public ResponseShaperFactory(IConfiguration configuration)
    {
        _configuration = configuration;
        _shaperRegistry = new Dictionary<string, Func<IResponseShaper<Product>>>(StringComparer.OrdinalIgnoreCase)
        {
            { "Full", () => new FullResponseShaper() },
            { "Limited", () => new LimitedResponseShaper() }
        };
    }

    /// <inheritdoc />
    public IResponseShaper<Product> GetShaper(string consumerKey)
    {
        var profileSection = _configuration.GetSection($"ConsumerProfiles:{consumerKey}");
        var profile = profileSection.Get<ConsumerProfile>();

        var responseLevel = profile?.ResponseLevel ?? "Limited";

        if (_shaperRegistry.TryGetValue(responseLevel, out var shaperFactory))
        {
            return shaperFactory();
        }

        return new LimitedResponseShaper();
    }
}
