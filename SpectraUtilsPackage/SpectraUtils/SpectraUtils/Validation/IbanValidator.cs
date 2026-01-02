using System.Globalization;

namespace SpectraUtils.Validation;

/// <summary>
/// Provides utilities for validating International Bank Account Numbers (IBAN).
/// </summary>
public static class IbanValidator
{
    /// <summary>
    /// Validates the provided IBAN using the standard MOD-97 algorithm.
    /// </summary>
    /// <param name="iban">IBAN value. Whitespaces are ignored.</param>
    /// <param name="expectedCountryCode">Optional. If provided, enforces the IBAN starts with this 2-letter country code (e.g., "TR").</param>
    /// <returns><see langword="true"/> when the IBAN is valid; otherwise <see langword="false"/>.</returns>
    public static bool IsValid(string iban, string? expectedCountryCode = null)
    {
        if (string.IsNullOrWhiteSpace(iban))
            return false;

        // Remove whitespaces for user-friendly input support.
        Span<char> buffer = stackalloc char[iban.Length];
        int len = 0;
        foreach (char c in iban)
        {
            if (!char.IsWhiteSpace(c))
                buffer[len++] = char.ToUpperInvariant(c);
        }

        ReadOnlySpan<char> normalized = buffer[..len];

        // Minimal IBAN length is 15, maximum is 34.
        if (normalized.Length is < 15 or > 34)
            return false;

        // Optional: enforce country code.
        if (!string.IsNullOrWhiteSpace(expectedCountryCode))
        {
            string cc = expectedCountryCode.Trim().ToUpperInvariant();
            if (cc.Length != 2)
                return false;

            if (!normalized.StartsWith(cc.AsSpan(), StringComparison.Ordinal))
                return false;
        }

        // IBAN format: 2 letters country, 2 digits check, rest alphanumeric.
        if (!char.IsLetter(normalized[0]) || !char.IsLetter(normalized[1]))
            return false;
        if (!char.IsDigit(normalized[2]) || !char.IsDigit(normalized[3]))
            return false;

        for (int i = 4; i < normalized.Length; i++)
        {
            char c = normalized[i];
            if (!char.IsLetterOrDigit(c))
                return false;
        }

        // Move first 4 characters to the end.
        Span<char> rearranged = stackalloc char[normalized.Length];
        normalized[4..].CopyTo(rearranged);
        normalized[..4].CopyTo(rearranged[(normalized.Length - 4)..]);

        // Compute MOD-97 iteratively to avoid big integer conversions.
        int remainder = 0;

        foreach (char c in rearranged)
        {
            if (char.IsDigit(c))
            {
                remainder = (remainder * 10 + (c - '0')) % 97;
                continue;
            }

            // A=10 ... Z=35
            int value = c - 'A' + 10;
            remainder = (remainder * 10 + (value / 10)) % 97;
            remainder = (remainder * 10 + (value % 10)) % 97;
        }

        return remainder == 1;
    }
}
