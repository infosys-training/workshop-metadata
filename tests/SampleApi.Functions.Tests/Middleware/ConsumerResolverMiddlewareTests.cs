using System.Security.Claims;
using FluentAssertions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using Moq;
using SampleApi.Functions.Middleware;

namespace SampleApi.Functions.Tests.Middleware;

/// <summary>
/// Unit tests for <see cref="ConsumerResolverMiddleware"/>.
/// </summary>
public class ConsumerResolverMiddlewareTests
{
    private readonly Mock<ILogger<ConsumerResolverMiddleware>> _mockLogger;
    private readonly ConsumerResolverMiddleware _middleware;

    public ConsumerResolverMiddlewareTests()
    {
        _mockLogger = new Mock<ILogger<ConsumerResolverMiddleware>>();
        _middleware = new ConsumerResolverMiddleware(_mockLogger.Object);
    }

    [Fact]
    public async Task Invoke_ShouldSetDefaultConsumerKey_WhenNoHttpContext()
    {
        var context = new Mock<FunctionContext>();
        var items = new Dictionary<object, object>();
        context.Setup(c => c.Items).Returns(items);
        context.Setup(c => c.Features).Returns(new Mock<IInvocationFeatures>().Object);
        var nextCalled = false;

        await _middleware.Invoke(context.Object, _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        nextCalled.Should().BeTrue();
        items.Should().ContainKey(ConsumerResolverMiddleware.ConsumerKeyItemName);
        items[ConsumerResolverMiddleware.ConsumerKeyItemName].Should().Be(ConsumerResolverMiddleware.DefaultConsumerKey);
    }

    [Fact]
    public async Task Invoke_ShouldCallNext()
    {
        var context = new Mock<FunctionContext>();
        var items = new Dictionary<object, object>();
        context.Setup(c => c.Items).Returns(items);
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
    public void Constants_ShouldHaveExpectedValues()
    {
        ConsumerResolverMiddleware.ConsumerKeyItemName.Should().Be("ConsumerKey");
        ConsumerResolverMiddleware.ConsumerKeyHeaderName.Should().Be("X-Consumer-Key");
        ConsumerResolverMiddleware.ConsumerClaimType.Should().Be("consumer_key");
        ConsumerResolverMiddleware.DefaultConsumerKey.Should().Be("default");
    }

    [Fact]
    public void Middleware_ShouldBeInstantiable()
    {
        _middleware.Should().NotBeNull();
    }
}
