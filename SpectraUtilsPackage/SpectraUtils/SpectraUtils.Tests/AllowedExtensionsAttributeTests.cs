using Microsoft.AspNetCore.Http;
using SpectraUtils.Attributes.ValidationAttributes;
using SpectraUtils.Tests.TestAssets;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Xunit;

namespace SpectraUtils.Tests;

public sealed class AllowedExtensionsAttributeTests
{
    private sealed class FileModel
    {
        [AllowedExtensions(1024, ".jpg", "png")]
        public string? FileName { get; set; }

        [AllowedExtensions(1024, ".jpg", "png")]
        public IFormFile? Upload { get; set; }

        [AllowedExtensions(1024, ".jpg", "png")]
        public IFormFileCollection? Uploads { get; set; }

        [AllowedExtensions(1024, ".jpg", "png")]
        public List<IFormFile>? UploadList { get; set; }
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("file", false)]
    [InlineData("file.exe", false)]
    [InlineData("file.jpg.exe", false)]
    [InlineData("file.jpg", true)]
    [InlineData("FILE.PNG", true)]
    [InlineData("folder/sub/photo.png", true)]
    public void FileName_Works(string? fileName, bool expectedOk)
    {
        var (ok, _) = ModelValidator.Validate(new FileModel { FileName = fileName });
        Assert.Equal(expectedOk, ok);
    }

