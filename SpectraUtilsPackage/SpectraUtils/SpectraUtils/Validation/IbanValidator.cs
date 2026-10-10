using System.Diagnostics.CodeAnalysis;

namespace SpectraUtils.Validation;

/// <summary>
/// Provides utilities for validating International Bank Account Numbers (IBAN).
/// </summary>
public static class IbanValidator
{
    /// <summary>Shortest IBAN length in the registry.</summary>
    public const int MinLength = 15;

    /// <summary>Longest IBAN length allowed by ISO 13616.</summary>
    public const int MaxLength = 34;

    // Input limit (after trimming, including inner whitespace) checked before anything is copied to the stack.
    private const int MaxInputLength = 64;

    // Country code + total IBAN length, from the SWIFT IBAN registry.
    // Countries missing here are still accepted when their length is between MinLength and MaxLength.
    private const string RegistryLengths =
        "AD24 AE23 AL28 AT20 AZ28 BA20 BE16 BG22 BH22 BI27 BR29 BY28 CH21 CR22 CY28 CZ24 DE22 DJ27 DK18 DO28 " +
        "EE20 EG29 ES24 FI18 FK18 FO18 FR27 GB22 GE22 GI23 GL18 GR27 GT28 HR21 HU28 IE22 IL23 IQ23 IS26 IT27 " +
        "JO30 KW30 KZ20 LB28 LC32 LI21 LT20 LU20 LV21 LY25 MC27 MD24 ME22 MK19 MN20 MR27 MT31 MU30 NI28 NL18 " +
        "NO15 OM23 PK24 PL28 PS29 PT25 QA29 RO24 RS22 RU33 SA24 SC31 SD18 SE24 SI19 SK24 SM27 SO23 ST25 SV28 " +
        "TL23 TN24 TR26 UA29 VA22 VG24 XK20 YE30";

    private static readonly Dictionary<int, int> CountryLengths = ParseRegistry(RegistryLengths);

    /// <summary>
    /// Validates the provided IBAN: format, country-specific length and the MOD-97 check digits.
    /// </summary>
    /// <param name="iban">IBAN value. Whitespace is ignored and lowercase letters are accepted.</param>
    /// <param name="expectedCountryCode">Optional. If provided, enforces the IBAN starts with this 2-letter country code (e.g., "TR").</param>
    /// <returns><see langword="true"/> when the IBAN is valid; otherwise <see langword="false"/>.</returns>
    public static bool IsValid([NotNullWhen(true)] string? iban, string? expectedCountryCode = null)
    {
        if (iban is null)
            return false;

        ReadOnlySpan<char> input = iban.AsSpan().Trim();
        if (input.Length > MaxInputLength)
            return false;

        Span<char> buffer = stackalloc char[MaxInputLength];
        int length = 0;

        foreach (char c in input)
        {
            if (char.IsWhiteSpace(c))
                continue;

            if (!char.IsAsciiLetterOrDigit(c))
                return false;

            buffer[length++] = char.IsAsciiLetterLower(c) ? (char)(c - ('a' - 'A')) : c;
        }

        ReadOnlySpan<char> normalized = buffer[..length];

        if (normalized.Length is < MinLength or > MaxLength)
            return false;

        if (!char.IsAsciiLetter(normalized[0]) || !char.IsAsciiLetter(normalized[1])
            || !char.IsAsciiDigit(normalized[2]) || !char.IsAsciiDigit(normalized[3]))
            return false;

        if (!string.IsNullOrWhiteSpace(expectedCountryCode)
            && !normalized[..2].Equals(expectedCountryCode.AsSpan().Trim(), StringComparison.OrdinalIgnoreCase))
            return false;

        if (CountryLengths.TryGetValue(CountryKey(normalized[0], normalized[1]), out int expectedLength)
            && normalized.Length != expectedLength)
            return false;

        // ISO 7064 MOD 97-10: move the first four characters to the end, then compute the remainder.
        int remainder = Mod97(normalized[4..], 0);
        remainder = Mod97(normalized[..4], remainder);

        return remainder == 1;
    }

    private static int Mod97(ReadOnlySpan<char> value, int remainder)
    {
        foreach (char c in value)
        {
            if (char.IsAsciiDigit(c))
            {
                remainder = (remainder * 10 + (c - '0')) % 97;
            }
            else
            {
                // A=10 ... Z=35, i.e. always two decimal digits.
                remainder = (remainder * 100 + (c - 'A' + 10)) % 97;
            }
        }

        return remainder;
    }

    private static int CountryKey(char first, char second) => (first << 8) | second;

    private static Dictionary<int, int> ParseRegistry(string registry)
    {
        var lengths = new Dictionary<int, int>();

        foreach (string entry in registry.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            lengths[CountryKey(entry[0], entry[1])] = int.Parse(entry.AsSpan(2), provider: System.Globalization.CultureInfo.InvariantCulture);

        return lengths;
    }
}
