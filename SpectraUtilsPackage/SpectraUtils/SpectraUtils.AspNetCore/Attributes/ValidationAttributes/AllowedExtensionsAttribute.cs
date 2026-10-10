using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace SpectraUtils.Attributes.ValidationAttributes;

/// <summary>
/// Validates uploaded files by extension and, optionally, by maximum size.
/// Supports <see cref="IFormFile"/>, <see cref="IFormFileCollection"/> / <see cref="IEnumerable{T}"/> of <see cref="IFormFile"/>,
/// and file names given as <see cref="string"/> (extension only).
/// <see langword="null"/> values are considered valid; combine with <see cref="RequiredAttribute"/> when a file is mandatory.
/// </summary>
/// <remarks>
/// <para>
/// Only the file name is inspected: a renamed executable with a <c>.png</c> extension passes. Verify the
/// file content (e.g. magic bytes) as well when the files are processed or served to other users.
/// </para>
/// <para>
/// <see cref="ValidationAttribute.ErrorMessage"/> is used for extension errors, <see cref="FileSizeErrorMessage"/> for size errors
/// (falling back to <see cref="ValidationAttribute.ErrorMessage"/>). Both support <c>{0}</c> (display name),
/// <c>{1}</c> (allowed extensions) and <c>{2}</c> (maximum size) placeholders.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class AllowedExtensionsAttribute : ValidationAttribute
{
    private const string DefaultExtensionMessage = "The {0} field only accepts files with the following extensions: {1}.";
    private const string DefaultFileSizeMessage = "The {0} field exceeds the maximum file size of {2}.";

    private readonly HashSet<string> _extensions;

    /// <summary>
    /// Initializes a new instance that only checks extensions.
    /// </summary>
    /// <param name="extensions">The supported file extensions (e.g., ".jpg", "png").</param>
    /// <exception cref="ArgumentException">Thrown when no extensions are provided or an extension is empty.</exception>
    public AllowedExtensionsAttribute(params string[] extensions)
        : this(long.MaxValue, extensions)
    {
    }

    /// <summary>
    /// Initializes a new instance that checks extensions and the maximum file size.
    /// </summary>
    /// <param name="maxFileSizeInBytes">The maximum file size allowed in bytes.</param>
    /// <param name="extensions">The supported file extensions (e.g., ".jpg", "png").</param>
    /// <exception cref="ArgumentException">Thrown when no extensions are provided, an extension is empty, or the size is not positive.</exception>
    public AllowedExtensionsAttribute(long maxFileSizeInBytes, params string[] extensions)
        : base(DefaultExtensionMessage)
    {
        if (extensions is null || extensions.Length == 0)
            throw new ArgumentException("At least one file extension must be specified.", nameof(extensions));

        if (maxFileSizeInBytes <= 0)
            throw new ArgumentException("Maximum file size must be greater than zero.", nameof(maxFileSizeInBytes));

        _extensions = new HashSet<string>(extensions.Select(NormalizeExtension), StringComparer.OrdinalIgnoreCase);
        MaxFileSizeInBytes = maxFileSizeInBytes;
    }

    /// <summary>
    /// Gets the allowed file extensions (lowercase, with a leading dot).
    /// </summary>
    public IReadOnlyCollection<string> AllowedExtensions => _extensions;

    /// <summary>
    /// Gets the maximum allowed file size in bytes (<see cref="long.MaxValue"/> means unlimited).
    /// </summary>
    public long MaxFileSizeInBytes { get; }

    /// <summary>
    /// Gets or sets the error message used when a file is too large.
    /// </summary>
    public string? FileSizeErrorMessage { get; set; }

    /// <inheritdoc />
    public override string FormatErrorMessage(string name) => Format(ErrorMessageString, name);

    /// <inheritdoc />
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        // validationContext is null when the attribute is used through IsValid(object).
        ValidationContext? context = validationContext;
        string displayName = context?.DisplayName ?? "file";

        switch (value)
        {
            case null:
                return ValidationResult.Success;

            case IFormFile file:
                return ValidateFile(file, displayName, context);

            case IEnumerable<IFormFile> files:
                foreach (IFormFile file in files)
                {
                    ValidationResult? result = ValidateFile(file, displayName, context);
                    if (result != ValidationResult.Success)
                        return result;
                }

                return ValidationResult.Success;

            case string fileName:
                return HasAllowedExtension(fileName)
                    ? ValidationResult.Success
                    : new ValidationResult(FormatErrorMessage(displayName), MemberNames(context));

            default:
                return new ValidationResult(
                    $"The {displayName} field must be an IFormFile, a collection of IFormFile or a file name.",
                    MemberNames(context));
        }
    }

    private ValidationResult? ValidateFile(IFormFile? file, string displayName, ValidationContext? context)
    {
        if (file is null || !HasAllowedExtension(file.FileName))
            return new ValidationResult(FormatErrorMessage(displayName), MemberNames(context));

        if (file.Length > MaxFileSizeInBytes)
            return new ValidationResult(Format(FileSizeErrorMessage ?? ErrorMessage ?? DefaultFileSizeMessage, displayName), MemberNames(context));

        return ValidationResult.Success;
    }

    private bool HasAllowedExtension(string? fileName)
    {
        string extension = Path.GetExtension(fileName ?? string.Empty);
        return extension.Length > 0 && _extensions.Contains(extension);
    }

    private string Format(string template, string displayName)
        => string.Format(CultureInfo.CurrentCulture, template, displayName, string.Join(", ", _extensions), FormatSize(MaxFileSizeInBytes));

    private static string NormalizeExtension(string extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
            throw new ArgumentException("Extension cannot be null or empty.", nameof(extension));

        string trimmed = extension.Trim().ToLowerInvariant();
        return trimmed.StartsWith('.') ? trimmed : "." + trimmed;
    }

    internal static string FormatSize(long bytes)
    {
        const double KB = 1024;
        const double MB = KB * 1024;
        const double GB = MB * 1024;

        return bytes switch
        {
            long.MaxValue => "unlimited",
            >= (long)GB => string.Format(CultureInfo.CurrentCulture, "{0:0.##} GB", bytes / GB),
            >= (long)MB => string.Format(CultureInfo.CurrentCulture, "{0:0.##} MB", bytes / MB),
            >= (long)KB => string.Format(CultureInfo.CurrentCulture, "{0:0.##} KB", bytes / KB),
            _ => string.Format(CultureInfo.CurrentCulture, "{0} bytes", bytes),
        };
    }

    private static string[]? MemberNames(ValidationContext? context)
        => context?.MemberName is { } memberName ? [memberName] : null;
}
