namespace SpectraUtils.Abstract;

/// <summary>
/// Provides one-time-password (OTP) generation utilities.
/// </summary>
public interface IOneTimeCodeGenerator
{
    /// <summary>
    /// Generates a numeric one-time code.
    /// </summary>
    /// <param name="digits">Number of digits. Default is 6.</param>
    /// <returns>OTP code as string (zero-padded).</returns>
    string GenerateNumericCode(int digits = 6);
}
