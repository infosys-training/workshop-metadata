namespace SampleApi.Services.ResponseShaping;

/// <summary>
/// Configuration model representing a consumer's response profile.
/// </summary>
public class ConsumerProfile
{
    /// <summary>
    /// Gets or sets the response level for the consumer (e.g., "Full", "Limited").
    /// </summary>
    public string ResponseLevel { get; set; } = "Limited";
}
