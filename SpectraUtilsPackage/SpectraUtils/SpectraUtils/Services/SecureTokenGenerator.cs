using SpectraUtils.Abstractions;
using System.Security.Cryptography;

namespace SpectraUtils.Services;

/// <summary>
/// Generates cryptographically secure Base64Url tokens.
/// </summary>
public sealed class SecureTokenGenerator : ISecureTokenGenerator
{
    /// <summary>Maximum supported number of random bytes.</summary>
    public const int MaxByteLength = 1024;

    /// <inheritdoc />
    public string GenerateToken(int byteLength = 32)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(byteLength);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(byteLength, MaxByteLength);

        Span<byte> bytes = stackalloc byte[byteLength];
        RandomNumberGenerator.Fill(bytes);

        try
        {
#if NET9_0_OR_GREATER
            return System.Buffers.Text.Base64Url.EncodeToString(bytes);
#else
            return Convert.ToBase64String(bytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
#endif
        }
        finally
        {
            CryptographicOperations.ZeroMemory(bytes);
        }
    }
}
