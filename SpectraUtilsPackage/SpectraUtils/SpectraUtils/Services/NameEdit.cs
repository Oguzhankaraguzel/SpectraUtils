using SpectraUtils.Abstractions;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace SpectraUtils.Services;

/// <summary>
/// Provides methods for correcting, standardizing and formatting person names.
/// </summary>
/// <remarks>
/// Turkish casing (<c>i ↔ İ</c>, <c>ı ↔ I</c>) is implemented explicitly, so it also works when the
/// application runs in globalization-invariant mode (e.g. small Docker images).
/// </remarks>
public sealed class NameEdit : INameEdit
{
    // Latin-1 Supplement / Latin Extended-A/B letters and their unaccented ASCII base letter
    // (generated from the Unicode NFD decomposition). LatinFrom[i] maps to LatinTo[i].
    private const char LatinTableStart = 'À';
    private const char LatinTableEnd = 'ɏ';

    private const string LatinFrom =
        "ÀÁÂÃÄÅÇÈÉÊËÌÍÎÏÑÒÓÔÕÖÙÚÛÜÝàáâãäåçèéêëìíî" +
        "ïñòóôõöùúûüýÿĀāĂăĄąĆćĈĉĊċČčĎďĒēĔĕĖėĘęĚěĜ" +
        "ĝĞğĠġĢģĤĥĨĩĪīĬĭĮįİĴĵĶķĹĺĻļĽľŃńŅņŇňŌōŎŏŐő" +
        "ŔŕŖŗŘřŚśŜŝŞşŠšŢţŤťŨũŪūŬŭŮůŰűŲųŴŵŶŷŸŹźŻżŽ" +
        "žƠơƯưǍǎǏǐǑǒǓǔǕǖǗǘǙǚǛǜǞǟǠǡǦǧǨǩǪǫǬǭǰǴǵǸǹǺǻ" +
        "ȀȁȂȃȄȅȆȇȈȉȊȋȌȍȎȏȐȑȒȓȔȕȖȗȘșȚțȞȟȦȧȨȩȪȫȬȭȮȯ" +
        "ȰȱȲȳ";

    private const string LatinTo =
        "AAAAAACEEEEIIIINOOOOOUUUUYaaaaaaceeeeiii" +
        "inooooouuuuyyAaAaAaCcCcCcCcDdEeEeEeEeEeG" +
        "gGgGgGgHhIiIiIiIiIJjKkLlLlLlNnNnNnOoOoOo" +
        "RrRrRrSsSsSsSsTtTtUuUuUuUuUuUuWwYyYZzZzZ" +
        "zOoUuAaIiOoUuUuUuUuUuAaAaGgKkOoOojGgNnAa" +
        "AaAaEeEeIiIiOoOoRrRrUuUuSsTtHhAaEeOoOoOo" +
        "OoYy";

    private readonly CultureInfo _culture;
    private readonly bool _turkicCasing;

    /// <summary>
    /// Initializes a new instance that uses Turkish casing rules.
    /// </summary>
    public NameEdit()
    {
        _culture = TryGetCulture("tr-TR") ?? CultureInfo.InvariantCulture;
        _turkicCasing = true;
    }

    /// <summary>
    /// Initializes a new instance that uses the casing rules of the given culture.
    /// </summary>
    /// <param name="culture">The culture used for upper/lower case conversion.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="culture"/> is <see langword="null"/>.</exception>
    public NameEdit(CultureInfo culture)
    {
        ArgumentNullException.ThrowIfNull(culture);

        _culture = culture;
        _turkicCasing = culture.TwoLetterISOLanguageName is "tr" or "az";
    }

    /// <inheritdoc />
    [return: NotNullIfNotNull(nameof(name))]
    public string? NameCorrection(string? name)
    {
        if (name is null)
            return null;

        var sb = new StringBuilder(name.Length);
        bool startOfWord = true;
        bool pendingSpace = false;

        foreach (char c in name)
        {
            if (char.IsWhiteSpace(c))
            {
                pendingSpace = sb.Length > 0;
                startOfWord = true;
                continue;
            }

            if (pendingSpace)
            {
                sb.Append(' ');
                pendingSpace = false;
            }

            sb.Append(startOfWord ? ToUpper(c) : ToLower(c));
            startOfWord = c == '-';
        }

        return sb.ToString();
    }

    /// <inheritdoc />
    public string NameCorrection(params string?[] names) => JoinCorrected(names, NameCorrection);

