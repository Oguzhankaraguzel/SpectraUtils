using SpectraUtils.Abstract;
using System.Security.Cryptography;

namespace SpectraUtils.Concerete;

/// <summary>
/// Generates cryptographically secure Base64Url tokens.
/// </summary>
public sealed class SecureTokenGenerator : ISecureTokenGenerator
{
    /// <inheritdoc />
    public string GenerateToken(int byteLength = 32)
    {
        if (byteLength <= 0)
            throw new ArgumentOutOfRangeException(nameof(byteLength));

        byte[] bytes = RandomNumberGenerator.GetBytes(byteLength);
        return Base64UrlEncode(bytes);
    }

    /// <summary>
    /// Encodes bytes as Base64Url (RFC 4648) without padding.
    /// </summary>
    private static string Base64UrlEncode(byte[] data)
    {
        string base64 = Convert.ToBase64String(data);

        // Convert to base64url.
        return base64
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }
}
