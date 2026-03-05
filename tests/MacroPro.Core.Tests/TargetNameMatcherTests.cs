using MacroPro.Core.Targeting;
using Xunit;

namespace MacroPro.Core.Tests;

public sealed class TargetNameMatcherTests
{
    [Fact]
    public void IsMatch_ReturnsTrue_WhenAllowedListIsEmpty()
    {
        var result = TargetNameMatcher.IsMatch("Any Monster", new List<string>(), requireNameMatch: false, out var matchedByName);
        Assert.True(result);
        Assert.False(matchedByName);
    }

    [Fact]
    public void IsMatch_ReturnsTrue_WhenNameContainsAllowedToken()
    {
        var allowed = new List<string> { "skeleton prisoner", "whisper" };
        var result = TargetNameMatcher.IsMatch("Skeleton  Prisoner", allowed, requireNameMatch: true, out var matchedByName);
        Assert.True(result);
        Assert.True(matchedByName);
    }

    [Fact]
    public void IsMatch_ReturnsFalse_WhenRequireNameMatchAndNoName()
    {
        var allowed = new List<string> { "hunter fly" };
        var result = TargetNameMatcher.IsMatch(null, allowed, requireNameMatch: true, out _);
        Assert.False(result);
    }
}
