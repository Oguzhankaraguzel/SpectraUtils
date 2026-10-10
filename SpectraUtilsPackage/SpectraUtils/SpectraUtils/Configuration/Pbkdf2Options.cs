namespace SpectraUtils.Configuration;

/// <summary>
/// Hash algorithms supported by <see cref="Services.Pbkdf2PasswordHasher"/>.
/// </summary>
public enum Pbkdf2HashAlgorithm
{
    /// <summary>HMAC-SHA256 (OWASP recommends at least 600,000 iterations).</summary>
    Sha256,

    /// <summary>HMAC-SHA384.</summary>
    Sha384,

    /// <summary>HMAC-SHA512 (OWASP recommends at least 210,000 iterations).</summary>
    Sha512,
}

/// <summary>
/// Settings for <see cref="Services.Pbkdf2PasswordHasher"/>.
/// </summary>
/// <remarks>
/// Changing these values does not break existing hashes: every hash stores its own parameters.
/// Use <see cref="Abstractions.IPasswordHasher.NeedsRehash"/> to upgrade old hashes after a successful login.
/// </remarks>
public sealed class Pbkdf2Options
{
    /// <summary>Default iteration count (OWASP 2023 recommendation for PBKDF2-HMAC-SHA256).</summary>
    public const int DefaultIterations = 600_000;

    /// <summary>Minimum accepted salt size in bytes.</summary>
    public const int MinSaltSize = 8;

    /// <summary>Minimum accepted derived key size in bytes.</summary>
    public const int MinKeySize = 16;

    /// <summary>Maximum accepted salt or derived key size in bytes.</summary>
    public const int MaxSize = 1024;

    /// <summary>Number of PBKDF2 iterations used for new hashes. Default is 600,000.</summary>
    public int Iterations { get; set; } = DefaultIterations;

    /// <summary>Salt size in bytes (8 to 1024). Default is 16.</summary>
    public int SaltSize { get; set; } = 16;

    /// <summary>Derived key size in bytes (16 to 1024). Default is 32 (256 bits).</summary>
    public int KeySize { get; set; } = 32;

    /// <summary>HMAC algorithm used for new hashes. Default is <see cref="Pbkdf2HashAlgorithm.Sha256"/>.</summary>
    public Pbkdf2HashAlgorithm Algorithm { get; set; } = Pbkdf2HashAlgorithm.Sha256;

    /// <summary>
    /// Highest iteration count accepted when verifying a stored hash. Protects against denial of service
    /// through tampered hashes with huge iteration counts. Default is 10,000,000.
    /// </summary>
    public int MaxIterations { get; set; } = 10_000_000;
}
