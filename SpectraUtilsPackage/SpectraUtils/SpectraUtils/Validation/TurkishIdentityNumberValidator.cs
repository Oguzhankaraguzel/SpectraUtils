using System.Diagnostics.CodeAnalysis;

namespace SpectraUtils.Validation;

/// <summary>
/// Provides utilities for validating 11-digit Turkish National Identification Numbers (TC Kimlik No).
/// </summary>
public static class TurkishIdentityNumberValidator
{
    private const int Length = 11;

    /// <summary>
    /// Validates a Turkish National Identification Number using its two check digits.
    /// </summary>
    /// <param name="identityNumber">An 11-digit identity number. Whitespace is ignored.</param>
    /// <returns><see langword="true"/> when the number is valid; otherwise <see langword="false"/>.</returns>
    /// <remarks>
    /// Placeholder numbers made of one repeated digit (such as <c>11111111110</c>) pass the checksum
    /// but are rejected because they are never issued.
    /// </remarks>
    public static bool IsValid([NotNullWhen(true)] string? identityNumber)
    {
        Span<int> digits = stackalloc int[Length];
        if (!DigitInput.TryRead(identityNumber, digits))
            return false;

        if (digits[0] == 0)
            return false;

        int sameAsFirst = 0;
        foreach (int digit in digits)
        {
            if (digit == digits[0])
                sameAsFirst++;
        }

        if (sameAsFirst >= Length - 1)
            return false;

        int oddSum = digits[0] + digits[2] + digits[4] + digits[6] + digits[8];
        int evenSum = digits[1] + digits[3] + digits[5] + digits[7];

        // ((oddSum * 7) - evenSum) can be negative, so normalize the modulo result.
        int expectedTenth = ((oddSum * 7 - evenSum) % 10 + 10) % 10;
        if (digits[9] != expectedTenth)
            return false;

        int expectedEleventh = (oddSum + evenSum + digits[9]) % 10;
        return digits[10] == expectedEleventh;
    }
}