    [Fact]
    public void FileName_IsNotLookedUpOnDisk()
    {
        // 2.x called File.Exists on the user-supplied string and reported the size of server files.
        string path = Path.Combine(Path.GetTempPath(), $"spectra_{Guid.NewGuid()}.jpg");
        File.WriteAllBytes(path, new byte[4096]);

        try
        {
            var (ok, _) = ModelValidator.Validate(new FileModel { FileName = path });
            Assert.True(ok);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void FormFile_Valid_Succeeds()
    {
        var (ok, results) = ModelValidator.Validate(new FileModel { Upload = FormFileFactory.Create("photo.png", 128) });

        Assert.True(ok);
        Assert.Empty(results);
    }

    [Fact]
    public void FormFile_InvalidExtension_Fails()
    {
        var (ok, results) = ModelValidator.Validate(new FileModel { Upload = FormFileFactory.Create("test.exe", 10) });

        Assert.False(ok);
        ValidationResultAssert.Single(results, nameof(FileModel.Upload));
        Assert.Equal("The Upload field only accepts files with the following extensions: .jpg, .png.", results[0].ErrorMessage);
    }

    [Fact]
    public void FormFile_TooLarge_FailsWithReadableSize()
    {
        using var _ = new CultureScope(CultureInfo.InvariantCulture);

        var (ok, results) = ModelValidator.Validate(new FileModel { Upload = FormFileFactory.Create("test.jpg", 2048) });

        Assert.False(ok);
        Assert.Equal("The Upload field exceeds the maximum file size of 1 KB.", Assert.Single(results).ErrorMessage);
    }

    [Fact]
    public void FormFileCollection_AllValid_Succeeds()
    {
        var files = FormFileFactory.CreateCollection(FormFileFactory.Create("a.jpg", 10), FormFileFactory.Create("b.png", 10));
        var (ok, _) = ModelValidator.Validate(new FileModel { Uploads = files, UploadList = files.ToList() });
        Assert.True(ok);
    }

    [Fact]
    public void FormFileCollection_OneInvalid_Fails()
    {
        var files = FormFileFactory.CreateCollection(FormFileFactory.Create("a.jpg", 10), FormFileFactory.Create("b.exe", 10));
        var (ok, results) = ModelValidator.Validate(new FileModel { Uploads = files, UploadList = files.ToList() });

        Assert.False(ok);
        Assert.Equal(2, results.Count);
    }

    [Fact]
    public void FormFileCollection_Empty_Succeeds()
    {
        var (ok, _) = ModelValidator.Validate(new FileModel { Uploads = new FormFileCollection() });
        Assert.True(ok);
    }

    [Fact]
    public void InvalidType_ReturnsValidationError()
    {
        var result = new AllowedExtensionsAttribute(1024, ".jpg").GetValidationResult(new object(), new ValidationContext(new object()));

        Assert.NotNull(result);
        Assert.Contains("must be an IFormFile", result!.ErrorMessage);
    }

    [Fact]
    public void IsValidWithoutContext_DoesNotThrow()
    {
        var attr = new AllowedExtensionsAttribute(".jpg");

        Assert.True(attr.IsValid("a.jpg"));
        Assert.False(attr.IsValid("a.exe"));
        Assert.True(attr.IsValid(null));
    }

    private sealed class CustomMessageModel
    {
        [AllowedExtensions(1024, ".pdf", ErrorMessage = "{0}: sadece {1}", FileSizeErrorMessage = "{0}: en fazla {2}")]
        public IFormFile? Document { get; set; }

        [AllowedExtensions(1024, ".pdf", ErrorMessage = "Geçersiz dosya.")]
        public IFormFile? Legacy { get; set; }
    }

    [Fact]
    public void CustomMessages_AreUsedForTheMatchingError()
    {
        using var _ = new CultureScope(CultureInfo.InvariantCulture);

        var (_, extensionResults) = ModelValidator.Validate(new CustomMessageModel { Document = FormFileFactory.Create("a.exe", 1) });
        var (_, sizeResults) = ModelValidator.Validate(new CustomMessageModel { Document = FormFileFactory.Create("a.pdf", 4096) });
        var (_, legacyResults) = ModelValidator.Validate(new CustomMessageModel { Legacy = FormFileFactory.Create("a.pdf", 4096) });

        Assert.Equal("Document: sadece .pdf", Assert.Single(extensionResults).ErrorMessage);
        Assert.Equal("Document: en fazla 1 KB", Assert.Single(sizeResults).ErrorMessage);
        Assert.Equal("Geçersiz dosya.", Assert.Single(legacyResults).ErrorMessage);
    }

    [Theory]
    [InlineData(500, "500 bytes")]
    [InlineData(500 * 1024, "500 KB")]
    [InlineData(1536 * 1024, "1.5 MB")]
    [InlineData(2L * 1024 * 1024 * 1024, "2 GB")]
    [InlineData(long.MaxValue, "unlimited")]
    public void FormatSize_IsHumanReadable(long bytes, string expected)
    {
        // 2.x reported "0 MB" for limits below one megabyte.
        using var _ = new CultureScope(CultureInfo.InvariantCulture);
        Assert.Equal(expected, AllowedExtensionsAttribute.FormatSize(bytes));
    }

    [Fact]
    public void AllowedExtensions_AreNormalized()
    {
        var attr = new AllowedExtensionsAttribute(" JPG ", ".Png");
        Assert.Equal(new[] { ".jpg", ".png" }, attr.AllowedExtensions.Order());
    }

    [Fact]
    public void Constructor_NoExtensions_Throws()
    {
        Assert.Throws<ArgumentException>(() => new AllowedExtensionsAttribute());
    }

    [Fact]
    public void Constructor_EmptyExtension_Throws()
    {
        Assert.Throws<ArgumentException>(() => new AllowedExtensionsAttribute(" "));
    }

    [Fact]
    public void Constructor_NonPositiveMaxSize_Throws()
    {
        Assert.Throws<ArgumentException>(() => new AllowedExtensionsAttribute(0, ".jpg"));
    }

    private sealed class CultureScope : IDisposable
    {
        private readonly CultureInfo _original = CultureInfo.CurrentCulture;

        public CultureScope(CultureInfo culture) => CultureInfo.CurrentCulture = culture;

        public void Dispose() => CultureInfo.CurrentCulture = _original;
    }
}
