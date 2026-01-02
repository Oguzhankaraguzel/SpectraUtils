namespace SpectraUtils.Abstract;

/// <summary>
/// Provides methods for correcting, standardizing, and formatting names and surnames.
/// </summary>
public interface INameEdit
{
    /// <summary>
    /// Corrects the capitalization and removes spaces from a given name. (naME = Name).
    /// </summary>
    /// <param name="name">The name to be corrected.</param>
    /// <returns>The name with corrected capitalization.</returns>
    public string NameCorrection(string? name);

    /// <summary>
    /// Corrects the capitalization of multiple names and concatenates them into a single string.
    /// </summary>
    /// <param name="names">The names to be corrected.</param>
    /// <returns>The corrected names concatenated with spaces.</returns>
    public string NameCorrection(params string?[] names);

    /// <summary>
    /// Corrects the capitalization and removes spaces from a given sir name.
    /// </summary>
    /// <param name="sirName">The sir name to be corrected.</param>
    /// <returns>The corrected sir name without spaces and with uppercase letters.</returns>
    public string SirNameCorrection(string? sirName);

    /// <summary>
    /// Corrects the capitalization and removes spaces from one or more sir names.
    /// </summary>
    /// <param name="sirNames">The sir names to be corrected.</param>
    /// <returns>The corrected sir names without spaces and with uppercase letters.</returns>
    public string SirNameCorrection(params string?[] sirNames);

    /// <summary>
    /// Corrects the capitalization and spaces in the given name and sir name, and concatenates them to form a full name.
    /// </summary>
    /// <param name="name">The name to be corrected.</param>
    /// <param name="sirName">The sir name to be corrected.</param>
    /// <returns>The corrected full name with proper capitalization and spacing.</returns>
    public string FullNameCorrection(string? name, string? sirName);

    /// <summary>
    /// Corrects the capitalization and spacing of the given names and sir names, and concatenates them to form a full name.
    /// </summary>
    /// <param name="names">The names to be corrected.</param>
    /// <param name="sirNames">The sir names to be corrected.</param>
    /// <returns>The corrected full name with proper capitalization and spacing.</returns>
    public string FullNameCorrection(string?[] names, string?[] sirNames);

    /// <summary>
    /// Standardizes and normalizes characters in the input string by removing diacritic marks.
    /// </summary>
    /// <param name="input">The input string to be standardized.</param>
    /// <returns>The standardized string without diacritic marks.</returns>
    public string StandardizeCharacters(string? input);

    /// <summary>
    /// Creates a user name by standardizing the given name and appending a four-digit random number.
    /// </summary>
    /// <param name="name">The name to create the user name from.</param>
    /// <returns>The generated user name.</returns>
    public string CreateUserName(string? name);
}
