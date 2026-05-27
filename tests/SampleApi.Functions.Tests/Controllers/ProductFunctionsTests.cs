using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SampleApi.DTOs.Requests;
using SampleApi.DTOs.Responses;
using SampleApi.Functions.Controllers;
using SampleApi.Functions.Middleware;
using SampleApi.Services.Interfaces;

namespace SampleApi.Functions.Tests.Controllers;

/// <summary>
/// Unit tests for <see cref="ProductFunctions"/>.
/// </summary>
public class ProductFunctionsTests
{
    private readonly Mock<IProductService> _mockProductService;
    private readonly Mock<ILogger<ProductFunctions>> _mockLogger;
    private readonly ProductFunctions _functions;

    public ProductFunctionsTests()
    {
        _mockProductService = new Mock<IProductService>();
        _mockLogger = new Mock<ILogger<ProductFunctions>>();
        _functions = new ProductFunctions(_mockProductService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAll_ShouldReturnOkWithProducts()
    {
        var products = new List<object>
        {
            new ProductResponseFull { Id = 1, Name = "Product 1" },
            new ProductResponseFull { Id = 2, Name = "Product 2" }
        };
        _mockProductService.Setup(s => s.GetAllAsync(It.IsAny<string>())).ReturnsAsync(products);

        var context = CreateMockFunctionContext();
        var request = CreateMockHttpRequest(context.Object);

        var result = await _functions.GetAll(request.Object, context.Object);

        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenProductExists()
    {
        var product = new ProductResponseFull
        {
            Id = 1,
            Name = "Test Product",
            Price = 19.99m
        };
        _mockProductService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(product);

        var context = CreateMockFunctionContext();
        var request = CreateMockHttpRequest(context.Object);

        var result = await _functions.GetById(request.Object, 1);

        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenProductNotExists()
    {
        _mockProductService.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((ProductResponseFull?)null);

        var context = CreateMockFunctionContext();
        var request = CreateMockHttpRequest(context.Object);

        var result = await _functions.GetById(request.Object, 999);

        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_ShouldReturnCreated_WhenValidRequest()
    {
        var createRequest = new CreateProductRequest
        {
            Name = "New Product",
            Description = "Description",
            Price = 29.99m,
            CategoryId = 1
        };
        var createdProduct = new ProductResponseFull
        {
            Id = 1,
            Name = "New Product",
            Price = 29.99m
        };
        _mockProductService.Setup(s => s.CreateAsync(It.IsAny<CreateProductRequest>()))
            .ReturnsAsync(createdProduct);

        var context = CreateMockFunctionContext();
        var request = CreateMockHttpRequestWithBody(context.Object, createRequest);

        var result = await _functions.Create(request.Object);

        result.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenNullBody()
    {
        var context = CreateMockFunctionContext();
        var request = CreateMockHttpRequestWithNullBody(context.Object);

        var result = await _functions.Create(request.Object);

        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_ShouldReturnOk_WhenValidRequest()
    {
        var updateRequest = new UpdateProductRequest
        {
            Name = "Updated Product",
            Description = "Updated",
            Price = 39.99m,
            CategoryId = 1
        };
        var updatedProduct = new ProductResponseFull
        {
            Id = 1,
            Name = "Updated Product",
            Price = 39.99m
        };
        _mockProductService.Setup(s => s.UpdateAsync(1, It.IsAny<UpdateProductRequest>()))
            .ReturnsAsync(updatedProduct);

        var context = CreateMockFunctionContext();
        var request = CreateMockHttpRequestWithBody(context.Object, updateRequest);

        var result = await _functions.Update(request.Object, 1);

        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Update_ShouldReturnBadRequest_WhenNullBody()
    {
        var context = CreateMockFunctionContext();
        var request = CreateMockHttpRequestWithNullBody(context.Object);

        var result = await _functions.Update(request.Object, 1);

        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Delete_ShouldReturnOk_WhenProductExists()
    {
        _mockProductService.Setup(s => s.DeleteAsync(1)).Returns(Task.CompletedTask);

        var context = CreateMockFunctionContext();
        var request = CreateMockHttpRequest(context.Object);

        var result = await _functions.Delete(request.Object, 1);

        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task HealthCheck_ShouldReturnOk()
    {
        var context = CreateMockFunctionContext();
        var request = CreateMockHttpRequest(context.Object);

        var result = await _functions.HealthCheck(request.Object);

        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private static Mock<FunctionContext> CreateMockFunctionContext()
    {
        var context = new Mock<FunctionContext>();
        var items = new Dictionary<object, object>
        {
            { ConsumerResolverMiddleware.ConsumerKeyItemName, "default" }
        };
        context.Setup(c => c.Items).Returns(items);

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddOptions<WorkerOptions>().Configure(options =>
        {
            options.Serializer = new Azure.Core.Serialization.JsonObjectSerializer();
        });
        var serviceProvider = serviceCollection.BuildServiceProvider();
        context.Setup(c => c.InstanceServices).Returns(serviceProvider);

        return context;
    }

    private static Mock<HttpRequestData> CreateMockHttpRequest(FunctionContext context)
    {
        var request = new Mock<HttpRequestData>(context);
        var response = new TestHttpResponseData(context);
        request.Setup(r => r.CreateResponse()).Returns(response);
        request.Setup(r => r.Headers).Returns(new HttpHeadersCollection());
        return request;
    }

    private static Mock<HttpRequestData> CreateMockHttpRequestWithBody<T>(FunctionContext context, T body)
    {
        var request = new Mock<HttpRequestData>(context);
        var response = new TestHttpResponseData(context);
        request.Setup(r => r.CreateResponse()).Returns(response);
        request.Setup(r => r.Headers).Returns(new HttpHeadersCollection());

        var json = JsonSerializer.Serialize(body);
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        request.Setup(r => r.Body).Returns(stream);

        return request;
    }

    private static Mock<HttpRequestData> CreateMockHttpRequestWithNullBody(FunctionContext context)
    {
        var request = new Mock<HttpRequestData>(context);
        var response = new TestHttpResponseData(context);
        request.Setup(r => r.CreateResponse()).Returns(response);
        request.Setup(r => r.Headers).Returns(new HttpHeadersCollection());

        var stream = new MemoryStream(Encoding.UTF8.GetBytes("null"));
        request.Setup(r => r.Body).Returns(stream);

        return request;
    }
}

/// <summary>
/// Test implementation of <see cref="HttpResponseData"/> for unit testing.
/// </summary>
public class TestHttpResponseData : HttpResponseData
{
    public TestHttpResponseData(FunctionContext functionContext) : base(functionContext)
    {
        Headers = new HttpHeadersCollection();
        Body = new MemoryStream();
    }

    public override HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
    public override HttpHeadersCollection Headers { get; set; }
    public override Stream Body { get; set; }
    public override HttpCookies Cookies { get; } = null!;
}
