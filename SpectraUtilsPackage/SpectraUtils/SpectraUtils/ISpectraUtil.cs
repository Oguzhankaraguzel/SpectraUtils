using SpectraUtils.Abstract;

namespace SpectraUtils;

/// <summary>
/// Interface for the main utility class providing access to name editing, password management, and security services.
/// </summary>
public interface ISpectraUtil
{
    /// <summary>
    /// Gets the name editing utility for correcting and formatting names.
    /// </summary>
    INameEdit NameEdit { get; }

    /// <summary>
    /// Gets the password helper utility for creating and hashing passwords.
    /// </summary>
    IPasswordHelper PasswordHelper { get; }

    /// <summary>
    /// Gets the low-level password hasher for hashing/verifying persisted passwords (PBKDF2).
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
