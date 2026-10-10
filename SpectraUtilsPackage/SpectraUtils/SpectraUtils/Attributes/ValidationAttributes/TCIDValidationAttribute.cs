using SpectraUtils.Validation;
using System.ComponentModel.DataAnnotations;

namespace SpectraUtils.Attributes.ValidationAttributes;

/// <summary>
/// Validates that a string property contains a valid Turkish National Identification Number (TC Kimlik No).
/// The value is mandatory: <see langword="null"/>, empty or whitespace values are invalid, so no separate
/// <see cref="RequiredAttribute"/> is needed.
/// </summary>
/// <remarks>The error message supports the <c>{0}</c> placeholder for the display name.</remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class TCIDValidationAttribute : ValidationAttribute
{
    private const string DefaultErrorMessage = "The {0} field is not a valid Turkish identity number.";

    /// <summary>
    /// Initializes a new instance of the <see cref="TCIDValidationAttribute"/> class.
    /// </summary>
    public TCIDValidationAttribute()
        : base(DefaultErrorMessage)
    {
    }

    /// <inheritdoc />
    public override bool IsValid(object? value)
        => value is string identityNumber && TurkishIdentityNumberValidator.IsValid(identityNumber);
}
