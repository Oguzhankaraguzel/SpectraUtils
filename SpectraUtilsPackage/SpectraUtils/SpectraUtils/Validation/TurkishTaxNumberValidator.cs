using System.Diagnostics.CodeAnalysis;

namespace SpectraUtils.Validation;

/// <summary>
/// Provides utilities for validating 10-digit Turkish Tax Identification Numbers (VKN).
/// </summary>
public static class TurkishTaxNumberValidator
{
    private const int Length = 10;

    // 2^(9-i) for i = 0..8.
    private static ReadOnlySpan<int> PowersOfTwo => [512, 256, 128, 64, 32, 16, 8, 4, 2];

    /// <summary>
    /// Validates a Turkish Tax Identification Number (VKN) using the official check digit algorithm.
    /// </summary>
    /// <param name="taxNumber">A 10-digit VKN. Whitespace is ignored.</param>
    /// <returns><see langword="true"/> when the VKN is valid; otherwise <see langword="false"/>.</returns>
    public static bool IsValid([NotNullWhen(true)] string? taxNumber)
    {
        Span<int> digits = stackalloc int[Length];
        if (!DigitInput.TryRead(taxNumber, digits))
            return false;

        int sum = 0;

        for (int i = 0; i < 9; i++)
        {
            int temp = (digits[i] + 9 - i) % 10;
            if (temp == 0)
                continue;

            int weighted = temp * PowersOfTwo[i] % 9;

            // A non-zero intermediate value whose weighted result is divisible by 9 counts as 9, not 0.
            sum += weighted == 0 ? 9 : weighted;
        }

        int checkDigit = (10 - sum % 10) % 10;
        return digits[9] == checkDigit;
    }
}
