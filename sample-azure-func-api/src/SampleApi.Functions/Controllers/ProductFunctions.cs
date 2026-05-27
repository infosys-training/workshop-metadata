using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using SampleApi.DTOs.Requests;
using SampleApi.DTOs.Responses;
using SampleApi.Functions.Middleware;
using SampleApi.Services.Interfaces;

namespace SampleApi.Functions.Controllers;

/// <summary>
/// Azure Functions HTTP triggers for product CRUD operations.
/// </summary>
public class ProductFunctions
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductFunctions> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductFunctions"/> class.
    /// </summary>
    /// <param name="productService">The product service.</param>
    /// <param name="logger">The logger instance.</param>
    public ProductFunctions(IProductService productService, ILogger<ProductFunctions> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all products. Response shape depends on the consumer key.
    /// </summary>
    /// <param name="req">The HTTP request data.</param>
    /// <param name="context">The function execution context.</param>
    /// <returns>A list of shaped product responses.</returns>
    [Function("GetProducts")]
    [OpenApiOperation(operationId: "GetProducts", tags: new[] { "Products" }, Summary = "Get all products", Description = "Retrieves all products with consumer-based response shaping.")]
    [OpenApiParameter(name: "X-Consumer-Key", In = ParameterLocation.Header, Required = false, Type = typeof(string), Description = "Consumer key for response shaping")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ApiResponse<IEnumerable<object>>), Description = "List of products")]
    public async Task<HttpResponseData> GetAll(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "products")] HttpRequestData req,
        FunctionContext context)
    {
        _logger.LogInformation("Getting all products");

        var consumerKey = GetConsumerKey(context);
        var products = await _productService.GetAllAsync(consumerKey);

        var response = req.CreateResponse();
        await response.WriteAsJsonAsync(new ApiResponse<IEnumerable<object>>
        {
            Success = true,
            Message = "Products retrieved successfully.",
            Data = products
        });
        response.StatusCode = HttpStatusCode.OK;

        return response;
    }

    /// <summary>
    /// Retrieves a product by its identifier.
    /// </summary>
    /// <param name="req">The HTTP request data.</param>
    /// <param name="id">The product identifier.</param>
    /// <returns>The product details.</returns>
    [Function("GetProductById")]
    [OpenApiOperation(operationId: "GetProductById", tags: new[] { "Products" }, Summary = "Get product by ID", Description = "Retrieves a single product by its identifier.")]
    [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(int), Description = "Product ID")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ApiResponse<ProductResponseFull>), Description = "Product details")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, contentType: "application/json", bodyType: typeof(ApiResponse<object>), Description = "Product not found")]
    public async Task<HttpResponseData> GetById(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "products/{id:int}")] HttpRequestData req,
        int id)
    {
        _logger.LogInformation("Getting product with ID: {Id}", id);

        var product = await _productService.GetByIdAsync(id);
        if (product is null)
        {
            var notFoundResponse = req.CreateResponse();
            await notFoundResponse.WriteAsJsonAsync(new ApiResponse<object>
            {
                Success = false,
                Message = $"Product with ID {id} not found.",
                Data = null
            });
            notFoundResponse.StatusCode = HttpStatusCode.NotFound;
            return notFoundResponse;
        }

        var response = req.CreateResponse();
        await response.WriteAsJsonAsync(new ApiResponse<ProductResponseFull>
        {
            Success = true,
            Message = "Product retrieved successfully.",
            Data = product
        });
        response.StatusCode = HttpStatusCode.OK;

        return response;
    }

    /// <summary>
    /// Creates a new product.
    /// </summary>
    /// <param name="req">The HTTP request data.</param>
    /// <returns>The created product.</returns>
    [Function("CreateProduct")]
    [OpenApiOperation(operationId: "CreateProduct", tags: new[] { "Products" }, Summary = "Create a product", Description = "Creates a new product.")]
    [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(CreateProductRequest), Required = true, Description = "Product data")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Created, contentType: "application/json", bodyType: typeof(ApiResponse<ProductResponseFull>), Description = "Created product")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(ApiResponse<object>), Description = "Validation error")]
    public async Task<HttpResponseData> Create(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "products")] HttpRequestData req)
    {
        _logger.LogInformation("Creating new product");

        var request = await req.ReadFromJsonAsync<CreateProductRequest>();
        if (request is null)
        {
            var badResponse = req.CreateResponse();
            await badResponse.WriteAsJsonAsync(new ApiResponse<object>
            {
                Success = false,
                Message = "Invalid request body.",
                Data = null
            });
            badResponse.StatusCode = HttpStatusCode.BadRequest;
            return badResponse;
        }

        var product = await _productService.CreateAsync(request);
        var response = req.CreateResponse();
        await response.WriteAsJsonAsync(new ApiResponse<ProductResponseFull>
        {
            Success = true,
            Message = "Product created successfully.",
            Data = product
        });
        response.StatusCode = HttpStatusCode.Created;

        return response;
    }

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    /// <param name="req">The HTTP request data.</param>
    /// <param name="id">The product identifier.</param>
    /// <returns>The updated product.</returns>
    [Function("UpdateProduct")]
    [OpenApiOperation(operationId: "UpdateProduct", tags: new[] { "Products" }, Summary = "Update a product", Description = "Updates an existing product by its identifier.")]
    [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(int), Description = "Product ID")]
    [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(UpdateProductRequest), Required = true, Description = "Updated product data")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ApiResponse<ProductResponseFull>), Description = "Updated product")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, contentType: "application/json", bodyType: typeof(ApiResponse<object>), Description = "Product not found")]
    public async Task<HttpResponseData> Update(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "products/{id:int}")] HttpRequestData req,
        int id)
    {
        _logger.LogInformation("Updating product with ID: {Id}", id);

        var request = await req.ReadFromJsonAsync<UpdateProductRequest>();
        if (request is null)
        {
            var badResponse = req.CreateResponse();
            await badResponse.WriteAsJsonAsync(new ApiResponse<object>
            {
                Success = false,
                Message = "Invalid request body.",
                Data = null
            });
            badResponse.StatusCode = HttpStatusCode.BadRequest;
            return badResponse;
        }

        var product = await _productService.UpdateAsync(id, request);
        var response = req.CreateResponse();
        await response.WriteAsJsonAsync(new ApiResponse<ProductResponseFull>
        {
            Success = true,
            Message = "Product updated successfully.",
            Data = product
        });
        response.StatusCode = HttpStatusCode.OK;

        return response;
    }

    /// <summary>
    /// Deletes a product by its identifier.
    /// </summary>
    /// <param name="req">The HTTP request data.</param>
    /// <param name="id">The product identifier.</param>
    /// <returns>A success response.</returns>
    [Function("DeleteProduct")]
    [OpenApiOperation(operationId: "DeleteProduct", tags: new[] { "Products" }, Summary = "Delete a product", Description = "Deletes a product by its identifier.")]
    [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(int), Description = "Product ID")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ApiResponse<object>), Description = "Product deleted")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, contentType: "application/json", bodyType: typeof(ApiResponse<object>), Description = "Product not found")]
    public async Task<HttpResponseData> Delete(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "products/{id:int}")] HttpRequestData req,
        int id)
    {
        _logger.LogInformation("Deleting product with ID: {Id}", id);

        await _productService.DeleteAsync(id);

        var response = req.CreateResponse();
        await response.WriteAsJsonAsync(new ApiResponse<object>
        {
            Success = true,
            Message = "Product deleted successfully.",
            Data = null
        });
        response.StatusCode = HttpStatusCode.OK;

        return response;
    }

    /// <summary>
    /// Health check endpoint.
    /// </summary>
    /// <param name="req">The HTTP request data.</param>
    /// <returns>A healthy response.</returns>
    [Function("HealthCheck")]
    [OpenApiOperation(operationId: "HealthCheck", tags: new[] { "Health" }, Summary = "Health check", Description = "Returns the health status of the API.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ApiResponse<string>), Description = "API is healthy")]
    public async Task<HttpResponseData> HealthCheck(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health")] HttpRequestData req)
    {
        var response = req.CreateResponse();
        await response.WriteAsJsonAsync(new ApiResponse<string>
        {
            Success = true,
            Message = "API is healthy.",
            Data = "Healthy"
        });
        response.StatusCode = HttpStatusCode.OK;

        return response;
    }

    private static string GetConsumerKey(FunctionContext context)
    {
        if (context.Items.TryGetValue(ConsumerResolverMiddleware.ConsumerKeyItemName, out var consumerKeyObj)
            && consumerKeyObj is string consumerKey)
        {
            return consumerKey;
        }

        return ConsumerResolverMiddleware.DefaultConsumerKey;
    }
}
