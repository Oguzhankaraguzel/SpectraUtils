using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace SpectraUtils.Extensions;

/// <summary>
/// Provides extension methods for <see cref="string"/>.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Returns <see langword="true"/> when the string is <see langword="null"/> or empty.
    /// </summary>
    /// <param name="value">Input string.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNullOrEmpty([NotNullWhen(false)] this string? value)
        => string.IsNullOrEmpty(value);

    /// <summary>
    /// Returns <see langword="true"/> when the string is <see langword="null"/>, empty, or whitespace.
    /// </summary>
    /// <param name="value">Input string.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this string? value)
        => string.IsNullOrWhiteSpace(value);

    /// <summary>
    /// Reverses a string by user-perceived characters (grapheme clusters), so emoji, surrogate pairs,
    /// combining accents and <c>\r\n</c> stay intact (e.g. <c>"a😀b"</c> becomes <c>"b😀a"</c>).
    /// </summary>
    /// <param name="value">Input string.</param>
    /// <returns>Reversed string, or <see langword="null"/> when input is <see langword="null"/>.</returns>
    [return: NotNullIfNotNull(nameof(value))]
    public static string? ReverseSafe(this string? value)
    {
        if (value is null || value.Length <= 1)
            return value;

        // Fast path: below U+0300 every UTF-16 unit is a complete character, except the "\r\n" pair.
        if (value.AsSpan().IndexOfAnyInRange('̀', '￿') < 0 && !value.Contains('\r'))
        {
            return string.Create(value.Length, value, static (span, source) =>
            {
                source.AsSpan().CopyTo(span);
                span.Reverse();
            });
        }

        int[] starts = StringInfo.ParseCombiningCharacters(value);

        return string.Create(value.Length, (value, starts), static (span, state) =>
        {
            (string source, int[] elementStarts) = state;
            int position = 0;

            for (int i = elementStarts.Length - 1; i >= 0; i--)
            {
                int start = elementStarts[i];
                int end = i + 1 < elementStarts.Length ? elementStarts[i + 1] : source.Length;

                source.AsSpan(start, end - start).CopyTo(span[position..]);
                position += end - start;
            }
        });
    }
}
