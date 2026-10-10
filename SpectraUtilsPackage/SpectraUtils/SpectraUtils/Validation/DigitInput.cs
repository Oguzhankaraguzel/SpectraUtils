namespace SpectraUtils.Validation;

/// <summary>
/// Helpers for reading fixed-length numeric identifiers from user input.
/// </summary>
internal static class DigitInput
{
    /// <summary>
    /// Upper bound for trimmed input (including inner whitespace). Longer input is rejected without being scanned,
    /// so validators never allocate or recurse proportionally to attacker-controlled length.
    /// </summary>
    public const int MaxInputLength = 64;

    /// <summary>
    /// Copies the digits of <paramref name="input"/> into <paramref name="digits"/>, ignoring whitespace.
    /// Fails when the input is too long, contains anything other than ASCII digits and whitespace,
    /// or does not contain exactly <c>digits.Length</c> digits.
    /// </summary>
    public static bool TryRead(string? input, Span<int> digits)
    {
        if (input is null)
            return false;

        ReadOnlySpan<char> trimmed = input.AsSpan().Trim();
        if (trimmed.Length > MaxInputLength)
            return false;

        int count = 0;

        foreach (char c in trimmed)
        {
            if (char.IsWhiteSpace(c))
                continue;

            if (!char.IsAsciiDigit(c) || count == digits.Length)
                return false;

            digits[count++] = c - '0';
        }

        return count == digits.Length;
    }
}
