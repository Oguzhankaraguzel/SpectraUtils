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
    public static bool IsNullOrEmpty(this string? value)
        => string.IsNullOrEmpty(value);

    /// <summary>
    /// Returns <see langword="true"/> when the string is <see langword="null"/>, empty, or whitespace.
    /// </summary>
    /// <param name="value">Input string.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNullOrWhiteSpace(this string? value)
        => string.IsNullOrWhiteSpace(value);

    /// <summary>
    /// Reverses a string in a safe and allocation-efficient manner.
    /// </summary>
    /// <param name="value">Input string.</param>
    /// <returns>Reversed string, or <see langword="null"/> when input is <see langword="null"/>.</returns>.
    /// <remarks>
    /// This method does not use unsafe code to keep the package compatible with restrictive environments.
    /// It uses <see cref="string.Create(int, TState, System.Buffers.SpanAction{char, TState})"/> to allocate the result only once.
    /// </remarks>
    public static string? ReverseSafe(this string? value)
    {
        if (value is null)
            return null;

        if (value.Length <= 1)
            return value;

        return string.Create(value.Length, value, static (span, src) =>
        {
            for (int i = 0, j = src.Length - 1; i < span.Length; i++, j--)
                span[i] = src[j];
        });
    }
}

