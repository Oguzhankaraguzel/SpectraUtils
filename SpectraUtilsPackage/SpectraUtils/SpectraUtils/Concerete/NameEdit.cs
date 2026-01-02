using SpectraUtils.Abstract;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace SpectraUtils.Concerete;

/// <summary>
/// Provides methods for correcting, standardizing, and formatting names and surnames.
/// </summary>
public class NameEdit : INameEdit
{
    // Turkish culture info for proper Ý/I handling
    private static readonly CultureInfo TurkishCulture = new("tr-TR");

    /// <summary>
    /// Corrects the capitalization and removes spaces from a given name. (naME = Name).
    /// </summary>
    /// <param name="name">The name to be corrected.</param>
    /// <returns>The name with corrected capitalization.</returns>
    public string NameCorrection(string? name)
    {
        if (string.IsNullOrEmpty(name))
            return name!;

        var sb = new StringBuilder(name.Length);
        bool isFirstChar = true;

        foreach (char c in name)
        {
            if (c == ' ')
                continue;

            sb.Append(isFirstChar
                ? char.ToUpper(c, TurkishCulture)
                : char.ToLower(c, TurkishCulture));
            isFirstChar = false;
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
        if (names == null || names.Length == 0)
            return string.Empty;

        var correctedNames = new List<string>(names.Length);

        foreach (var name in names)
        {
            string corrected = NameCorrection(name);
            if (!string.IsNullOrEmpty(corrected))
                correctedNames.Add(corrected);
        }

        return string.Join(" ", correctedNames);
    }

    /// <summary>
    /// Corrects the capitalization and removes spaces from a given sir name.
    /// </summary>
    /// <param name="sirName">The sir name to be corrected.</param>
    /// <returns>The corrected sir name without spaces and with uppercase letters.</returns>
    public string SirNameCorrection(string? sirName)
    {
        if (string.IsNullOrEmpty(sirName))
            return sirName!;

        // Remove spaces and convert to uppercase using Turkish culture
        return sirName.Replace(" ", "", StringComparison.Ordinal)
                      .ToUpper(TurkishCulture);
    }

    /// <summary>
    /// Corrects the capitalization and removes spaces from one or more sir names.
    /// </summary>
    /// <param name="sirNames">The sir names to be corrected.</param>
    /// <returns>The corrected sir names without spaces and with uppercase letters.</returns>
    public string SirNameCorrection(params string?[] sirNames)
    {
        if (sirNames == null || sirNames.Length == 0)
            return string.Empty;

        var correctedNames = new List<string>(sirNames.Length);

        foreach (var sirName in sirNames)
        {
            string corrected = SirNameCorrection(sirName);
            if (!string.IsNullOrEmpty(corrected))
                correctedNames.Add(corrected);
        }

        return string.Join(" ", correctedNames);
    }

    /// <summary>
    /// Corrects the capitalization and spaces in the given name and sir name, and concatenates them to form a full name.
    /// </summary>
    /// <param name="name">The name to be corrected.</param>
    /// <param name="sirName">The sir name to be corrected.</param>
    /// <returns>The corrected full name with proper capitalization and spacing.</returns>
    public string FullNameCorrection(string? name, string? sirName)
    {
        string correctedName = NameCorrection(name);
        string correctedSirName = SirNameCorrection(sirName);

        if (string.IsNullOrEmpty(correctedName) && string.IsNullOrEmpty(correctedSirName))
            return string.Empty;

        if (string.IsNullOrEmpty(correctedName))
            return correctedSirName;

        if (string.IsNullOrEmpty(correctedSirName))
            return correctedName;

        return $"{correctedName} {correctedSirName}";
    }

    /// <summary>
    /// Corrects the capitalization and spacing of the given names and sir names, and concatenates them to form a full name.
    /// </summary>
    /// <param name="names">The names to be corrected.</param>
    /// <param name="sirNames">The sir names to be corrected.</param>
    /// <returns>The corrected full name with proper capitalization and spacing.</returns>
    public string FullNameCorrection(string?[] names, string?[] sirNames)
    {
        string correctedNames = NameCorrection(names);
        string correctedSirNames = SirNameCorrection(sirNames);

        if (string.IsNullOrEmpty(correctedNames) && string.IsNullOrEmpty(correctedSirNames))
            return string.Empty;

        if (string.IsNullOrEmpty(correctedNames))
            return correctedSirNames;

        if (string.IsNullOrEmpty(correctedSirNames))
            return correctedNames;

        return $"{correctedNames} {correctedSirNames}";
    }

    /// <summary>
    /// Standardizes and normalizes characters in the input string by removing diacritic marks.
    /// </summary>
    /// <param name="input">The input string to be standardized.</param>
    /// <returns>The standardized string without diacritic marks.</returns>
    public string StandardizeCharacters(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return input!;

        // Handle Turkish special characters explicitly
        var turkishMapping = new Dictionary<char, char>
        {
            ['ý'] = 'i', ['Ý'] = 'I',
            ['ð'] = 'g', ['Ð'] = 'G',
            ['ü'] = 'u', ['Ü'] = 'U',
            ['þ'] = 's', ['Þ'] = 'S',
            ['ö'] = 'o', ['Ö'] = 'O',
            ['ç'] = 'c', ['Ç'] = 'C'
        };

        var sb = new StringBuilder(input.Length);

        foreach (char c in input)
        {
            if (turkishMapping.TryGetValue(c, out char replacement))
            {
                sb.Append(replacement);
            }
            else
            {
                sb.Append(c);
            }
        }

        // Normalize remaining diacritics
        string normalized = sb.ToString().Normalize(NormalizationForm.FormD);
        sb.Clear();

        foreach (char c in normalized)
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
        if (string.IsNullOrEmpty(name))
            return name!;

        string standardized = StandardizeCharacters(name);
        int randomSuffix = RandomNumberGenerator.GetInt32(0, 10000);

        return $"{standardized}{randomSuffix:D4}";
    }
}
