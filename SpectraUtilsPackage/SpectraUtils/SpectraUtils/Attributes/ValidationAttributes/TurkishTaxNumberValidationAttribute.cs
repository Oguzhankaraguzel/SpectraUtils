using System.ComponentModel.DataAnnotations;
using SpectraUtils.Validation;

namespace SpectraUtils.Attributes.ValidationAttributes;

/// <summary>
/// Validation attribute for verifying Turkish Tax Identification Numbers (VKN).
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class TurkishTaxNumberValidationAttribute : ValidationAttribute
{
    private const string DefaultErrorMessage = "Invalid Turkish Tax Number (VKN).";

    /// <inheritdoc />
    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
            return ValidationResult.Success!;

        if (value is not string vkn || string.IsNullOrWhiteSpace(vkn))
            return new ValidationResult(ErrorMessage ?? DefaultErrorMessage);

        return TurkishTaxNumberValidator.IsValid(vkn)
            ? ValidationResult.Success!
            : new ValidationResult(ErrorMessage ?? DefaultErrorMessage);
    }
}