    /// <inheritdoc />
    [return: NotNullIfNotNull(nameof(surname))]
    public string? SurnameCorrection(string? surname)
    {
        if (surname is null)
            return null;

        var sb = new StringBuilder(surname.Length);
        bool pendingSpace = false;

        foreach (char c in surname)
        {
            if (char.IsWhiteSpace(c))
            {
                pendingSpace = sb.Length > 0;
                continue;
            }

            if (pendingSpace)
            {
                sb.Append(' ');
                pendingSpace = false;
            }

            sb.Append(ToUpper(c));
        }

        return sb.ToString();
    }

    /// <inheritdoc />
    public string SurnameCorrection(params string?[] surnames) => JoinCorrected(surnames, SurnameCorrection);

    /// <inheritdoc />
    public string FullNameCorrection(string? name, string? surname)
        => JoinNonEmpty(NameCorrection(name), SurnameCorrection(surname));

    /// <inheritdoc />
    public string FullNameCorrection(string?[]? names, string?[]? surnames)
        => JoinNonEmpty(NameCorrection(names ?? []), SurnameCorrection(surnames ?? []));

    /// <inheritdoc />
    [return: NotNullIfNotNull(nameof(input))]
    public string? StandardizeCharacters(string? input)
    {
        if (string.IsNullOrEmpty(input) || Ascii.IsValid(input))
            return input;

        // Pass 1: table lookup for Latin letters. Does not depend on ICU, so it also works in invariant mode.
        var sb = new StringBuilder(input.Length);

        foreach (char c in input)
        {
            int index = c is >= LatinTableStart and <= LatinTableEnd ? LatinFrom.IndexOf(c) : -1;
            if (index >= 0)
            {
                sb.Append(LatinTo[index]);
                continue;
            }

            string? replacement = GetSpecialReplacement(c);
            if (replacement is null)
                sb.Append(c);
            else
                sb.Append(replacement);
        }

        string result = sb.ToString();
        if (Ascii.IsValid(result))
            return result;

        // Pass 2: remaining characters (e.g. Vietnamese, Greek, or combining marks typed separately).
        string decomposed = result.Normalize(NormalizationForm.FormD);
        sb.Clear();

        foreach (char c in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }

        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    /// <inheritdoc />
    [return: NotNullIfNotNull(nameof(name))]
    public string? CreateUserName(string? name)
    {
        if (name is null)
            return null;

        string standardized = StandardizeCharacters(name);
        var sb = new StringBuilder(standardized.Length + 4);

        foreach (char c in standardized)
        {
            if (char.IsAsciiLetterOrDigit(c))
                sb.Append(char.ToLowerInvariant(c));
        }

        if (sb.Length == 0)
            throw new ArgumentException("The name must contain at least one letter or digit.", nameof(name));

        int suffix = RandomNumberGenerator.GetInt32(0, 10_000);
        sb.Append(suffix.ToString("D4", CultureInfo.InvariantCulture));

        return sb.ToString();
    }

    private char ToUpper(char c)
    {
        if (_turkicCasing)
        {
            if (c == 'i') return 'İ';
            if (c == 'ı') return 'I';
        }

        return char.ToUpper(c, _culture);
    }

    private char ToLower(char c)
    {
        if (_turkicCasing)
        {
            if (c == 'I') return 'ı';
            if (c == 'İ') return 'i';
        }

        return char.ToLower(c, _culture);
    }

    private static string JoinCorrected(string?[]? values, Func<string?, string?> correct)
    {
        if (values is null || values.Length == 0)
            return string.Empty;

        var corrected = new List<string>(values.Length);

        foreach (string? value in values)
        {
            string? result = correct(value);
            if (!string.IsNullOrEmpty(result))
                corrected.Add(result);
        }

        return string.Join(' ', corrected);
    }

    private static string JoinNonEmpty(string? first, string? second)
    {
        if (string.IsNullOrEmpty(first))
            return second ?? string.Empty;

        if (string.IsNullOrEmpty(second))
            return first;

        return string.Concat(first, " ", second);
    }

    /// <summary>
    /// Characters that have no Unicode decomposition and therefore survive diacritic removal.
    /// </summary>
    private static string? GetSpecialReplacement(char c) => c switch
    {
        'ı' => "i",
        'ł' => "l",
        'Ł' => "L",
        'ø' => "o",
        'Ø' => "O",
        'đ' => "d",
        'Đ' => "D",
        'ß' => "ss",
        'æ' => "ae",
        'Æ' => "AE",
        'œ' => "oe",
        'Œ' => "OE",
        _ => null,
    };

    private static CultureInfo? TryGetCulture(string name)
    {
        try
        {
            return CultureInfo.GetCultureInfo(name);
        }
        catch (CultureNotFoundException)
        {
            // Globalization-invariant mode: casing falls back to the explicit Turkish rules above.
            return null;
        }
    }
}
