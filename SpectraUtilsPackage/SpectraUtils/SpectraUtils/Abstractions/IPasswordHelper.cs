namespace SpectraUtils.Abstractions;

/// <summary>
/// Provides methods for generating random passwords.
/// </summary>
public interface IPasswordHelper
{
    /// <summary>
    /// Creates a cryptographically secure random password with the specified length and character requirements.
    /// At least one character from every selected category is guaranteed.
    /// </summary>
    /// <param name="passwordLength">The length of the password to be generated (4 to 4096).</param>
    /// <param name="includeUppercaseLetter">Determines whether to include uppercase letters in the password (default: true).</param>
    /// <param name="includeLowercaseLetter">Determines whether to include lowercase letters in the password (default: true).</param>
    /// <param name="includeSpecialCharacter">Determines whether to include special characters in the password (default: true).</param>
    /// <param name="includeNumber">Determines whether to include numbers in the password (default: true).</param>
    /// <returns>The generated password that meets the specified requirements.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="passwordLength"/> is outside the supported range.</exception>
    /// <exception cref="ArgumentException">Thrown when no character category is selected.</exception>
    string CreatePassword(int passwordLength, bool includeUppercaseLetter = true, bool includeLowercaseLetter = true, bool includeSpecialCharacter = true, bool includeNumber = true);

    /// <summary>
    /// Creates a 12-character password containing uppercase letters, lowercase letters, digits and special characters.
    /// </summary>
    /// <returns>The generated password.</returns>
    string CreateStandardPassword();

    /// <summary>
    /// Hashes a password using unsalted SHA-256 and returns it as a lowercase hexadecimal string.
    /// </summary>
    /// <remarks>
    /// Unsalted fast hashes are not suitable for storing passwords. Use <see cref="IPasswordHasher"/> instead.
    /// This member is kept only to verify legacy hashes during a migration.
    /// </remarks>
    /// <param name="password">The password to be hashed.</param>
    /// <returns>The hashed password as a hexadecimal string.</returns>
    [Obsolete(ObsoleteMessages.HashPassword, DiagnosticId = ObsoleteMessages.HashPasswordId)]
    string HashPassword(string password);
}
