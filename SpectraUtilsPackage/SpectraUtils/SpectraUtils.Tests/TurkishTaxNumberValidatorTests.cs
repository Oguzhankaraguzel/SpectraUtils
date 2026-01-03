using SpectraUtils.Attributes.ValidationAttributes;
using SpectraUtils.Validation;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace SpectraUtils.Tests;

public sealed class TurkishTaxNumberValidatorTests
{
    private const string ValidTaxNumber = "1111111113";

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("123456789")]
    [InlineData("12345678901")]
    [InlineData("abcdefghij")]
    public void IsValid_InvalidInputs_ReturnFalse(string? input)
    {
        Assert.False(TurkishTaxNumberValidator.IsValid(input!));
    }

    [Fact]
    public void IsValid_ValidNumber_ReturnsTrue()
    {
        Assert.True(TurkishTaxNumberValidator.IsValid(ValidTaxNumber));
    }

    [Fact]
    public void IsValid_WrongCheckDigit_ReturnsFalse()
    {
        Assert.False(TurkishTaxNumberValidator.IsValid("1111111110"));
    }

    private sealed class TaxModel
    {
        [TurkishTaxNumberValidation]
        public string? TaxNumber { get; set; }
    }

    [Fact]
    public void TurkishTaxNumberValidation_NullValue_Succeeds()
    {
        var model = new TaxModel { TaxNumber = null };
        var results = new List<ValidationResult>();

        bool ok = Validator.TryValidateObject(model, new ValidationContext(model), results, true);

        Assert.True(ok);
        Assert.Empty(results);
    }

    [Fact]
    public void TurkishTaxNumberValidation_InvalidValue_Fails()
    {
        var model = new TaxModel { TaxNumber = "123" };
        var results = new List<ValidationResult>();

        bool ok = Validator.TryValidateObject(model, new ValidationContext(model), results, true);

        Assert.False(ok);
        Assert.NotEmpty(results);
    }

    [Fact]
    public void TurkishTaxNumberValidation_ValidValue_Succeeds()
    {
        var model = new TaxModel { TaxNumber = ValidTaxNumber };
        var results = new List<ValidationResult>();

        bool ok = Validator.TryValidateObject(model, new ValidationContext(model), results, true);

        Assert.True(ok);
        Assert.Empty(results);
    }
}
