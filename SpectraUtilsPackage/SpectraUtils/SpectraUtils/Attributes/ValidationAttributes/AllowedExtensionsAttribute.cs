using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace SpectraUtils.Attributes.ValidationAttributes;

/// <summary>
/// Validation attribute to specify allowed file extensions and optional maximum file size for file uploads.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class AllowedExtensionsAttribute : ValidationAttribute
{
    private readonly HashSet<string> _extensions;
    private readonly long _maxFileSizeInBytes;

    private const long BytesPerMegabyte = 1024 * 1024;

    /// <summary>
    /// Initializes a new instance of the AllowedExtensionsAttribute class with the specified supported file extensions.
    /// </summary>
    /// <param name="extensions">The supported file extensions (e.g., ".jpg", ".png").</param>
    /// <exception cref="ArgumentException">Thrown when no extensions are provided.</exception>
    public AllowedExtensionsAttribute(params string[] extensions)
        : this(long.MaxValue, extensions)
    {
    }

    /// <summary>
    /// Initializes a new instance of the AllowedExtensionsAttribute class with the specified maximum file size and supported file extensions.
    /// </summary>
    /// <param name="maxFileSizeInBytes">The maximum file size allowed in bytes.</param>
    /// <param name="extensions">The supported file extensions (e.g., ".jpg", ".png").</param>
    /// <exception cref="ArgumentException">Thrown when no extensions are provided or max file size is invalid.</exception>
    public AllowedExtensionsAttribute(long maxFileSizeInBytes, params string[] extensions)
    {
        if (extensions == null || extensions.Length == 0)
            throw new ArgumentException("At least one file extension must be specified.", nameof(extensions));

        if (maxFileSizeInBytes <= 0)
            throw new ArgumentException("Maximum file size must be greater than zero.", nameof(maxFileSizeInBytes));

        // Normalize extensions to lowercase and ensure they start with '.'
        _extensions = new HashSet<string>(
            extensions.Select(NormalizeExtension),
            StringComparer.OrdinalIgnoreCase);
        
        _maxFileSizeInBytes = maxFileSizeInBytes;
    }

    /// <summary>
    /// Gets the allowed file extensions.
    /// </summary>
    public IReadOnlyCollection<string> AllowedExtensions => _extensions;

    /// <summary>
    /// Gets the maximum allowed file size in bytes.
    /// </summary>
    public long MaxFileSizeInBytes => _maxFileSizeInBytes;

    /// <summary>
    /// Validates the specified value to ensure it is a file with an allowed extension and, if specified, does not exceed the maximum file size.
    /// </summary>
    /// <param name="value">The value to validate. Can be an <see cref="IFormFile"/> or a file path as <see cref="string"/>.</param>
    /// <param name="validationContext">The context information about the validation operation.</param>
    /// <returns>A <see cref="ValidationResult"/> indicating whether validation succeeded or failed.</returns>
    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
            return ValidationResult.Success!;

        return value switch
        {
            IFormFile formFile => ValidateFormFile(formFile),
            string filePath => ValidateFilePath(filePath),
            _ => new ValidationResult("Invalid type. Attribute can only be used for string or IFormFile properties.")
        };
    }

    private ValidationResult ValidateFormFile(IFormFile formFile)
    {
        string? extension = Path.GetExtension(formFile.FileName);

        if (string.IsNullOrEmpty(extension))
            return new ValidationResult("File extension could not be determined.");

        if (!_extensions.Contains(extension))
            return new ValidationResult(GetInvalidExtensionMessage());

        if (formFile.Length > _maxFileSizeInBytes)
            return new ValidationResult(GetFileSizeExceededMessage());

        return ValidationResult.Success!;
    }

    private ValidationResult ValidateFilePath(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return new ValidationResult("File path cannot be empty.");

        string? extension = Path.GetExtension(filePath);

        if (string.IsNullOrEmpty(extension))
            return new ValidationResult("File extension could not be determined.");

        if (!_extensions.Contains(extension))
            return new ValidationResult(GetInvalidExtensionMessage());

        // Only check file size if the file exists
        if (File.Exists(filePath))
        {
            var fileInfo = new FileInfo(filePath);
            if (fileInfo.Length > _maxFileSizeInBytes)
                return new ValidationResult(GetFileSizeExceededMessage());
        }

        return ValidationResult.Success!;
    }

    private static string NormalizeExtension(string extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
            throw new ArgumentException("Extension cannot be null or empty.");

        return extension.StartsWith('.') ? extension.ToLowerInvariant() : $".{extension.ToLowerInvariant()}";
    }

    private string GetInvalidExtensionMessage()
    {
        return ErrorMessage ?? $"Invalid file type. Supported file types: {string.Join(", ", _extensions)}";
    }

    private string GetFileSizeExceededMessage()
    {
        long maxSizeInMb = _maxFileSizeInBytes / BytesPerMegabyte;
        return ErrorMessage ?? $"File size exceeds the maximum limit of {maxSizeInMb} MB.";
    }
}
