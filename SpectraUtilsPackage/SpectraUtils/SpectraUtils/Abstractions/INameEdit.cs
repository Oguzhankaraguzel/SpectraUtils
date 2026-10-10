using System.Diagnostics.CodeAnalysis;

namespace SpectraUtils.Abstractions;

/// <summary>
/// Provides methods for correcting, standardizing and formatting person names.
/// </summary>
public interface INameEdit
{
    /// <summary>
    /// Corrects the capitalization of a name: every word starts with an uppercase letter and the rest is lowercase.
    /// Leading/trailing whitespace is removed and repeated whitespace is collapsed into a single space
    /// (e.g. <c>"  aYşe   nur "</c> becomes <c>"Ayşe Nur"</c>). Letters after a hyphen are capitalized too.
    /// </summary>
    /// <param name="name">The name to be corrected.</param>
    /// <returns>The corrected name, or <see langword="null"/> when <paramref name="name"/> is <see langword="null"/>.</returns>
    [return: NotNullIfNotNull(nameof(name))]
    string? NameCorrection(string? name);

    /// <summary>
    /// Corrects every given name and joins the non-empty results with a single space.
    /// </summary>
    /// <param name="names">The names to be corrected. <see langword="null"/> and empty entries are skipped.</param>
    /// <returns>The corrected names separated by spaces, or an empty string.</returns>
    string NameCorrection(params string?[] names);

    /// <summary>
    /// Converts a surname to uppercase. Leading/trailing whitespace is removed and repeated whitespace
    /// is collapsed into a single space (e.g. <c>"kara  güzel"</c> becomes <c>"KARA GÜZEL"</c>).
    /// </summary>
    /// <param name="surname">The surname to be corrected.</param>
    /// <returns>The corrected surname, or <see langword="null"/> when <paramref name="surname"/> is <see langword="null"/>.</returns>
    [return: NotNullIfNotNull(nameof(surname))]
    string? SurnameCorrection(string? surname);

    /// <summary>
    /// Corrects every given surname and joins the non-empty results with a single space.
    /// </summary>
    /// <param name="surnames">The surnames to be corrected. <see langword="null"/> and empty entries are skipped.</param>
    /// <returns>The corrected surnames separated by spaces, or an empty string.</returns>
    string SurnameCorrection(params string?[] surnames);

    /// <summary>
    /// Corrects a name and a surname and combines them into a full name (e.g. <c>"Ayşe YILMAZ"</c>).
    /// </summary>
    /// <param name="name">The name to be corrected.</param>
    /// <param name="surname">The surname to be corrected.</param>
    /// <returns>The corrected full name, or an empty string when both parts are empty.</returns>
    string FullNameCorrection(string? name, string? surname);

    /// <summary>
    /// Corrects multiple names and surnames and combines them into a full name.
    /// </summary>
    /// <param name="names">The names to be corrected. May be <see langword="null"/>.</param>
    /// <param name="surnames">The surnames to be corrected. May be <see langword="null"/>.</param>
    /// <returns>The corrected full name, or an empty string when all parts are empty.</returns>
    string FullNameCorrection(string?[]? names, string?[]? surnames);

    /// <summary>
    /// Replaces accented and language-specific characters with their closest ASCII equivalents
    /// (e.g. <c>"Çağdaş Işık"</c> becomes <c>"Cagdas Isik"</c>, <c>"Crème"</c> becomes <c>"Creme"</c>).
    /// </summary>
    /// <param name="input">The text to be standardized.</param>
    /// <returns>The standardized text, or <see langword="null"/> when <paramref name="input"/> is <see langword="null"/>.</returns>
    [return: NotNullIfNotNull(nameof(input))]
    string? StandardizeCharacters(string? input);

    /// <summary>
    /// Creates a lowercase ASCII user name from the given name and appends a random four-digit suffix
    /// (e.g. <c>"Ayşe Nur"</c> becomes <c>"aysenur0427"</c>). The suffix does not guarantee uniqueness.
    /// </summary>
    /// <param name="name">The name to create the user name from.</param>
    /// <returns>The generated user name, or <see langword="null"/> when <paramref name="name"/> is <see langword="null"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> contains no letters or digits.</exception>
    [return: NotNullIfNotNull(nameof(name))]
    string? CreateUserName(string? name);
}
