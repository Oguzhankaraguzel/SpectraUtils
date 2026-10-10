using SpectraUtils.Validation;
using System.ComponentModel.DataAnnotations;

namespace SpectraUtils.Attributes.ValidationAttributes;

/// <summary>
/// Validates that a string property contains a valid Turkish Tax Identification Number (VKN).
/// <see langword="null"/> values are considered valid; combine with <see cref="RequiredAttribute"/> when the value is mandatory.
/// </summary>
/// <remarks>The error message supports the <c>{0}</c> placeholder for the display name.</remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class TurkishTaxNumberValidationAttribute : ValidationAttribute
{
    private const string DefaultErrorMessage = "The {0} field is not a valid Turkish tax number (VKN).";

    /// <summary>
    /// Initializes a new instance of the <see cref="TurkishTaxNumberValidationAttribute"/> class.
    /// </summary>
    public TurkishTaxNumberValidationAttribute()
        : base(DefaultErrorMessage)
    {
    }

    /// <inheritdoc />
    public override bool IsValid(object? value) => value switch
    {
        null => true,
        string taxNumber => TurkishTaxNumberValidator.IsValid(taxNumber),
        _ => false,
    };
}
