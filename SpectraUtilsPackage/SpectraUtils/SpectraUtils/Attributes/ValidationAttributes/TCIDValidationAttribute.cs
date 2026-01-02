using System.ComponentModel.DataAnnotations;

namespace SpectraUtils.Attributes.ValidationAttributes;

/// <summary>
/// Validation attribute for verifying Turkish National Identification Numbers (TC Kimlik No).
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class TCIDValidationAttribute : ValidationAttribute
{
    private const int TcIdLength = 11;
    private const string DefaultErrorMessage = "You must write a valid ID!";
    private const string InvalidIdErrorMessage = "Invalid ID!";

    /// <summary>
    /// Determines whether the specified value is a valid Turkish National Identification Number.
    /// </summary>
    /// <param name="value">The value of the object to validate.</param>
    /// <param name="validationContext">The context information about the validation operation.</param>
    /// <returns>A <see cref="ValidationResult"/> indicating whether validation succeeded.</returns>
    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string tcKimlik || string.IsNullOrEmpty(tcKimlik))
        {
            return new ValidationResult(ErrorMessage ?? DefaultErrorMessage);
        }

        return IsValidTcKimlik(tcKimlik)
            ? ValidationResult.Success!
            : new ValidationResult(ErrorMessage ?? InvalidIdErrorMessage);
    }

    /// <summary>
    /// Checks if the given string is a valid Turkish National Identification Number.
    /// </summary>
    /// <param name="tcKimlik">The identification number to validate.</param>
    /// <returns>True if valid, otherwise false.</returns>
    private static bool IsValidTcKimlik(ReadOnlySpan<char> tcKimlik)
    {
        // Length check and first digit cannot be '0'
        if (tcKimlik.Length != TcIdLength || tcKimlik[0] == '0')
            return false;

        int oddSum = 0;  // Sum of digits at odd positions (1, 3, 5, 7, 9)
        int evenSum = 0; // Sum of digits at even positions (2, 4, 6, 8)
        int totalSum = 0;

        for (int i = 0; i < TcIdLength; i++)
        {
            char c = tcKimlik[i];
            if (c < '0' || c > '9')
                return false;

            int digit = c - '0';

            if (i < 9)
            {
                if (i % 2 == 0)
                    oddSum += digit;
                else
                    evenSum += digit;
            }

            if (i < 10)
                totalSum += digit;
        }

        // 10th digit validation: ((oddSum * 7) - evenSum) % 10 == 10th digit
        int tenthDigit = tcKimlik[9] - '0';
        int expectedTenth = ((oddSum * 7) - evenSum) % 10;
        if (expectedTenth < 0)
            expectedTenth += 10;

        if (expectedTenth != tenthDigit)
            return false;

        // 11th digit validation: totalSum % 10 == 11th digit
        int eleventhDigit = tcKimlik[10] - '0';
        return (totalSum % 10) == eleventhDigit;
    }
}
