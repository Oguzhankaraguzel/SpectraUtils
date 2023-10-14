using SpectraUtils.Abstract;
using System.Globalization;
using System.Text;

namespace SpectraUtils.Concerete
{
    public class NameEdit : INameEdit
    {

        /// <summary>
        /// Corrects the capitalization and removes spaces from a given name. (naME = Name).
        /// </summary>
        /// <param name="name">The name to be corrected.</param>
        /// <returns>The name with corrected capitalization.</returns>
        public string NameCorrection(string? name)
        {
            if (string.IsNullOrEmpty(name)) return name!;

            StringBuilder sb = new StringBuilder();
            bool isFirstChar = true;

            foreach (char c in name)
            {
                if (c != ' ')
                {
                    if (isFirstChar)
                    {
                        sb.Append(char.ToUpper(c));
                        isFirstChar = false;
                    }
                    else
                    {
                        sb.Append(char.ToLower(c));
                    }
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Corrects the capitalization of multiple names and concatenates them into a single string.
        /// </summary>
        /// <param name="names">The names to be corrected.</param>
        /// <returns>The corrected names concatenated with spaces.</returns>
        public string NameCorrection(params string?[] names)
        {
            if (names == null || names.Length == 0) return string.Empty;

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < names.Length; i++)
            {
                if (!string.IsNullOrEmpty(names[i]))
                {
                    if (i > 0) sb.Append(" ");
                    sb.Append(NameCorrection(names[i]));
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Corrects the capitalization and removes spaces from a given sir name.
        /// </summary>
        /// <param name="sirName">The sir name to be corrected.</param>
        /// <returns>The corrected sir name without spaces and with uppercase letters.</returns>
        public string SirNameCorrection(string? sirName)
        {
            if (string.IsNullOrEmpty(sirName)) return sirName!;

            StringBuilder sb = new StringBuilder(sirName);
            sb.Replace(" ", "");

            return sb.ToString().ToUpper();
        }


        /// <summary>
        /// Corrects the capitalization and removes spaces from one or more sir names.
        /// </summary>
        /// <param name="sirNames">The sir names to be corrected.</param>
        /// <returns>The corrected sir names without spaces and with uppercase letters.</returns>
        public string SirNameCorrection(params string?[] sirNames)
        {
            if (sirNames == null || sirNames.Length == 0) return string.Empty;

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < sirNames.Length; i++)
            {
                if (!string.IsNullOrEmpty(sirNames[i]))
                {
                    if (i > 0) sb.Append(" ");
                    sb.Append(SirNameCorrection(sirNames[i]));
                }
            }

            return sb.ToString();
        }



        /// <summary>
        /// Corrects the capitalization and spaces in the given name and sir name, and concatenates them to form a full name.
        /// </summary>
        /// <param name="name">The name to be corrected.</param>
        /// <param name="sirName">The sir name to be corrected.</param>
        /// <returns>The corrected full name with proper capitalization and spacing.</returns>
        public string FullNameCorrection(string? name, string? sirName)
        {
            if (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(sirName)) return name + sirName;

            StringBuilder sb = new StringBuilder();

            sb.Append(NameCorrection(name));

            if (sb.Length > 0) sb.Append(" ");

            sb.Append(SirNameCorrection(sirName));

            return sb.ToString();
        }


        /// <summary>
        /// Corrects the capitalization and spacing of the given names and sir names, and concatenates them to form a full name.
        /// </summary>
        /// <param name="names">The names to be corrected.</param>
        /// <param name="sirNames">The sir names to be corrected.</param>
        /// <returns>The corrected full name with proper capitalization and spacing.</returns>
        public string FullNameCorrection(string?[] names, string?[] sirNames)
        {
            if ((names == null || names.Length == 0) && (sirNames == null || sirNames.Length == 0))
                return string.Empty;

            StringBuilder sb = new StringBuilder();

            if (names != null)
            {
                foreach (var name in names)
                {
                    if (!string.IsNullOrEmpty(name))
                    {
                        if (sb.Length > 0)
                        {
                            sb.Append(" ");
                        }
                        sb.Append(NameCorrection(name));
                    }
                }
            }

            if (sirNames != null)
            {
                foreach (var sirName in sirNames)
                {
                    if (!string.IsNullOrEmpty(sirName))
                    {
                        if (sb.Length > 0)
                        {
                            sb.Append(" ");
                        }
                        sb.Append(SirNameCorrection(sirName));
                    }
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Standardizes and normalizes characters in the input string by removing diacritic marks.
        /// </summary>
        /// <param name="input">The input string to be standardized.</param>
        /// <returns>The standardized string without diacritic marks.</returns>
        public string StandardizeCharacters(string? input)
        {
            if (string.IsNullOrEmpty(input)) return input!;

            StringBuilder sb = new StringBuilder();

            string normalizedString = input.Normalize(NormalizationForm.FormD);

            foreach (char c in normalizedString)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Creates a user name by standardizing the given name and appending a four-digit random number.
        /// </summary>
        /// <param name="name">The name to create the user name from.</param>
        /// <returns>The generated user name.</returns>
        public string CreateUserName(string? name)
        {
            if (string.IsNullOrEmpty(name)) return name!;

            Random rnd = new Random();
            string standardized = StandardizeCharacters(name);
            string randomSuffix = rnd.Next(0, 10000).ToString("D4");

            return standardized + randomSuffix;
        }

    }
}