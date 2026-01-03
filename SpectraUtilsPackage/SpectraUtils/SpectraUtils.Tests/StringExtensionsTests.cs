using SpectraUtils.Extensions;
using Xunit;

namespace SpectraUtils.Tests;

public sealed class StringExtensionsTests
{
    [Theory]
    [InlineData(null, true)]
    [InlineData("", true)]
    [InlineData("text", false)]
    public void IsNullOrEmpty_Works(string? input, bool expected)
    {
        Assert.Equal(expected, input.IsNullOrEmpty());
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData("", true)]
    [InlineData("   ", true)]
    [InlineData("text", false)]
    public void IsNullOrWhiteSpace_Works(string? input, bool expected)
    {
        Assert.Equal(expected, input.IsNullOrWhiteSpace());
    }

    [Fact]
    public void ReverseSafe_Null_ReturnsNull()
    {
        string? reversed = ((string?)null).ReverseSafe();
        Assert.Null(reversed);
    }

    [Fact]
    public void ReverseSafe_SingleCharacter_ReturnsSame()
    {
        const string input = "a";
        Assert.Equal(input, input.ReverseSafe());
    }

    [Fact]
    public void ReverseSafe_ReversesString()
    {
        Assert.Equal("cba", "abc".ReverseSafe());
    }
}
