using SpectraUtils.Abstractions;
using System.Security.Cryptography;
using System.Text;

namespace SpectraUtils.Services;

/// <summary>
/// Generates cryptographically secure random passwords.
/// </summary>
public sealed class PasswordHelper : IPasswordHelper
{
    /// <summary>Minimum supported password length.</summary>
    public const int MinimumPasswordLength = 4;

    /// <summary>Maximum supported password length.</summary>
    public const int MaximumPasswordLength = 4096;

    /// <summary>Length used by <see cref="CreateStandardPassword"/>.</summary>
    public const int StandardPasswordLength = 12;

    private const string UppercaseLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string LowercaseLetters = "abcdefghijklmnopqrstuvwxyz";
    private const string Numbers = "0123456789";
    private const string SpecialCharacters = "!@#$%^&*()-_=+[]{}|;:',.<>?/`~";

    /// <inheritdoc />
    public string CreateStandardPassword() => CreatePassword(StandardPasswordLength);

    /// <inheritdoc />
    public string CreatePassword(
        int passwordLength,
        bool includeUppercaseLetter = true,
        bool includeLowercaseLetter = true,
        bool includeSpecialCharacter = true,
        bool includeNumber = true)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(passwordLength, MinimumPasswordLength);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(passwordLength, MaximumPasswordLength);

        if (!includeUppercaseLetter && !includeLowercaseLetter && !includeSpecialCharacter && !includeNumber)
            throw new ArgumentException("At least one character type (uppercase letter, lowercase letter, special character, or number) must be included in the password.");

        var pool = new StringBuilder();
        char[] password = new char[passwordLength];
        int requiredCount = 0;

        // One guaranteed character from each selected category.
        if (includeUppercaseLetter)
            AddCategory(UppercaseLetters);
        if (includeLowercaseLetter)
            AddCategory(LowercaseLetters);
        if (includeNumber)
            AddCategory(Numbers);
        if (includeSpecialCharacter)
            AddCategory(SpecialCharacters);

        // Fill the rest from the combined pool, then shuffle so the required characters are not at fixed positions.
        RandomNumberGenerator.GetItems<char>(pool.ToString(), password.AsSpan(requiredCount));
        RandomNumberGenerator.Shuffle(password.AsSpan());

        return new string(password);

        void AddCategory(string characters)
        {
            pool.Append(characters);
            password[requiredCount++] = characters[RandomNumberGenerator.GetInt32(characters.Length)];
        }
    }

    /// <inheritdoc />
    [Obsolete(ObsoleteMessages.HashPassword, DiagnosticId = ObsoleteMessages.HashPasswordId)]
    public string HashPassword(string password)
    {
        ArgumentNullException.ThrowIfNull(password);

        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
