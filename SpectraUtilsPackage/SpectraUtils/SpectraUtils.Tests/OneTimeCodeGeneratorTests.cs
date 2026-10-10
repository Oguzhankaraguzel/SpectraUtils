using SpectraUtils.Services;
using Xunit;

namespace SpectraUtils.Tests;

public sealed class OneTimeCodeGeneratorTests
{
    private readonly OneTimeCodeGenerator _sut = new();

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(OneTimeCodeGenerator.MaxDigits + 1)]
    public void GenerateNumericCode_OutOfRangeDigits_Throws(int digits)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _sut.GenerateNumericCode(digits));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(6)]
    [InlineData(10)]
    [InlineData(18)]
    [InlineData(OneTimeCodeGenerator.MaxDigits)]
    public void GenerateNumericCode_ReturnsRequestedNumberOfDigits(int digits)
    {
        string code = _sut.GenerateNumericCode(digits);

        Assert.Equal(digits, code.Length);
        Assert.All(code, c => Assert.InRange(c, '0', '9'));
    }

    [Fact]
    public void GenerateNumericCode_DefaultLength_IsSix()
    {
        Assert.Equal(6, _sut.GenerateNumericCode().Length);
    }

    [Fact]
    public void GenerateNumericCode_TenDigits_EveryLeadingDigitAppears()
    {
        // 2.x drew 10+ digit codes from [0, int.MaxValue), so they never started with 3-9.
        var leadingDigits = new HashSet<char>();

        for (int i = 0; i < 2_000; i++)
            leadingDigits.Add(_sut.GenerateNumericCode(10)[0]);

        Assert.Equal(10, leadingDigits.Count);
    }

    [Fact]
    public void GenerateNumericCode_DigitsAreRoughlyUniform()
    {
        var counts = new int[10];

        for (int i = 0; i < 10_000; i++)
        {
            foreach (char c in _sut.GenerateNumericCode(6))
                counts[c - '0']++;
        }

        // 60,000 digits, 6,000 expected per digit; bounds are > 10 standard deviations wide.
        Assert.All(counts, count => Assert.InRange(count, 5_200, 6_800));
    }
}
