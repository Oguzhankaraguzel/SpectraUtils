using SpectraUtils.Abstractions;
using SpectraUtils.Configuration;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace SpectraUtils.Services;

/// <summary>
/// Password hasher based on PBKDF2 (RFC 8018).
/// <para>Format: <c>PBKDF2$&lt;SHA256|SHA384|SHA512&gt;$iterations$saltBase64$hashBase64</c></para>
/// </summary>
/// <remarks>
/// Hashes created by SpectraUtils 2.x (<c>PBKDF2$SHA256$210000$...</c>) are still verified;
/// <see cref="NeedsRehash"/> reports them as outdated so they can be upgraded transparently.
/// </remarks>
public sealed class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const string Prefix = "PBKDF2";
    private const char Separator = '$';

    // Longest legitimate hash is far below this; anything larger is rejected before parsing.
    private const int MaxHashLength = 4096;

    private readonly int _iterations;
    private readonly int _saltSize;
    private readonly int _keySize;
    private readonly int _maxIterations;
    private readonly HashAlgorithmName _algorithm;
    private readonly string _algorithmName;

    /// <summary>
    /// Initializes a new instance with the default settings (PBKDF2-HMAC-SHA256, 600,000 iterations,
    /// 16-byte salt, 32-byte key).
    /// </summary>
    public Pbkdf2PasswordHasher()
        : this(new Pbkdf2Options())
    {
    }

    /// <summary>
    /// Initializes a new instance with the given settings.
    /// </summary>
    /// <param name="options">PBKDF2 settings. Values are copied, later changes have no effect.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when a setting is outside its supported range.</exception>
    public Pbkdf2PasswordHasher(Pbkdf2Options options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (options.Iterations <= 0)
            throw new ArgumentOutOfRangeException(nameof(options), options.Iterations, "Iterations must be greater than zero.");
        if (options.MaxIterations < options.Iterations)
            throw new ArgumentOutOfRangeException(nameof(options), options.MaxIterations, "MaxIterations must be greater than or equal to Iterations.");
        if (options.SaltSize is < Pbkdf2Options.MinSaltSize or > Pbkdf2Options.MaxSize)
            throw new ArgumentOutOfRangeException(nameof(options), options.SaltSize, $"SaltSize must be between {Pbkdf2Options.MinSaltSize} and {Pbkdf2Options.MaxSize} bytes.");
        if (options.KeySize is < Pbkdf2Options.MinKeySize or > Pbkdf2Options.MaxSize)
            throw new ArgumentOutOfRangeException(nameof(options), options.KeySize, $"KeySize must be between {Pbkdf2Options.MinKeySize} and {Pbkdf2Options.MaxSize} bytes.");

        (_algorithm, _algorithmName) = options.Algorithm switch
        {
            Pbkdf2HashAlgorithm.Sha256 => (HashAlgorithmName.SHA256, "SHA256"),
            Pbkdf2HashAlgorithm.Sha384 => (HashAlgorithmName.SHA384, "SHA384"),
            Pbkdf2HashAlgorithm.Sha512 => (HashAlgorithmName.SHA512, "SHA512"),
            _ => throw new ArgumentOutOfRangeException(nameof(options), options.Algorithm, "Unsupported hash algorithm."),
        };

        _iterations = options.Iterations;
        _saltSize = options.SaltSize;
        _keySize = options.KeySize;
        _maxIterations = options.MaxIterations;
    }

    /// <inheritdoc />
    public string Hash(string password)
    {
        ArgumentNullException.ThrowIfNull(password);

        byte[] salt = RandomNumberGenerator.GetBytes(_saltSize);
        byte[] key = DeriveKey(password, salt, _iterations, _algorithm, _keySize);

        return string.Join(Separator,
            Prefix,
            _algorithmName,
            _iterations.ToString(CultureInfo.InvariantCulture),
            Convert.ToBase64String(salt),
            Convert.ToBase64String(key));
    }

    /// <inheritdoc />
    public bool Verify(string password, string hash)
    {
        ArgumentNullException.ThrowIfNull(password);
        ArgumentNullException.ThrowIfNull(hash);

        if (!TryParseHash(hash, out ParsedHash parsed))
            return false;

        byte[] actualKey = DeriveKey(password, parsed.Salt, parsed.Iterations, parsed.Algorithm, parsed.Key.Length);
        return CryptographicOperations.FixedTimeEquals(actualKey, parsed.Key);
    }

    /// <inheritdoc />
    public bool NeedsRehash(string hash)
    {
        ArgumentNullException.ThrowIfNull(hash);

        if (!TryParseHash(hash, out ParsedHash parsed))
            return true;

        return parsed.AlgorithmName != _algorithmName
            || parsed.Iterations < _iterations
            || parsed.Salt.Length < _saltSize
            || parsed.Key.Length < _keySize;
    }

    private static byte[] DeriveKey(string password, byte[] salt, int iterations, HashAlgorithmName algorithm, int length)
    {
        // Encoding.UTF8 replaces invalid surrogates instead of throwing, which keeps 2.x hashes verifiable.
        byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
        try
        {
            return Rfc2898DeriveBytes.Pbkdf2(passwordBytes, salt, iterations, algorithm, length);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(passwordBytes);
        }
    }

    private bool TryParseHash(string hash, out ParsedHash parsed)
    {
        parsed = default;

        if (hash.Length > MaxHashLength)
            return false;

        string[] parts = hash.Trim().Split(Separator);
        if (parts.Length != 5 || !string.Equals(parts[0], Prefix, StringComparison.Ordinal))
            return false;

        HashAlgorithmName algorithm;
        switch (parts[1])
        {
            case "SHA256": algorithm = HashAlgorithmName.SHA256; break;
            case "SHA384": algorithm = HashAlgorithmName.SHA384; break;
            case "SHA512": algorithm = HashAlgorithmName.SHA512; break;
            default: return false;
        }

        if (!int.TryParse(parts[2], NumberStyles.None, CultureInfo.InvariantCulture, out int iterations)
            || iterations <= 0
            || iterations > _maxIterations)
            return false;

        byte[] salt;
        byte[] key;
        try
        {
            salt = Convert.FromBase64String(parts[3]);
            key = Convert.FromBase64String(parts[4]);
        }
        catch (FormatException)
        {
            return false;
        }

        if (salt.Length == 0 || key.Length == 0 || salt.Length > Pbkdf2Options.MaxSize || key.Length > Pbkdf2Options.MaxSize)
            return false;

        parsed = new ParsedHash(parts[1], algorithm, iterations, salt, key);
        return true;
    }

    private readonly record struct ParsedHash(string AlgorithmName, HashAlgorithmName Algorithm, int Iterations, byte[] Salt, byte[] Key);
}
