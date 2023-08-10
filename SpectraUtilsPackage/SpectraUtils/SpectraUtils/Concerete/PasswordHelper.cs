using SpectraUtils.Abstract;
using System.Security.Cryptography;
using System.Text;

namespace SpectraUtils.Concrete
{
    public class PasswordHelper : IPasswordHelper
    {
        /// <summary>
        /// Gets a random standard password with a default length of 8 characters.
        /// </summary>
        public string CreateStardartPassword { get => CreatePassword(8); }

        /// <summary>
        /// Creates a password with the specified length and character requirements.
        /// </summary>
        /// <param name="passwordLength">The length of the password to be generated.</param>
        /// <param name="includeUppercaseLetter">Determines whether to include uppercase letters in the password (default: true).</param>
        /// <param name="includeLowercaseLetter">Determines whether to include lowercase letters in the password (default: true).</param>
        /// <param name="includeSpecialCharacter">Determines whether to include special characters in the password (default: true).</param>
        /// <param name="includeNumber">Determines whether to include numbers in the password (default: true).</param>
        /// <returns>The generated password that meets the specified requirements.</returns>
        public string CreatePassword(int passwordLength, bool includeUppercaseLetter = true, bool includeLowercaseLetter = true, bool includeSpecialCharacter = true, bool includeNumber = true)
        {
            if (passwordLength < 4)
                throw new ArgumentException("Password length must be greater than or equal to 4 characters.");

            if (!includeUppercaseLetter && !includeLowercaseLetter && !includeSpecialCharacter && !includeNumber)
                throw new ArgumentException("At least one character type (uppercase letter, lowercase letter, special character, or number) must be included in the password.");

            string passwordBuilder = "";
            Random rnd = new Random();
            int counter = 0;

            while (counter < passwordLength)
            {
                char c = (char)rnd.Next(1, 128);

                if ((includeUppercaseLetter && (c >= 'A' && c <= 'Z')) || (includeLowercaseLetter && (c >= 'a' && c <= 'z')) || (includeNumber && (c >= '0' && c <= '9')) || (includeSpecialCharacter && ((c >= 33 && c <= 47) || (c >= 58 && c <= 64) || (c >= 91 && c <= 96) || (c >= 123 && c <= 126))))
                {
                    passwordBuilder += c;
                    counter++;
                }

            }

            return passwordBuilder.ToString();
        }

        /// <summary>
        /// Hashes a password using the SHA-256 algorithm.
        /// </summary>
        /// <param name="password">The password to be hashed.</param>
        /// <returns>The hashed password as a hexadecimal string.</returns>
        public string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password);
                byte[] hash = sha256.ComputeHash(bytes);

                StringBuilder stringBuilder = new StringBuilder();
                foreach (byte b in hash)
                {
                    stringBuilder.Append(b.ToString("x2"));
                }

                return stringBuilder.ToString();
            }
        }
    }
}
