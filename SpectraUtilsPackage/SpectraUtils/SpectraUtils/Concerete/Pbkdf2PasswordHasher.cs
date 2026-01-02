using SpectraUtils.Abstract;
using System.Security.Cryptography;
using System.Text;

namespace SpectraUtils.Concerete;

/// <summary>
/// Password hasher based on PBKDF2 (RFC 2898).
/// 
/// Format: <c>PBKDF2$SHA256$iterations$saltBase64$hashBase64</c>
/// </summary>
public sealed class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const string Prefix = "PBKDF2";
    private const string Algorithm = "SHA256";

    private readonly int _iterations;
    private readonly int _saltSize;
    private readonly int _keySize;

    /// <summary>
    /// Initializes a new instance of the <see cref="Pbkdf2PasswordHasher"/> class.
    /// </summary>
    /// <param name="iterations">PBKDF2 iterations. Defaults to 210_000 (reasonable for modern systems).</param>
    /// <param name="saltSize">Salt size in bytes. Defaults to 16.</param>
    /// <param name="keySize">Derived key size in bytes. Defaults to 32 (256-bit).</param>
    public Pbkdf2PasswordHasher(int iterations = 210_000, int saltSize = 16, int keySize = 32)
    {
        if (iterations <= 0)
            throw new ArgumentOutOfRangeException(nameof(iterations));
        if (saltSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(saltSize));
        if (keySize <= 0)
            throw new ArgumentOutOfRangeException(nameof(keySize));

        _iterations = iterations;
        _saltSize = saltSize;
        _keySize = keySize;
    }

    /// <inheritdoc />
    public string Hash(string password)
    {
        ArgumentNullException.ThrowIfNull(password);

        byte[] salt = RandomNumberGenerator.GetBytes(_saltSize);

        byte[] key = Rfc2898DeriveBytes.Pbkdf2(
            password: Encoding.UTF8.GetBytes(password),
            salt: salt,
            iterations: _iterations,
            hashAlgorithm: HashAlgorithmName.SHA256,
            outputLength: _keySize);

        // PBKDF2$SHA256$iterations$salt$hash
        return string.Join('$',
            Prefix,
            Algorithm,
            _iterations.ToString(),
            Convert.ToBase64String(salt),
            Convert.ToBase64String(key));
    }

    /// <inheritdoc />
    public bool Verify(string password, string hash)
    {
        ArgumentNullException.ThrowIfNull(password);
        ArgumentNullException.ThrowIfNull(hash);

        if (!TryParseHash(hash, out int iterations, out byte[] salt, out byte[] expectedKey))
            return false;

        byte[] actualKey = Rfc2898DeriveBytes.Pbkdf2(
            password: Encoding.UTF8.GetBytes(password),
            salt: salt,
            iterations: iterations,
            hashAlgorithm: HashAlgorithmName.SHA256,
            outputLength: expectedKey.Length);

        return CryptographicOperations.FixedTimeEquals(actualKey, expectedKey);
    }

    /// <summary>
    /// Parses the persisted hash format.
    /// </summary>
    private static bool TryParseHash(string hash, out int iterations, out byte[] salt, out byte[] key)
    {
        iterations = 0;
        salt = Array.Empty<byte>();
        key = Array.Empty<byte>();

        // Expected: PBKDF2$SHA256$iterations$saltBase64$hashBase64
        string[] parts = hash.Split('$', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length != 5)
            return false;

        if (!string.Equals(parts[0], Prefix, StringComparison.Ordinal))
            return false;

        if (!string.Equals(parts[1], Algorithm, StringComparison.Ordinal))
            return false;

        if (!int.TryParse(parts[2], out iterations) || iterations <= 0)
            return false;

        try
        {
            salt = Convert.FromBase64String(parts[3]);
            key = Convert.FromBase64String(parts[4]);
        }
        catch (FormatException)
        {
            return false;
        }

        return salt.Length > 0 && key.Length > 0;
    }
}
