namespace SpectraUtils.Abstractions;

/// <summary>
/// Provides password hashing and verification operations.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes the provided password with a new random salt.
    /// </summary>
    /// <param name="password">Plain text password.</param>
    /// <returns>A persisted hash string containing all parameters required for verification.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="password"/> is <see langword="null"/>.</exception>
    string Hash(string password);

    /// <summary>
    /// Verifies the password against the persisted hash.
    /// </summary>
    /// <param name="password">Plain text password.</param>
    /// <param name="hash">Previously created hash string.</param>
    /// <returns>
    /// <see langword="true"/> when the password matches the hash; otherwise <see langword="false"/>.
    /// A malformed or unsupported hash also returns <see langword="false"/> instead of throwing.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when an argument is <see langword="null"/>.</exception>
    bool Verify(string password, string hash);

    /// <summary>
    /// Returns <see langword="true"/> when the hash was created with weaker parameters than the current
    /// configuration (or cannot be parsed) and should be replaced with a new hash after a successful login.
    /// </summary>
    /// <param name="hash">Previously created hash string.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="hash"/> is <see langword="null"/>.</exception>
    bool NeedsRehash(string hash);
}
