using SpectraUtils.Concerete;
using Xunit;

namespace SpectraUtils.Tests;

public sealed class OneTimeCodeGeneratorTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void GenerateNumericCode_NonPositiveDigits_Throws(int digits)
    {
        var generator = new OneTimeCodeGenerator();
        Assert.Throws<ArgumentOutOfRangeException>(() => generator.GenerateNumericCode(digits));
    }

    [Fact]
    public void GenerateNumericCode_TooLargeDigits_Throws()
    {
        var generator = new OneTimeCodeGenerator();
        Assert.Throws<ArgumentOutOfRangeException>(() => generator.GenerateNumericCode(19));
    }

    [Fact]
    public void GenerateNumericCode_DefaultLength_ReturnsDigitsOnly()
    {
        var generator = new OneTimeCodeGenerator();
        string code = generator.GenerateNumericCode();

        Assert.Equal(6, code.Length);
        Assert.All(code, c => Assert.InRange(c, '0', '9'));
    }

    [Fact]
    public void GenerateNumericCode_CustomDigits_PaddedCorrectly()
    {
        var generator = new OneTimeCodeGenerator();
        string code = generator.GenerateNumericCode(10);

        Assert.Equal(10, code.Length);
        Assert.All(code, c => Assert.InRange(c, '0', '9'));
    }
}
