namespace SpectraUtils.Abstract
{
    public interface IPasswordHelper
    {
        /// <summary>
        /// Creates a password with the specified length and character requirements.
        /// </summary>
        /// <param name="passwordLength">The length of the password to be generated.</param>
        /// <param name="includeUppercaseLetter">Determines whether to include uppercase letters in the password (default: true).</param>
        /// <param name="includeLowercaseLetter">Determines whether to include lowercase letters in the password (default: true).</param>
        /// <param name="includeSpecialCharacter">Determines whether to include special characters in the password (default: true).</param>
        /// <param name="includeNumber">Determines whether to include numbers in the password (default: true).</param>
        /// <returns>The generated password that meets the specified requirements.</returns>
        public string CreatePassword(int passwordLength, bool includeUppercaseLetter = true, bool includeLowercaseLetter = true, bool includeSpecialCharacter = true, bool includeNumber = true);

        /// <summary>
        /// Hashes a password using the SHA-256 algorithm.
        /// </summary>
        /// <param name="password">The password to be hashed.</param>
        /// <returns>The hashed password as a hexadecimal string.</returns>
        public string HashPassword(string password);

        /// <summary>
        /// Gets a random standard password with a default length of 8 characters.
        /// </summary>
        public string CreateStardartPassword { get; }
    }
}
