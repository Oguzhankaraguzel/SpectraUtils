using SpectraUtils.Validation;
using System.ComponentModel.DataAnnotations;

namespace SpectraUtils.Attributes.ValidationAttributes;

/// <summary>
/// Validates that a string property contains a valid International Bank Account Number (IBAN).
/// <see langword="null"/> values are considered valid; combine with <see cref="RequiredAttribute"/> when the value is mandatory.
/// </summary>
/// <remarks>The error message supports the <c>{0}</c> placeholder for the display name.</remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class IbanValidationAttribute : ValidationAttribute
{
    private const string DefaultErrorMessage = "The {0} field is not a valid IBAN.";

    /// <summary>
    /// Initializes a new instance of the <see cref="IbanValidationAttribute"/> class that accepts IBANs from any country.
    /// </summary>
    public IbanValidationAttribute()
        : base(DefaultErrorMessage)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="IbanValidationAttribute"/> class that only accepts IBANs from the given country.
    /// </summary>
    /// <param name="countryCode">Two-letter ISO country code (e.g., "TR").</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="countryCode"/> is not two ASCII letters.</exception>
    public IbanValidationAttribute(string countryCode)
        : this()
    {
        if (countryCode is null || countryCode.Length != 2 || !char.IsAsciiLetter(countryCode[0]) || !char.IsAsciiLetter(countryCode[1]))
            throw new ArgumentException("Country code must be a two-letter ISO country code, e.g. \"TR\".", nameof(countryCode));

        CountryCode = countryCode.ToUpperInvariant();
    }

    /// <summary>
    /// When specified, validates that the IBAN belongs to the given country code (e.g., "TR").
    /// </summary>
    public string? CountryCode { get; }

    /// <inheritdoc />
    public override bool IsValid(object? value) => value switch
    {
        null => true,
        string iban => IbanValidator.IsValid(iban, CountryCode),
        _ => false,
    };
}
