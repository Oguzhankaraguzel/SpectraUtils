using SpectraUtils.Abstractions;

namespace SpectraUtils;

/// <summary>
/// Convenience facade that groups all SpectraUtils services. Prefer injecting the individual
/// interfaces; use this facade when you need several services or do not use dependency injection.
/// </summary>
public interface ISpectraUtil
{
    /// <summary>
    /// Gets the name editing utility for correcting and formatting names.
    /// </summary>
    INameEdit NameEdit { get; }

    /// <summary>
    /// Gets the password helper utility for generating passwords.
    /// </summary>
    IPasswordHelper PasswordHelper { get; }

    /// <summary>
    /// Gets the password hasher for hashing/verifying persisted passwords (PBKDF2).
    /// </summary>
    IPasswordHasher PasswordHasher { get; }

    /// <summary>
    /// Gets the secure token generator for creating random Base64Url tokens.
    /// </summary>
    ISecureTokenGenerator TokenGenerator { get; }

    /// <summary>
    /// Gets the one-time code generator for numeric OTPs.
    /// </summary>
    IOneTimeCodeGenerator OneTimeCodeGenerator { get; }
}
