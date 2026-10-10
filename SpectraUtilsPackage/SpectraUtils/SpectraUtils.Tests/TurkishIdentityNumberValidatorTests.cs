using SpectraUtils.Attributes.ValidationAttributes;
using SpectraUtils.Tests.TestAssets;
using SpectraUtils.Validation;
using Xunit;

namespace SpectraUtils.Tests;

public sealed class TurkishIdentityNumberValidatorTests
{
    [Theory]
    [InlineData("10000000146")]
    [InlineData("24782525476")]
    [InlineData("30261004950")]
    [InlineData(" 10000000146 ")]
    [InlineData("100 000 001 46")]
    public void IsValid_ValidNumbers_ReturnTrue(string identityNumber)
    {
        Assert.True(TurkishIdentityNumberValidator.IsValid(identityNumber));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("123")]
    [InlineData("100000001460")]
    [InlineData("00000000000")]
    [InlineData("01000000146")]
    [InlineData("11111111110")] // passes the checksum but is a placeholder, never issued
    [InlineData("10000000147")]
    [InlineData("10000000156")]
    [InlineData("1000000014a")]
    [InlineData("1000000014٦")] // Arabic-Indic digit six
    public void IsValid_InvalidNumbers_ReturnFalse(string? identityNumber)
    {
        Assert.False(TurkishIdentityNumberValidator.IsValid(identityNumber));
    }

    [Fact]
    public void IsValid_HugeInput_ReturnsFalseWithoutCrashing()
    {
        Assert.False(TurkishIdentityNumberValidator.IsValid(new string('1', 2_000_000)));
    }

    private sealed class TcIdModel
    {
        [TCIDValidation]
        public string? TcId { get; set; }
    }

    [Theory]
    [InlineData(null, false)] // the attribute implies a mandatory value
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("11111111110", false)]
    [InlineData("10000000146", true)]
    public void Attribute_Works(string? tcId, bool expectedOk)
    {
        var (ok, _) = ModelValidator.Validate(new TcIdModel { TcId = tcId });
        Assert.Equal(expectedOk, ok);
    }

    [Fact]
    public void Attribute_InvalidValue_ReportsMemberName()
    {
        var (_, results) = ModelValidator.Validate(new TcIdModel { TcId = "123" });

        ValidationResultAssert.Single(results, nameof(TcIdModel.TcId));
        Assert.Equal("The TcId field is not a valid Turkish identity number.", results[0].ErrorMessage);
    }

    private sealed class CustomMessageModel
    {
        [TCIDValidation(ErrorMessage = "{0} geçersiz.")]
        public string? Kimlik { get; set; }
    }

    [Fact]
    public void Attribute_CustomMessage_SupportsDisplayNamePlaceholder()
    {
        var (_, results) = ModelValidator.Validate(new CustomMessageModel { Kimlik = "123" });
        Assert.Equal("Kimlik geçersiz.", Assert.Single(results).ErrorMessage);
    }
}
