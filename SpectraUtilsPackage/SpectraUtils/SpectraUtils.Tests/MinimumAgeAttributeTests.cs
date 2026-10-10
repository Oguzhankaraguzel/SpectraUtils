using SpectraUtils.Attributes.ValidationAttributes;
using SpectraUtils.Tests.TestAssets;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace SpectraUtils.Tests;

public sealed class MinimumAgeAttributeTests
{
    // "Today" for every test: 15 June 2026 (UTC).
    private static readonly DateOnly Today = new(2026, 6, 15);
    private static readonly IServiceProvider Services =
        new TimeServiceProvider(new FixedTimeProvider(new DateTimeOffset(2026, 6, 15, 12, 0, 0, TimeSpan.Zero)));

    private sealed class AgeModel
    {
        [MinimumAge(18)]
        public DateTime BirthDateTime { get; set; }

        [MinimumAge(18)]
        public DateOnly BirthDateOnly { get; set; }

        [MinimumAge(18)]
        public DateTimeOffset BirthDateTimeOffset { get; set; }

        [MinimumAge(18)]
        public DateTime? NullableDateTime { get; set; }

        [MinimumAge(18)]
        public object? Untyped { get; set; }
    }

    private static AgeModel ModelBornOn(DateOnly birthDate) => new()
    {
        BirthDateTime = birthDate.ToDateTime(TimeOnly.MinValue),
        BirthDateOnly = birthDate,
        BirthDateTimeOffset = new DateTimeOffset(birthDate.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero),
        NullableDateTime = birthDate.ToDateTime(TimeOnly.MinValue),
        Untyped = null,
    };

    [Fact]
    public void Constructor_NegativeMinimum_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new MinimumAgeAttribute(-1));
    }

    [Fact]
    public void ExactlyMinimumAge_Succeeds()
    {
        var (ok, results) = ModelValidator.Validate(ModelBornOn(Today.AddYears(-18)), Services);

        Assert.True(ok);
        Assert.Empty(results);
    }

    [Fact]
    public void OneDayTooYoung_FailsForEverySupportedType()
    {
        var (ok, results) = ModelValidator.Validate(ModelBornOn(Today.AddYears(-18).AddDays(1)), Services);

        Assert.False(ok);
        Assert.Equal(4, results.Count);
        Assert.All(results, r => Assert.Single(r.MemberNames));
    }

    [Fact]
    public void FutureBirthDate_Fails()
    {
        var (ok, _) = ModelValidator.Validate(ModelBornOn(Today.AddDays(1)), Services);
        Assert.False(ok);
    }

    [Fact]
    public void NullValue_Succeeds()
    {
        // null means "not provided"; use [Required] to make it mandatory.
        var attr = new MinimumAgeAttribute(18);

        Assert.Equal(ValidationResult.Success, attr.GetValidationResult(null, new ValidationContext(new object())));
        Assert.True(attr.IsValid(null));
    }

    [Fact]
    public void IsValidWithoutContext_DoesNotThrow()
    {
        // 2.x dereferenced the (null) validation context here and threw NullReferenceException.
        var attr = new MinimumAgeAttribute(18);

        Assert.True(attr.IsValid(new DateOnly(1990, 1, 1)));
        Assert.False(attr.IsValid(DateOnly.FromDateTime(DateTime.Today)));
        Assert.False(attr.IsValid("not-a-date"));
    }

    [Fact]
    public void InvalidType_FailsWithTypeMessage()
    {
        var result = new MinimumAgeAttribute(18).GetValidationResult("not-a-date", new ValidationContext(new object()) { MemberName = "BirthDate" });

        Assert.NotNull(result);
        Assert.Contains("must be a DateOnly, DateTime or DateTimeOffset", result!.ErrorMessage);
        Assert.Equal(new[] { "BirthDate" }, result.MemberNames);
    }

    [Fact]
    public void DefaultErrorMessage_ContainsDisplayNameAndAge()
    {
        var result = new MinimumAgeAttribute(21).GetValidationResult(
            Today, new ValidationContext(new object(), Services, null) { DisplayName = "Doğum tarihi", MemberName = "BirthDate" });

        Assert.Equal("The Doğum tarihi field requires a minimum age of 21.", result!.ErrorMessage);
    }

    private sealed class CustomMessageModel
    {
        [MinimumAge(18, ErrorMessage = "{0}: en az {1} yaşında olmalısınız.")]
        public DateOnly DogumTarihi { get; set; }
    }

    [Fact]
    public void CustomErrorMessage_SupportsPlaceholders()
    {
        var (_, results) = ModelValidator.Validate(new CustomMessageModel { DogumTarihi = Today }, Services);
        Assert.Equal("DogumTarihi: en az 18 yaşında olmalısınız.", Assert.Single(results).ErrorMessage);
    }

    [Fact]
    public void UsesTimeProviderFromServices()
    {
        var birthDate = new DateOnly(2008, 6, 16);
        var attr = new MinimumAgeAttribute(18);

        var dayBefore = new TimeServiceProvider(new FixedTimeProvider(new DateTimeOffset(2026, 6, 15, 0, 0, 0, TimeSpan.Zero)));
        var birthday = new TimeServiceProvider(new FixedTimeProvider(new DateTimeOffset(2026, 6, 16, 0, 0, 0, TimeSpan.Zero)));

        Assert.NotEqual(ValidationResult.Success, attr.GetValidationResult(birthDate, new ValidationContext(new object(), dayBefore, null)));
        Assert.Equal(ValidationResult.Success, attr.GetValidationResult(birthDate, new ValidationContext(new object(), birthday, null)));
    }

    [Theory]
    [InlineData("2008-02-29", "2026-02-27", 17)]
    [InlineData("2008-02-29", "2026-02-28", 18)]
    [InlineData("2008-02-29", "2028-02-28", 19)]
    [InlineData("2008-02-29", "2028-02-29", 20)]
    [InlineData("2000-12-31", "2026-01-01", 25)]
    public void CalculateAge_HandlesLeapYearsAndYearBoundaries(string birth, string today, int expected)
    {
        Assert.Equal(expected, MinimumAgeAttribute.CalculateAge(DateOnly.Parse(birth), DateOnly.Parse(today)));
    }
}
