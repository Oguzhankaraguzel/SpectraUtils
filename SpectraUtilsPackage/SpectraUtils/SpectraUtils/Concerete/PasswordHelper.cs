using SpectraUtils.Abstract;
using System.Security.Cryptography;
using System.Text;

namespace SpectraUtils.Concrete;

/// <summary>
/// Provides methods for creating and hashing passwords with various requirements.
/// </summary>
public class PasswordHelper : IPasswordHelper
{
    private const string UppercaseLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string LowercaseLetters = "abcdefghijklmnopqrstuvwxyz";
    private const string Numbers = "0123456789";
    private const string SpecialCharacters = "!@#$%^&*()-_=+[]{}|;:',.<>?/`~";
    private const int MinimumPasswordLength = 4;

    /// <summary>
    /// Gets a random standard password with a default length of 8 characters.
    /// </summary>
    public string CreateStardartPassword => CreatePassword(8);

    /// <summary>
    /// Creates a password with the specified length and character requirements.
    /// </summary>
    /// <param name="passwordLength">The length of the password to be generated.</param>
    /// <param name="includeUppercaseLetter">Determines whether to include uppercase letters in the password (default: true).</param>
    /// <param name="includeLowercaseLetter">Determines whether to include lowercase letters in the password (default: true).</param>
    /// <param name="includeSpecialCharacter">Determines whether to include special characters in the password (default: true).</param>
    /// <param name="includeNumber">Determines whether to include numbers in the password (default: true).</param>
    /// <returns>The generated password that meets the specified requirements.</returns>
    /// <exception cref="ArgumentException">Thrown when password length is less than 4 or no character types are selected.</exception>
    public string CreatePassword(
        int passwordLength,
        bool includeUppercaseLetter = true,
        bool includeLowercaseLetter = true,
        bool includeSpecialCharacter = true,
        bool includeNumber = true)
    {
        if (passwordLength < MinimumPasswordLength)
            throw new ArgumentException($"Password length must be greater than or equal to {MinimumPasswordLength} characters.", nameof(passwordLength));

        if (!includeUppercaseLetter && !includeLowercaseLetter && !includeSpecialCharacter && !includeNumber)
            throw new ArgumentException("At least one character type (uppercase letter, lowercase letter, special character, or number) must be included in the password.");

        // Build the character pool based on requirements
        var charPoolBuilder = new StringBuilder();
        var requiredChars = new List<char>();

        if (includeUppercaseLetter)
        {
            charPoolBuilder.Append(UppercaseLetters);
            requiredChars.Add(GetSecureRandomChar(UppercaseLetters));
        }

        if (includeLowercaseLetter)
        {
            charPoolBuilder.Append(LowercaseLetters);
            requiredChars.Add(GetSecureRandomChar(LowercaseLetters));
        }

        if (includeNumber)
        {
            charPoolBuilder.Append(Numbers);
            requiredChars.Add(GetSecureRandomChar(Numbers));
        }

        if (includeSpecialCharacter)
        {
            charPoolBuilder.Append(SpecialCharacters);
            requiredChars.Add(GetSecureRandomChar(SpecialCharacters));
        }

        string charPool = charPoolBuilder.ToString();
        char[] password = new char[passwordLength];

        // Fill remaining positions with random characters from the pool
        int remainingLength = passwordLength - requiredChars.Count;
        for (int i = 0; i < remainingLength; i++)
        {
            password[i] = GetSecureRandomChar(charPool);
        }

        // Add required characters
        for (int i = 0; i < requiredChars.Count; i++)
        {
            password[remainingLength + i] = requiredChars[i];
        }

        // Shuffle the password using Fisher-Yates algorithm
        ShuffleArray(password);

        return new string(password);
    }

    /// <summary>
    /// Hashes a password using the SHA-256 algorithm.
    /// </summary>
    /// <param name="password">The password to be hashed.</param>
    /// <returns>The hashed password as a hexadecimal string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when password is null.</exception>
    public string HashPassword(string password)
    {
        ArgumentNullException.ThrowIfNull(password);

        byte[] bytes = Encoding.UTF8.GetBytes(password);
        byte[] hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    /// <summary>
    /// Gets a cryptographically secure random character from the specified character set.
    /// </summary>
    private static char GetSecureRandomChar(string charSet)
    {
        return charSet[RandomNumberGenerator.GetInt32(charSet.Length)];
    }

    /// <summary>
    /// Shuffles an array using the Fisher-Yates algorithm with cryptographically secure random numbers.
    /// </summary>
    private static void ShuffleArray(char[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = RandomNumberGenerator.GetInt32(i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }
    }
}
