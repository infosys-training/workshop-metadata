using System.Net;
using FluentAssertions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Moq;
using SampleApi.Functions.Middleware;

namespace SampleApi.Functions.Tests.Middleware;

/// <summary>
/// Unit tests for <see cref="ExceptionHandlingMiddleware"/>.
/// </summary>
public class ExceptionHandlingMiddlewareTests
{
    private readonly Mock<ILogger<ExceptionHandlingMiddleware>> _mockLogger;
    private readonly ExceptionHandlingMiddleware _middleware;

    public ExceptionHandlingMiddlewareTests()
    {
        _mockLogger = new Mock<ILogger<ExceptionHandlingMiddleware>>();
        _middleware = new ExceptionHandlingMiddleware(_mockLogger.Object);
    }

    [Fact]
    public async Task Invoke_ShouldCallNext_WhenNoException()
    {
        var context = new Mock<FunctionContext>();
        context.Setup(c => c.Items).Returns(new Dictionary<object, object>());
        context.Setup(c => c.Features).Returns(new Mock<IInvocationFeatures>().Object);
        var nextCalled = false;

        await _middleware.Invoke(context.Object, _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public void Middleware_ShouldBeInstantiable()
    {
        _middleware.Should().NotBeNull();
    }

    [Fact]
    public async Task Invoke_ShouldNotThrow_WhenKeyNotFoundExceptionAndNoHttpContext()
    {
        var context = new Mock<FunctionContext>();
        context.Setup(c => c.Items).Returns(new Dictionary<object, object>());
        context.Setup(c => c.Features).Returns(new Mock<IInvocationFeatures>().Object);

        var act = () => _middleware.Invoke(context.Object, _ =>
            throw new KeyNotFoundException("Product not found"));

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Invoke_ShouldNotThrow_WhenUnauthorizedAndNoHttpContext()
    {
        var context = new Mock<FunctionContext>();
        context.Setup(c => c.Items).Returns(new Dictionary<object, object>());
        context.Setup(c => c.Features).Returns(new Mock<IInvocationFeatures>().Object);

        var act = () => _middleware.Invoke(context.Object, _ =>
            throw new UnauthorizedAccessException("Unauthorized"));

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Invoke_ShouldNotThrow_WhenArgumentExceptionAndNoHttpContext()
    {
        var context = new Mock<FunctionContext>();
        context.Setup(c => c.Items).Returns(new Dictionary<object, object>());
        context.Setup(c => c.Features).Returns(new Mock<IInvocationFeatures>().Object);

        var act = () => _middleware.Invoke(context.Object, _ =>
            throw new ArgumentException("Bad argument"));

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Invoke_ShouldNotThrow_WhenGenericExceptionAndNoHttpContext()
    {
        var context = new Mock<FunctionContext>();
        context.Setup(c => c.Items).Returns(new Dictionary<object, object>());
        context.Setup(c => c.Features).Returns(new Mock<IInvocationFeatures>().Object);

        var act = () => _middleware.Invoke(context.Object, _ =>
            throw new InvalidOperationException("Something went wrong"));

        await act.Should().NotThrowAsync();
    }
}
