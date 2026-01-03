using Microsoft.AspNetCore.Http;
using SpectraUtils.Attributes.ValidationAttributes;
using SpectraUtils.Tests.TestAssets;
using System.ComponentModel.DataAnnotations;
using System.IO;
using Xunit;

namespace SpectraUtils.Tests;

public sealed class ValidationAttributeTests
{
    private static ValidationContext CreateContext(object instance) => new(instance);

    private sealed class AgeModel
    {
        [MinimumAge(18)]
        public DateTime BirthDateTime { get; set; }

        [MinimumAge(18)]
        public DateOnly BirthDateOnly { get; set; }

        [MinimumAge(18)]
        public object? InvalidType { get; set; }

        [MinimumAge(18)]
        public DateTime? NullableDateTime { get; set; }
    }

    [Fact]
    public void MinimumAge_NegativeMinimum_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new MinimumAgeAttribute(-1));
    }

    [Fact]
    public void MinimumAge_NullValue_Fails()
    {
        var attr = new MinimumAgeAttribute(18);
        var context = new ValidationContext(new object());
        
        var result = attr.GetValidationResult(null, context);
        Assert.NotNull(result);
        Assert.NotEqual(ValidationResult.Success, result);
    }

    [Fact]
    public void MinimumAge_DateOnly_ExactlyMinimumAge_Succeeds()
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.Today);
        var model = new AgeModel 
        { 
            BirthDateTime = today.AddYears(-18).ToDateTime(TimeOnly.MinValue),
            BirthDateOnly = today.AddYears(-18),
            InvalidType = null,
            NullableDateTime = today.AddYears(-18).ToDateTime(TimeOnly.MinValue)
        };

        var results = new List<ValidationResult>();
        bool ok = Validator.TryValidateObject(model, CreateContext(model), results, validateAllProperties: true);

        Assert.True(ok);
        Assert.Empty(results);
    }

    [Fact]
    public void MinimumAge_DateTime_ExactlyMinimumAge_Succeeds()
    {
        DateTime today = DateTime.Today;
        DateOnly todayDateOnly = DateOnly.FromDateTime(today);
        var model = new AgeModel 
        { 
            BirthDateTime = today.AddYears(-18),
            BirthDateOnly = todayDateOnly.AddYears(-18),
            InvalidType = null,
            NullableDateTime = today.AddYears(-18)
        };

        var results = new List<ValidationResult>();
        bool ok = Validator.TryValidateObject(model, CreateContext(model), results, validateAllProperties: true);

        Assert.True(ok);
        Assert.Empty(results);
    }

    [Fact]
    public void MinimumAge_DateTime_OneDayTooYoung_Fails()
    {
        DateTime today = DateTime.Today;
        DateOnly todayDateOnly = DateOnly.FromDateTime(today);
        var model = new AgeModel 
        { 
            BirthDateTime = today.AddYears(-18).AddDays(1),
            BirthDateOnly = todayDateOnly.AddYears(-18).AddDays(1),
            InvalidType = null,
            NullableDateTime = today.AddYears(-18).AddDays(1)
        };

        var results = new List<ValidationResult>();
        bool ok = Validator.TryValidateObject(model, CreateContext(model), results, validateAllProperties: true);

        Assert.False(ok);
        Assert.NotEmpty(results);
    }

    [Fact]
    public void MinimumAge_InvalidType_FailsWithInvalidFormatMessage()
    {
        var attr = new MinimumAgeAttribute(18);
        var result = attr.GetValidationResult("not-a-date", new ValidationContext(new object()));

        Assert.NotNull(result);
        Assert.NotEqual(ValidationResult.Success, result);
        Assert.Contains("Invalid date format", result!.ErrorMessage);
    }

    private sealed class TcIdModel
    {
        [TCIDValidation]
        public string? TcId { get; set; }
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("00000000000", false)]
    [InlineData("123", false)]
    [InlineData("11111111110", false)]
    [InlineData("10000000146", true)]
    [InlineData("24782525476", true)]
    [InlineData("30261004950", true)]
    public void TcIdValidation_Works(string? tcId, bool expectedOk)
    {
        var model = new TcIdModel { TcId = tcId };
        var results = new List<ValidationResult>();

        bool ok = Validator.TryValidateObject(model, CreateContext(model), results, validateAllProperties: true);

        Assert.Equal(expectedOk, ok);
    }

    private sealed class FileModel
    {
        [AllowedExtensions(1024, ".jpg", ".png")]
        public string? FilePath { get; set; }

        [AllowedExtensions(1024, ".jpg", ".png")]
        public IFormFile? Upload { get; set; }
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("file", false)]
    [InlineData("file.exe", false)]
    [InlineData("file.jpg", true)]
    [InlineData("FILE.PNG", true)]
    public void AllowedExtensions_FilePath_Works(string? path, bool expectedOk)
    {
        var model = new FileModel { FilePath = path };
        var results = new List<ValidationResult>();

        bool ok = Validator.TryValidateObject(model, CreateContext(model), results, validateAllProperties: true);
        Assert.Equal(expectedOk, ok);
    }

    [Fact]
    public void AllowedExtensions_IFormFile_TooLarge_Fails()
    {
        var file = FormFileFactory.Create("test.jpg", 2048);

        var model = new FileModel { Upload = file };
        var results = new List<ValidationResult>();

        bool ok = Validator.TryValidateObject(model, CreateContext(model), results, validateAllProperties: true);

        Assert.False(ok);
        Assert.Contains(results, r => r.ErrorMessage != null && r.ErrorMessage.Contains("File size"));
    }

    [Fact]
    public void AllowedExtensions_IFormFile_Valid_Succeeds()
    {
        var file = FormFileFactory.Create("photo.png", 128);

        var model = new FileModel { Upload = file };
        var results = new List<ValidationResult>();

        bool ok = Validator.TryValidateObject(model, CreateContext(model), results, validateAllProperties: true);

        Assert.True(ok);
        Assert.Empty(results);
    }

    [Fact]
    public void AllowedExtensions_IFormFile_InvalidExtension_Fails()
    {
        var file = FormFileFactory.Create("test.exe", 10);

        var model = new FileModel { Upload = file };
        var results = new List<ValidationResult>();

        bool ok = Validator.TryValidateObject(model, CreateContext(model), results, validateAllProperties: true);

        Assert.False(ok);
        Assert.Contains(results, r => r.ErrorMessage != null && r.ErrorMessage.Contains("Invalid file type"));
    }

    [Fact]
    public void AllowedExtensions_InvalidType_ReturnsValidationError()
    {
        var attr = new AllowedExtensionsAttribute(1024, ".jpg");

        var result = attr.GetValidationResult(new object(), new ValidationContext(new object()));

        Assert.NotNull(result);
        Assert.NotEqual(ValidationResult.Success, result);
        Assert.Contains("Invalid type", result!.ErrorMessage);
    }

    [Fact]
    public void AllowedExtensions_FilePath_ExistingFileTooLarge_Fails()
    {
        string path = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.jpg");
        File.WriteAllBytes(path, new byte[2048]);

        try
        {
            var model = new FileModel { FilePath = path };
            var results = new List<ValidationResult>();

            bool ok = Validator.TryValidateObject(model, CreateContext(model), results, validateAllProperties: true);

            Assert.False(ok);
            Assert.Contains(results, r => r.ErrorMessage != null && r.ErrorMessage.Contains("File size"));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void AllowedExtensions_Constructor_NoExtensions_Throws()
    {
        Assert.Throws<ArgumentException>(() => new AllowedExtensionsAttribute());
    }

    [Fact]
    public void AllowedExtensions_Constructor_NonPositiveMaxSize_Throws()
    {
        Assert.Throws<ArgumentException>(() => new AllowedExtensionsAttribute(0, ".jpg"));
    }
}
