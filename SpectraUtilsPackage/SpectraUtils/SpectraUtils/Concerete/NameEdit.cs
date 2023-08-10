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

            name = name.Replace(" ", "");
            string correctedName = char.ToUpper(name[0]) + name.Substring(1).ToLower();

            return correctedName;
        }

        /// <summary>
        /// Corrects the capitalization of multiple names and concatenates them into a single string.
        /// </summary>
        /// <param name="names">The names to be corrected.</param>
        /// <returns>The corrected names concatenated with spaces.</returns>
        public string NameCorrection(params string?[] names)
        {
            if (names == null || names.Length == 0) return string.Empty;

            for (int i = 0; i < names.Length; i++)
            {
                if (!string.IsNullOrEmpty(names[i]))
                {
                    names[i] = NameCorrection(names[i]);
                }
            }

            return string.Join(" ", names);
        }

        /// <summary>
        /// Corrects the capitalization and removes spaces from a given sir name.
        /// </summary>
        /// <param name="sirName">The sir name to be corrected.</param>
        /// <returns>The corrected sir name without spaces and with uppercase letters.</returns>
        public string SirNameCorrection(string? sirName)
        {
            if (string.IsNullOrEmpty(sirName)) return sirName!;
            return sirName.Replace(" ", "").ToUpper();
        }

        /// <summary>
        /// Corrects the capitalization and removes spaces from one or more sir names.
        /// </summary>
        /// <param name="sirNames">The sir names to be corrected.</param>
        /// <returns>The corrected sir names without spaces and with uppercase letters.</returns>
        public string SirNameCorrection(params string?[] sirNames)
        {
            if (sirNames == null || sirNames.Length == 0) return string.Empty;

            for (int i = 0; i < sirNames.Length; i++)
            {
                if (!string.IsNullOrEmpty(sirNames[i]))
                {
                    sirNames[i] = SirNameCorrection(sirNames[i]);
                }
            }

            return string.Join(" ", sirNames);
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

            return NameCorrection(name) + " " + SirNameCorrection(sirName);

        }

        /// <summary>
        /// Corrects the capitalization and spacing of the given names and sir names, and concatenates them to form a full name.
        /// </summary>
        /// <param name="names">The names to be corrected.</param>
        /// <param name="sirNames">The sir names to be corrected.</param>
        /// <returns>The corrected full name with proper capitalization and spacing.</returns>
        public string FullNameCorrection(string?[] names, string?[] sirNames)
        {
            if (names == null && sirNames == null) return names + " " + sirNames;
            string correctedFullNames = NameCorrection(names!) + " " + SirNameCorrection(sirNames!);
            return correctedFullNames;
        }

        /// <summary>
        /// Standardizes and normalizes characters in the input string by removing diacritic marks.
        /// </summary>
        /// <param name="input">The input string to be standardized.</param>
        /// <returns>The standardized string without diacritic marks.</returns>
        public string StandardizeCharacters(string? input)
        {
            if (string.IsNullOrEmpty(input)) return input!;

            string? normalizedString = input!.Normalize(NormalizationForm.FormD);
            string result = new string(normalizedString.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray());

            return result;
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

            return StandardizeCharacters(name) + rnd.Next(0, 10000).ToString("D4");
        }

    }
}