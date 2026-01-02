namespace SpectraUtils.Validation;

/// <summary>
/// Provides utilities for validating 10-digit Turkish Tax Identification Numbers (VKN).
/// </summary>
public static class TurkishTaxNumberValidator
{
    /// <summary>
    /// Validates a Turkish Tax Identification Number (VKN).
    /// </summary>
    /// <param name="taxNumber">A 10-digit VKN. Whitespaces are ignored.</param>
    /// <returns><see langword="true"/> when the VKN is valid; otherwise <see langword="false"/>.</returns>
    public static bool IsValid(string taxNumber)
    {
        if (string.IsNullOrWhiteSpace(taxNumber))
            return false;

        // Remove whitespaces.
        Span<char> buffer = stackalloc char[taxNumber.Length];
        int len = 0;
        foreach (char c in taxNumber)
        {
            if (!char.IsWhiteSpace(c))
                buffer[len++] = c;
        }

        ReadOnlySpan<char> vkn = buffer[..len];

        if (vkn.Length != 10)
            return false;

        int[] digits = new int[10];

        for (int i = 0; i < 10; i++)
        {
            char c = vkn[i];
            if (c < '0' || c > '9')
                return false;

            digits[i] = c - '0';
        }

        // VKN algorithm:
        // For i=0..8:
        //   temp = (digit[i] + (9 - i)) % 10
        //   temp = (temp * 2^(9-i)) % 9  (if temp != 0)
        // Sum temps, and check digit is (10 - (sum % 10)) % 10.

        int sum = 0;

        for (int i = 0; i < 9; i++)
        {
            int temp = (digits[i] + (9 - i)) % 10;

            if (temp != 0)
            {
                int pow = Pow2Mod9(9 - i);
                temp = (temp * pow) % 9;
            }

            sum += temp;
        }

        int checkDigit = (10 - (sum % 10)) % 10;
        return digits[9] == checkDigit;
    }

    /// <summary>
    /// Computes (2^exp) mod 9.
    /// </summary>
    private static int Pow2Mod9(int exp)
    {
        int result = 1;
        int baseVal = 2;
        int e = exp;

        while (e > 0)
        {
            if ((e & 1) == 1)
                result = (result * baseVal) % 9;

            baseVal = (baseVal * baseVal) % 9;
            e >>= 1;
        }

        return result;
    }
}
