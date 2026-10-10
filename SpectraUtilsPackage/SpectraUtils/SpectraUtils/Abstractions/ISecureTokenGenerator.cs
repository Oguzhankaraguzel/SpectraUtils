namespace SpectraUtils.Abstractions;

/// <summary>
/// Generates cryptographically secure tokens.
/// </summary>
public interface ISecureTokenGenerator
{
    /// <summary>
    /// Generates a Base64Url-encoded token (RFC 4648, without padding).
    /// </summary>
    /// <param name="byteLength">Number of random bytes before encoding (1 to 1024). Default is 32 bytes.</param>
    /// <returns>Base64Url token.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="byteLength"/> is outside the supported range.</exception>
    string GenerateToken(int byteLength = 32);
}
