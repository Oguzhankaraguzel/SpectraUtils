using SpectraUtils.Attributes.ValidationAttributes;
using SpectraUtils.Validation;
using System.ComponentModel.DataAnnotations;
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
        Assert.False(IbanValidator.IsValid(input!));
    }

    [Fact]
    public void IsValid_ValidIbanWithSpaces_ReturnsTrue()
    {
        Assert.True(IbanValidator.IsValid(ValidTurkishIban));
    }

    [Fact]
    public void IsValid_InvalidCheckDigits_ReturnsFalse()
    {
        Assert.False(IbanValidator.IsValid("TR33 0006 1005 1978 6457 8413 27"));
    }

    [Fact]
    public void IsValid_InvalidCharacters_ReturnsFalse()
    {
        Assert.False(IbanValidator.IsValid("TR33-0006-1005-1978-6457-8413-26"));
    }

    [Fact]
    public void IsValid_CountryCodeMismatch_ReturnsFalse()
    {
        Assert.False(IbanValidator.IsValid("TR330006100519786457841326", "DE"));
    }

    [Fact]
    public void IsValid_CountryCodeMatch_ReturnsTrue()
    {
        Assert.True(IbanValidator.IsValid("TR330006100519786457841326", "TR"));
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
    public void IbanValidationAttribute_NullValue_Succeeds()
    {
        var model = new IbanModel { Iban = null };
        var results = new List<ValidationResult>();

        bool ok = Validator.TryValidateObject(model, new ValidationContext(model), results, true);

        Assert.True(ok);
        Assert.Empty(results);
    }

    [Fact]
    public void IbanValidationAttribute_InvalidValue_Fails()
    {
        var model = new IbanModel { Iban = "invalid" };
        var results = new List<ValidationResult>();

        bool ok = Validator.TryValidateObject(model, new ValidationContext(model), results, true);

        Assert.False(ok);
        Assert.NotEmpty(results);
    }

    [Fact]
    public void IbanValidationAttribute_ValidValue_Succeeds()
    {
        var model = new IbanModel { Iban = ValidTurkishIban };
        var results = new List<ValidationResult>();

        bool ok = Validator.TryValidateObject(model, new ValidationContext(model), results, true);

        Assert.True(ok);
        Assert.Empty(results);
    }

    [Fact]
    public void IbanValidationAttribute_CountryMismatch_Fails()
    {
        var model = new CountrySpecificIbanModel { Iban = "DE12500105170648489890" };
        var results = new List<ValidationResult>();

        bool ok = Validator.TryValidateObject(model, new ValidationContext(model), results, true);

        Assert.False(ok);
        Assert.NotEmpty(results);
    }

    [Fact]
    public void IbanValidationAttribute_CountryMatch_Succeeds()
    {
        var model = new CountrySpecificIbanModel { Iban = ValidTurkishIban };
        var results = new List<ValidationResult>();

        bool ok = Validator.TryValidateObject(model, new ValidationContext(model), results, true);

        Assert.True(ok);
        Assert.Empty(results);
    }
}
