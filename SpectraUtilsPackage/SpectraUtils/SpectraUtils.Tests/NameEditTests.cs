using SpectraUtils.Concerete;
using Xunit;

namespace SpectraUtils.Tests;

public sealed class NameEditTests
{
    private readonly NameEdit _sut = new();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void NameCorrection_NullOrEmpty_ReturnsSame(string? input)
    {
        string output = _sut.NameCorrection(input);
        Assert.Equal(input, output);
    }

    [Theory]
    [InlineData("  ", "")]
    [InlineData("oÐuzHan", "Oðuzhan")]
    [InlineData(" i", "Ý")]
    [InlineData("I", "I")]
    [InlineData("ý", "I")]
    public void NameCorrection_SingleName_ProducesExpected(string input, string expected)
    {
        string output = _sut.NameCorrection(input);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void NameCorrection_Params_IgnoresNullAndEmpty_AndJoinsWithSpaces()
    {
        string output = _sut.NameCorrection("oÐuzHan", null, "", "aLi");
        Assert.Equal("Oðuzhan Ali", output);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void SirNameCorrection_NullOrEmpty_ReturnsSame(string? input)
    {
        string output = _sut.SirNameCorrection(input);
        Assert.Equal(input, output);
    }

    [Theory]
    [InlineData("kara güzel", "KARAGÜZEL")]
    [InlineData("  karagüzel  ", "KARAGÜZEL")]
    public void SirNameCorrection_RemovesSpaces_AndUppercasesTurkish(string input, string expected)
    {
        string output = _sut.SirNameCorrection(input);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void FullNameCorrection_BothNullOrEmpty_ReturnsEmpty()
    {
        string output = _sut.FullNameCorrection((string?)null, (string?)null);
        Assert.Equal(string.Empty, output);
    }

    [Fact]
    public void FullNameCorrection_OnlyName_ReturnsName()
    {
        string output = _sut.FullNameCorrection("oÐuzHan", null);
        Assert.Equal("Oðuzhan", output);
    }

    [Fact]
    public void FullNameCorrection_OnlySirName_ReturnsSirName()
    {
        string output = _sut.FullNameCorrection(null, "kara güzel");
        Assert.Equal("KARAGÜZEL", output);
    }

    [Fact]
    public void FullNameCorrection_Both_ReturnsCombined()
    {
        string output = _sut.FullNameCorrection("oÐuzHan", "kara güzel");
        Assert.Equal("Oðuzhan KARAGÜZEL", output);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void StandardizeCharacters_NullOrEmpty_ReturnsSame(string? input)
    {
        string output = _sut.StandardizeCharacters(input);
        Assert.Equal(input, output);
    }

    [Theory]
    [InlineData("oðuzhan", "oguzhan")]
    [InlineData("Ýstanbul", "Istanbul")]
    [InlineData("Çaðdaþ Þöhret", "Cagdas Sohret")]
    [InlineData("Crème Brûlée", "Creme Brulee")]
    public void StandardizeCharacters_RemovesDiacritics_AndMapsTurkish(string input, string expected)
    {
        string output = _sut.StandardizeCharacters(input);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void CreateUserName_Appends4DigitSuffix()
    {
        string output = _sut.CreateUserName("oðuzhan");
        Assert.StartsWith("oguzhan", output);

        string suffix = output[^4..];
        Assert.True(int.TryParse(suffix, out int n));
        Assert.InRange(n, 0, 9999);
    }
}
