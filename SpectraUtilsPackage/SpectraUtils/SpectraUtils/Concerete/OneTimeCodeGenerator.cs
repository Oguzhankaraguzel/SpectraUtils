using SpectraUtils.Abstract;
using System.Security.Cryptography;

namespace SpectraUtils.Concerete;

/// <summary>
/// Generates random numeric OTP codes.
/// </summary>
public sealed class OneTimeCodeGenerator : IOneTimeCodeGenerator
{
    /// <inheritdoc />
    public string GenerateNumericCode(int digits = 6)
    {
        if (digits <= 0)
            throw new ArgumentOutOfRangeException(nameof(digits));
        if (digits > 18)
            throw new ArgumentOutOfRangeException(nameof(digits), "Digits value is too large.");

        // max: 10^digits
        long maxExclusive = 1;
        for (int i = 0; i < digits; i++)
            maxExclusive *= 10;

        long value = RandomNumberGenerator.GetInt32((int)Math.Min(int.MaxValue, maxExclusive));

        // If digits > 9, GetInt32 cannot cover full range. Keep it simple and safe.
        // For library use-cases, 6-8 digits is typical.
        if (maxExclusive > int.MaxValue)
            value = RandomNumberGenerator.GetInt32(int.MaxValue);

        return value.ToString().PadLeft(digits, '0');
    }
}
