using SpectraUtils.Abstractions;
using System.Security.Cryptography;

namespace SpectraUtils.Services;

/// <summary>
/// Generates uniformly distributed numeric OTP codes using a cryptographically secure random source.
/// </summary>
public sealed class OneTimeCodeGenerator : IOneTimeCodeGenerator
{
    /// <summary>Maximum supported number of digits.</summary>
    public const int MaxDigits = 32;

    private const string DigitChars = "0123456789";

    /// <inheritdoc />
    public string GenerateNumericCode(int digits = 6)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(digits);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(digits, MaxDigits);

        return RandomNumberGenerator.GetString(DigitChars, digits);
    }
}
