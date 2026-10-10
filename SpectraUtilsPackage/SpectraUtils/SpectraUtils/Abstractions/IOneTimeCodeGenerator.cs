namespace SpectraUtils.Abstractions;

/// <summary>
/// Provides one-time-password (OTP) generation utilities.
/// </summary>
public interface IOneTimeCodeGenerator
{
    /// <summary>
    /// Generates a numeric one-time code. Every digit is drawn independently from a
    /// cryptographically secure random source, so all codes of the given length are equally likely.
    /// </summary>
    /// <param name="digits">Number of digits (1 to 32). Default is 6.</param>
    /// <returns>The OTP code; it may start with zeros.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="digits"/> is outside the supported range.</exception>
    string GenerateNumericCode(int digits = 6);
}
