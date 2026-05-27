using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using SampleApi.DTOs.Responses;

namespace SampleApi.Functions.Middleware;

/// <summary>
/// Middleware for centralized exception handling. Returns consistent error responses.
/// </summary>
public class ExceptionHandlingMiddleware : IFunctionsWorkerMiddleware
{
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExceptionHandlingMiddleware"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Invokes the middleware, catching and handling any unhandled exceptions.
    /// </summary>
    /// <param name="context">The function context.</param>
    /// <param name="next">The next middleware in the pipeline.</param>
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Resource not found");
            await WriteErrorResponse(context, HttpStatusCode.NotFound, ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access attempt");
            await WriteErrorResponse(context, HttpStatusCode.Unauthorized, "Unauthorized");
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Bad request");
            await WriteErrorResponse(context, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");
            await WriteErrorResponse(context, HttpStatusCode.InternalServerError, "An unexpected error occurred.");
        }
    }

    private static async Task WriteErrorResponse(FunctionContext context, HttpStatusCode statusCode, string message)
    {
        try
        {
            var httpReqData = await context.GetHttpRequestDataAsync();
            if (httpReqData is not null)
            {
                var response = httpReqData.CreateResponse(statusCode);
                var errorResponse = new ApiResponse<object>
                {
                    Success = false,
                    Message = message,
                    Data = null
                };

                await response.WriteAsJsonAsync(errorResponse);
                context.GetInvocationResult().Value = response;
            }
        }
        catch (NullReferenceException)
        {
            // HTTP request data is not available in this context
        }
    }
}
