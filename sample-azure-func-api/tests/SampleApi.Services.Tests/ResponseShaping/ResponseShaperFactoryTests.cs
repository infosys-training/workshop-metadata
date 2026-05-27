using FluentAssertions;
using Microsoft.Extensions.Configuration;
using SampleApi.Services.ResponseShaping;

namespace SampleApi.Services.Tests.ResponseShaping;

/// <summary>
/// Unit tests for <see cref="ResponseShaperFactory"/>.
/// </summary>
public class ResponseShaperFactoryTests
{
    [Fact]
    public void GetShaper_ShouldReturnFullShaper_ForFullConsumer()
    {
        var config = BuildConfiguration(new Dictionary<string, string?>
        {
            { "ConsumerProfiles:cognition:ResponseLevel", "Full" }
        });
        var factory = new ResponseShaperFactory(config);

        var shaper = factory.GetShaper("cognition");

        shaper.Should().BeOfType<FullResponseShaper>();
    }

    [Fact]
    public void GetShaper_ShouldReturnLimitedShaper_ForLimitedConsumer()
    {
        var config = BuildConfiguration(new Dictionary<string, string?>
        {
            { "ConsumerProfiles:infosys:ResponseLevel", "Limited" }
        });
        var factory = new ResponseShaperFactory(config);

        var shaper = factory.GetShaper("infosys");

        shaper.Should().BeOfType<LimitedResponseShaper>();
    }

    [Fact]
    public void GetShaper_ShouldReturnLimitedShaper_ForUnknownConsumer()
    {
        var config = BuildConfiguration(new Dictionary<string, string?>());
        var factory = new ResponseShaperFactory(config);

        var shaper = factory.GetShaper("unknown-consumer");

        shaper.Should().BeOfType<LimitedResponseShaper>();
    }

    [Fact]
    public void GetShaper_ShouldReturnLimitedShaper_ForUnknownResponseLevel()
    {
        var config = BuildConfiguration(new Dictionary<string, string?>
        {
            { "ConsumerProfiles:custom:ResponseLevel", "NonExistentLevel" }
        });
        var factory = new ResponseShaperFactory(config);

        var shaper = factory.GetShaper("custom");

        shaper.Should().BeOfType<LimitedResponseShaper>();
    }

    [Fact]
    public void GetShaper_ShouldBeCaseInsensitive_ForResponseLevel()
    {
        var config = BuildConfiguration(new Dictionary<string, string?>
        {
            { "ConsumerProfiles:test:ResponseLevel", "full" }
        });
        var factory = new ResponseShaperFactory(config);

        var shaper = factory.GetShaper("test");

        shaper.Should().BeOfType<FullResponseShaper>();
    }

    private static IConfiguration BuildConfiguration(Dictionary<string, string?> values)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }
}
