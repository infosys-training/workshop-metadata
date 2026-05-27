using System.IdentityModel.Tokens.Jwt;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SampleApi.DTOs.Responses;

namespace SampleApi.Functions.Middleware;

/// <summary>
/// Middleware for validating JWT Bearer tokens on incoming requests.
/// </summary>
public class AuthenticationMiddleware : IFunctionsWorkerMiddleware
{
    private readonly ILogger<AuthenticationMiddleware> _logger;
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationMiddleware"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="configuration">The application configuration.</param>
    public AuthenticationMiddleware(
        ILogger<AuthenticationMiddleware> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Invokes the middleware, validating the JWT token if present.
    /// </summary>
    /// <param name="context">The function context.</param>
    /// <param name="next">The next middleware in the pipeline.</param>
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        HttpRequestData? httpReqData;
        try
        {
            httpReqData = await context.GetHttpRequestDataAsync();
        }
        catch (NullReferenceException)
        {
            await next(context);
            return;
        }

        if (httpReqData is null)
        {
            await next(context);
            return;
        }

        var authHeader = httpReqData.Headers
            .FirstOrDefault(h => h.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase));

        if (authHeader.Value is null || !authHeader.Value.Any())
        {
            await next(context);
            return;
        }

        var token = authHeader.Value.First();
        if (string.IsNullOrWhiteSpace(token))
        {
            await next(context);
            return;
        }

        if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            token = token["Bearer ".Length..].Trim();
        }

        try
        {
            var authority = _configuration["JwtSettings:Authority"];
            var audience = _configuration["JwtSettings:Audience"];
            var validIssuers = _configuration.GetSection("JwtSettings:ValidIssuers").Get<string[]>();

            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = !string.IsNullOrEmpty(authority),
                ValidIssuers = validIssuers ?? Array.Empty<string>(),
                ValidateAudience = !string.IsNullOrEmpty(audience),
                ValidAudience = audience,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = false,
                SignatureValidator = (tokenString, _) => new JwtSecurityToken(tokenString)
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
            context.Items["User"] = principal;
        }
        catch (SecurityTokenException ex)
        {
            _logger.LogWarning(ex, "Token validation failed");
            var response = httpReqData.CreateResponse(HttpStatusCode.Unauthorized);
            var errorResponse = new ApiResponse<object>
            {
                Success = false,
                Message = "Invalid or expired token.",
                Data = null
            };
            await response.WriteAsJsonAsync(errorResponse);
            context.GetInvocationResult().Value = response;
            return;
        }

        await next(context);
    }
}
