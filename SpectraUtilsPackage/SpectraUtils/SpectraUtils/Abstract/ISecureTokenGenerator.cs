namespace SpectraUtils.Abstract;

/// <summary>
/// Generates cryptographically secure tokens.
/// </summary>
public interface ISecureTokenGenerator
{
    /// <summary>
    /// Generates a Base64Url-encoded token.
    /// </summary>
    /// <param name="byteLength">Number of random bytes (before encoding). Default is 32 bytes.</param>
    /// <returns>Base64Url token.</returns>
    string GenerateToken(int byteLength = 32);
}
