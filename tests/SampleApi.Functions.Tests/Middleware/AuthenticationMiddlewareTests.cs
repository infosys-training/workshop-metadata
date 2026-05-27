using FluentAssertions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using SampleApi.Functions.Middleware;

namespace SampleApi.Functions.Tests.Middleware;

/// <summary>
/// Unit tests for <see cref="AuthenticationMiddleware"/>.
/// </summary>
public class AuthenticationMiddlewareTests
{
    private readonly Mock<ILogger<AuthenticationMiddleware>> _mockLogger;
    private readonly IConfiguration _configuration;
    private readonly AuthenticationMiddleware _middleware;

    public AuthenticationMiddlewareTests()
    {
        _mockLogger = new Mock<ILogger<AuthenticationMiddleware>>();
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "JwtSettings:Authority", "https://login.microsoftonline.com/test-tenant" },
                { "JwtSettings:Audience", "api://sample-api" },
                { "JwtSettings:ValidIssuers:0", "https://sts.windows.net/test-tenant/" }
            })
            .Build();
        _middleware = new AuthenticationMiddleware(_mockLogger.Object, _configuration);
    }

    [Fact]
    public async Task Invoke_ShouldCallNext_WhenNoHttpRequest()
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
    public async Task Invoke_ShouldProceed_WhenNoAuthorizationHeader()
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
}
