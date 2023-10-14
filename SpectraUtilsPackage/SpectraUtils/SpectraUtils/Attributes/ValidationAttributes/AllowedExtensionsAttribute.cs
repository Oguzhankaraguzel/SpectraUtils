using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class AllowedExtensionsAttribute : ValidationAttribute
{
    private readonly string[] _extensions;
    private readonly long _maxFileSizeInBytes;

    /// <summary>
    /// Initializes a new instance of the AllowedExtensionsAttribute class with the specified supported file extensions.
    /// </summary>
    /// <param name="extensions">The supported file extensions.</param>
    public AllowedExtensionsAttribute(params string[] extensions)
    {
        _extensions = extensions;
        _maxFileSizeInBytes = long.MaxValue;
    }

    /// <summary>
    /// Initializes a new instance of the AllowedExtensionsAttribute class with the specified maximum file size and supported file extensions.
    /// </summary>
    /// <param name="maxFileSizeInBytes">The maximum file size allowed in bytes.</param>
    /// <param name="extensions">The supported file extensions.</param>
    public AllowedExtensionsAttribute(long maxFileSizeInBytes, params string[] extensions)
    {
        _extensions = extensions;
        _maxFileSizeInBytes = maxFileSizeInBytes;
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value == null)
            return ValidationResult.Success;

        if (value is string filePath)
        {
            string fileExtension = Path.GetExtension(filePath)?.ToLower();

            if (!_extensions.Contains(fileExtension))
                return new ValidationResult($"Invalid file type. Supported file types: {string.Join(", ", _extensions)}");

            FileInfo fileInfo = new FileInfo(filePath);
            long fileSizeInBytes = fileInfo.Length;

            if (fileSizeInBytes > _maxFileSizeInBytes)
                return new ValidationResult($"File size exceeds the maximum limit of {_maxFileSizeInBytes / (1024 * 1024)} MB.");
        }
        else if (value is IFormFile formFile)
        {
            string fileExtension = Path.GetExtension(formFile.FileName)?.ToLower();

            if (!_extensions.Contains(fileExtension))
                return new ValidationResult($"Invalid file type. Supported file types: {string.Join(", ", _extensions)}");

            long fileSizeInBytes = formFile.Length;

            if (fileSizeInBytes > _maxFileSizeInBytes)
                return new ValidationResult($"File size exceeds the maximum limit of {_maxFileSizeInBytes / (1024 * 1024)} MB.");
        }
        else
        {
            throw new ArgumentException("Invalid type. Attribute can only be used for string or IFormFile properties.");
        }

        return ValidationResult.Success;
    }

}
