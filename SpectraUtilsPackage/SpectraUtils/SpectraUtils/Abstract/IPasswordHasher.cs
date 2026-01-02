namespace SpectraUtils.Abstract;

/// <summary>
/// Provides password hashing and verification operations.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes the provided password.
    /// </summary>
    /// <param name="password">Plain text password.</param>
    /// <returns>A persisted hash string containing all parameters required for verification.</returns>
    string Hash(string password);

    /// <summary>
    /// Verifies the password against the persisted hash.
    /// </summary>
    /// <param name="password">Plain text password.</param>
    /// <param name="hash">Previously created hash string.</param>
    /// <returns><see langword="true"/> when the password matches the hash; otherwise <see langword="false"/>.</returns>
    bool Verify(string password, string hash);
}
