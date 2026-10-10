using SpectraUtils.Attributes.ValidationAttributes;
using SpectraUtils.Tests.TestAssets;
using SpectraUtils.Validation;
using System.Numerics;
using Xunit;

namespace SpectraUtils.Tests;

public sealed class IbanValidatorTests
{
    private const string ValidTurkishIban = "TR33 0006 1005 1978 6457 8413 26";

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void IsValid_NullOrWhitespace_False(string? input)
    {
        Assert.False(IbanValidator.IsValid(input));
    }

    [Theory]
    [InlineData("TR330006100519786457841326")]
    [InlineData(ValidTurkishIban)]
    [InlineData("tr33 0006 1005 1978 6457 8413 26")]
    [InlineData("  TR330006100519786457841326  ")]
    [InlineData("DE89370400440532013000")]
    [InlineData("GB82WEST12345698765432")]
    [InlineData("NL91ABNA0417164300")]
    [InlineData("FR1420041010050500013M02606")]
    public void IsValid_ValidIbans_ReturnTrue(string iban)
    {
        Assert.True(IbanValidator.IsValid(iban));
    }

    [Fact]
    public void IsValid_InvalidCheckDigits_ReturnsFalse()
    {
        Assert.False(IbanValidator.IsValid("TR33 0006 1005 1978 6457 8413 27"));
    }

    [Theory]
    [InlineData("TR33-0006-1005-1978-6457-8413-26")]
    [InlineData("TR33 0006 1005 1978 6457 8413 2٦")] // Arabic-Indic digit six
    [InlineData("TR33 0006 1005 1978 6457 8413 ı6")] // dotless i must not be upper-cased to 'I'
    [InlineData("TRAB0006100519786457841326")]
    [InlineData("1R330006100519786457841326")]
    public void IsValid_InvalidCharactersOrFormat_ReturnsFalse(string iban)
    {
        Assert.False(IbanValidator.IsValid(iban));
    }

    [Fact]
    public void IsValid_WrongLengthForCountry_ReturnsFalseEvenWithCorrectChecksum()
    {
        // A Turkish IBAN must be 26 characters; this one is 24 but has valid check digits.
        string tooShort = BuildIban("TR", "00061005197864578413");
        Assert.Equal(24, tooShort.Length);
        Assert.True(Mod97(tooShort) == 1);

        Assert.False(IbanValidator.IsValid(tooShort));
    }

    [Fact]
    public void IsValid_UnknownCountryWithValidChecksum_ReturnsTrue()
    {
        // Countries that are not in the built-in registry are only checked for the generic 15..34 length.
        string iban = BuildIban("ZZ", "1234567890123456");
        Assert.True(IbanValidator.IsValid(iban));
    }

    [Fact]
    public void IsValid_MatchesReferenceImplementation_ForRandomInput()
    {
        var random = new Random(97);

        for (int i = 0; i < 2_000; i++)
        {
            string bban = string.Concat(Enumerable.Range(0, 22).Select(_ => (char)('0' + random.Next(10))));
            string checkDigits = random.Next(100).ToString("D2");
            string iban = "TR" + checkDigits + bban;

            Assert.Equal(Mod97(iban) == 1, IbanValidator.IsValid(iban));
        }
    }

    [Fact]
    public void IsValid_HugeInput_ReturnsFalseWithoutCrashing()
    {
        // Used to be stackalloc'ed with the input length: 2M chars crashed the process with a stack overflow.
        Assert.False(IbanValidator.IsValid(new string('1', 2_000_000)));
        Assert.False(IbanValidator.IsValid("TR33" + new string(' ', 1_000_000) + "0006100519786457841326"));
    }

    [Fact]
    public void IsValid_CountryCodeMismatch_ReturnsFalse()
    {
        Assert.False(IbanValidator.IsValid("TR330006100519786457841326", "DE"));
    }

    [Theory]
    [InlineData("TR")]
    [InlineData("tr")]
    [InlineData(" TR ")]
    public void IsValid_CountryCodeMatch_ReturnsTrue(string countryCode)
    {
        Assert.True(IbanValidator.IsValid("TR330006100519786457841326", countryCode));
    }

    private sealed class IbanModel
    {
        [IbanValidation]
        public string? Iban { get; set; }
    }

    private sealed class CountrySpecificIbanModel
    {
        [IbanValidation("TR")]
        public string? Iban { get; set; }
    }

    [Fact]
    public void Attribute_NullValue_Succeeds()
    {
        var (ok, results) = ModelValidator.Validate(new IbanModel { Iban = null });

        Assert.True(ok);
        Assert.Empty(results);
    }

    [Fact]
    public void Attribute_InvalidValue_FailsWithMemberNameAndDisplayName()
    {
        var (ok, results) = ModelValidator.Validate(new IbanModel { Iban = "invalid" });

        Assert.False(ok);
        ValidationResultAssert.Single(results, nameof(IbanModel.Iban));
        Assert.Equal("The Iban field is not a valid IBAN.", results[0].ErrorMessage);
    }

    [Fact]
    public void Attribute_ValidValue_Succeeds()
    {
        var (ok, _) = ModelValidator.Validate(new IbanModel { Iban = ValidTurkishIban });
        Assert.True(ok);
    }

    [Fact]
    public void Attribute_CountryMismatch_Fails()
    {
        var (ok, _) = ModelValidator.Validate(new CountrySpecificIbanModel { Iban = "DE89370400440532013000" });
        Assert.False(ok);
    }

    [Fact]
    public void Attribute_CountryMatch_Succeeds()
    {
        var (ok, _) = ModelValidator.Validate(new CountrySpecificIbanModel { Iban = ValidTurkishIban });
        Assert.True(ok);
    }

    [Theory]
    [InlineData("")]
    [InlineData("T")]
    [InlineData("TUR")]
    [InlineData("T1")]
    public void Attribute_InvalidCountryCode_Throws(string countryCode)
    {
        Assert.Throws<ArgumentException>(() => new IbanValidationAttribute(countryCode));
    }

    [Fact]
    public void Attribute_NonStringValue_Fails()
    {
        Assert.False(new IbanValidationAttribute().IsValid(12345));
    }

    private static string BuildIban(string country, string bban)
    {
        int checkDigits = 98 - Mod97(country + "00" + bban);
        return country + checkDigits.ToString("D2") + bban;
    }

    /// <summary>Independent reference implementation using BigInteger.</summary>
    private static int Mod97(string iban)
    {
        string rearranged = iban[4..] + iban[..4];
        string numeric = string.Concat(rearranged.Select(c => char.IsDigit(c) ? c.ToString() : (c - 'A' + 10).ToString()));
        return (int)(BigInteger.Parse(numeric) % 97);
    }
}
