using SpectraUtils.Attributes.ValidationAttributes;
using SpectraUtils.Tests.TestAssets;
using SpectraUtils.Validation;
using Xunit;

namespace SpectraUtils.Tests;

public sealed class TurkishTaxNumberValidatorTests
{
    // No digit at index i (0..8) equals i, so the "9 rule" is never triggered; 2.x also accepted this one.
    private const string ValidWithoutNineRule = "9999999994";

    // Digit 5 at index 5 triggers the "9 rule": temp = 9 and 9 * 2^4 % 9 == 0 must count as 9, not 0.
    // SpectraUtils 2.x rejected this valid number (and ~60% of all valid VKNs).
    private const string ValidWithNineRule = "1739750504";

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("123456789")]
    [InlineData("12345678901")]
    [InlineData("abcdefghij")]
    [InlineData("12345-6789")]
    [InlineData("１２３４５６７８９０")] // full-width digits
    public void IsValid_InvalidInputs_ReturnFalse(string? input)
    {
        Assert.False(TurkishTaxNumberValidator.IsValid(input));
    }

    [Theory]
    [InlineData(ValidWithoutNineRule)]
    [InlineData(ValidWithNineRule)]
    [InlineData("1111111114")]
    [InlineData(" 173 975 0504 ")]
    public void IsValid_ValidNumbers_ReturnTrue(string taxNumber)
    {
        Assert.True(TurkishTaxNumberValidator.IsValid(taxNumber));
    }

    [Fact]
    public void IsValid_OldIncorrectTestValue_IsNowRejected()
    {
        // 2.x used "1111111113" as its "valid" sample; the real check digit for 111111111 is 4.
        Assert.False(TurkishTaxNumberValidator.IsValid("1111111113"));
    }

    [Fact]
    public void IsValid_MatchesReferenceImplementation_ForRandomInput()
    {
        var random = new Random(10);
        int valid = 0;

        for (int i = 0; i < 100_000; i++)
        {
            string vkn = random.NextInt64(0, 10_000_000_000).ToString("D10");
            bool expected = ReferenceIsValid(vkn);
            if (expected)
                valid++;

            Assert.Equal(expected, TurkishTaxNumberValidator.IsValid(vkn));
        }

        // Roughly one in ten random numbers has the correct check digit.
        Assert.InRange(valid, 9_000, 11_000);
    }

    [Fact]
    public void IsValid_ExactlyOneCheckDigitIsValidForEachPrefix()
    {
        var random = new Random(42);

        for (int i = 0; i < 1_000; i++)
        {
            string prefix = random.Next(0, 1_000_000_000).ToString("D9");
            int validCount = Enumerable.Range(0, 10).Count(d => TurkishTaxNumberValidator.IsValid(prefix + d));
            Assert.Equal(1, validCount);
        }
    }

    [Fact]
    public void IsValid_HugeInput_ReturnsFalseWithoutCrashing()
    {
        Assert.False(TurkishTaxNumberValidator.IsValid(new string('1', 2_000_000)));
    }

    private sealed class TaxModel
    {
        [TurkishTaxNumberValidation]
        public string? TaxNumber { get; set; }
    }

    [Fact]
    public void Attribute_NullValue_Succeeds()
    {
        var (ok, results) = ModelValidator.Validate(new TaxModel { TaxNumber = null });

        Assert.True(ok);
        Assert.Empty(results);
    }

    [Fact]
    public void Attribute_InvalidValue_Fails()
    {
        var (ok, results) = ModelValidator.Validate(new TaxModel { TaxNumber = "123" });

        Assert.False(ok);
        ValidationResultAssert.Single(results, nameof(TaxModel.TaxNumber));
    }

    [Fact]
    public void Attribute_ValidValue_Succeeds()
    {
        var (ok, _) = ModelValidator.Validate(new TaxModel { TaxNumber = ValidWithNineRule });
        Assert.True(ok);
    }

    /// <summary>
    /// Straightforward port of the commonly published algorithm, written independently of the library code.
    /// </summary>
    private static bool ReferenceIsValid(string vkn)
    {
        int sum = 0;
        for (int i = 0; i < 9; i++)
        {
            int tmp = (vkn[i] - '0' + (9 - i)) % 10;
            int tmp2 = (int)(tmp * Math.Pow(2, 9 - i) % 9);
            if (tmp != 0 && tmp2 == 0)
                tmp2 = 9;
            sum += tmp2;
        }

        return (10 - sum % 10) % 10 == vkn[9] - '0';
    }
}
