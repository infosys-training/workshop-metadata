using System.Security.Claims;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;

namespace SampleApi.Functions.Middleware;

/// <summary>
/// Middleware that extracts consumer identity from the X-Consumer-Key header
/// or from a JWT claim and makes it available to downstream services.
/// </summary>
public class ConsumerResolverMiddleware : IFunctionsWorkerMiddleware
{
    /// <summary>
    /// The key used to store the consumer key in the function context items.
    /// </summary>
    public const string ConsumerKeyItemName = "ConsumerKey";

    /// <summary>
    /// The HTTP header name for the consumer key.
    /// </summary>
    public const string ConsumerKeyHeaderName = "X-Consumer-Key";

    /// <summary>
    /// The JWT claim type used for consumer identification.
    /// </summary>
    public const string ConsumerClaimType = "consumer_key";

    /// <summary>
    /// The default consumer key when none is provided.
    /// </summary>
    public const string DefaultConsumerKey = "default";

    private readonly ILogger<ConsumerResolverMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConsumerResolverMiddleware"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public ConsumerResolverMiddleware(ILogger<ConsumerResolverMiddleware> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Invokes the middleware, resolving the consumer key from headers or JWT claims.
    /// </summary>
    /// <param name="context">The function context.</param>
    /// <param name="next">The next middleware in the pipeline.</param>
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var consumerKey = DefaultConsumerKey;

        HttpRequestData? httpReqData = null;
        try
        {
            httpReqData = await context.GetHttpRequestDataAsync();
        }
        catch (NullReferenceException)
        {
            // HTTP request data is not available in this context
        }

        if (httpReqData is not null)
        {
            var headerValue = httpReqData.Headers
                .FirstOrDefault(h => h.Key.Equals(ConsumerKeyHeaderName, StringComparison.OrdinalIgnoreCase));

            if (headerValue.Value is not null && headerValue.Value.Any())
            {
                consumerKey = headerValue.Value.First();
            }
            else if (context.Items.TryGetValue("User", out var userObj) && userObj is ClaimsPrincipal principal)
            {
                var claim = principal.FindFirst(ConsumerClaimType);
                if (claim is not null)
                {
                    consumerKey = claim.Value;
                }
            }
        }

        _logger.LogDebug("Resolved consumer key: {ConsumerKey}", consumerKey);
        context.Items[ConsumerKeyItemName] = consumerKey;

        await next(context);
    }
}
