using SpectraUtils.Extensions;
using System.Text;
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
    public void IsNullOrEmpty_FalseResult_ProvesNotNullToTheCompiler()
    {
        string? value = "abc";

        // Compiles without a nullable warning thanks to [NotNullWhen(false)] (TreatWarningsAsErrors would catch a regression).
        int length = value.IsNullOrEmpty() ? 0 : value.Length;
        int length2 = value.IsNullOrWhiteSpace() ? 0 : value.Length;

        Assert.Equal(3, length);
        Assert.Equal(3, length2);
    }

    [Fact]
    public void ReverseSafe_Null_ReturnsNull()
    {
        Assert.Null(((string?)null).ReverseSafe());
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("a", "a")]
    [InlineData("abc", "cba")]
    [InlineData("Çağrı", "ırğaÇ")]
    [InlineData("a😀b", "b😀a")]
    [InlineData("👨‍👩‍👧x", "x👨‍👩‍👧")] // ZWJ family sequence
    [InlineData("🇹🇷!", "!🇹🇷")] // regional indicator flag
    [InlineData("line1\r\nline2", "2enil\r\n1enil")]
    public void ReverseSafe_ReversesUserPerceivedCharacters(string input, string expected)
    {
        Assert.Equal(expected, input.ReverseSafe());
    }

    [Fact]
    public void ReverseSafe_CombiningMarks_StayWithTheirBaseCharacter()
    {
        string input = "éa"; // é written as e + combining acute
        Assert.Equal("aé", input.ReverseSafe());
    }

    [Fact]
    public void ReverseSafe_ResultIsAlwaysWellFormedUtf16()
    {
        var strict = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
        string reversed = "x😀y🎉z".ReverseSafe();

        strict.GetBytes(reversed); // throws on lone surrogates
        Assert.Equal("z🎉y😀x", reversed);
    }
}
