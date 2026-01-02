using System.Diagnostics.CodeAnalysis;

namespace SpectraUtils.Extensions;

/// <summary>
/// Provides useful LINQ-like extensions.
/// </summary>
public static class LinqExtensions
{
    /// <summary>
    /// Returns <see langword="true"/> when the sequence is <see langword="null"/> or contains no elements.
    /// </summary>
    /// <typeparam name="T">Element type.</typeparam>
    /// <param name="source">Sequence.</param>
    public static bool IsNullOrEmpty<T>([NotNullWhen(false)] this IEnumerable<T>? source)
    {
        if (source is null)
            return true;

        return !source.Any();
    }

    /// <summary>
    /// Applies a <c>Where</c> filter only if <paramref name="condition"/> is <see langword="true"/>.
    /// </summary>
    /// <typeparam name="T">Element type.</typeparam>
    /// <param name="source">Sequence.</param>
    /// <param name="condition">Condition to apply the predicate.</param>
    /// <param name="predicate">Filter predicate.</param>
    /// <returns>Filtered sequence when condition is true; otherwise returns source.</returns>
    /// <exception cref="ArgumentNullException">Thrown when source or predicate is null.</exception>
    public static IEnumerable<T> WhereIf<T>(this IEnumerable<T> source, bool condition, Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);

        return condition ? source.Where(predicate) : source;
    }

    /// <summary>
    /// Applies a <c>Where</c> filter only if <paramref name="condition"/> is <see langword="true"/>.
    /// </summary>
    /// <typeparam name="T">Element type.</typeparam>
    /// <param name="source">Sequence.</param>
    /// <param name="condition">Condition to apply the predicate.</param>
    /// <param name="predicate">Filter predicate.</param>
    /// <returns>Filtered sequence when condition is true; otherwise returns source.</returns>
    /// <exception cref="ArgumentNullException">Thrown when source or predicate is null.</exception>
    public static IQueryable<T> WhereIf<T>(this IQueryable<T> source, bool condition, System.Linq.Expressions.Expression<Func<T, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);

        return condition ? source.Where(predicate) : source;
    }

    /// <summary>
    /// Executes an action for each element in the sequence.
    /// </summary>
    /// <typeparam name="T">Element type.</typeparam>
    /// <param name="source">Sequence.</param>
    /// <param name="action">Action to execute.</param>
    /// <exception cref="ArgumentNullException">Thrown when source or action is null.</exception>
    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(action);

        foreach (var item in source)
            action(item);
    }

    /// <summary>
    /// Executes an action for each element in the sequence and returns the original sequence.
    /// Useful for fluent pipelines.
    /// </summary>
    /// <typeparam name="T">Element type.</typeparam>
    /// <param name="source">Sequence.</param>
    /// <param name="action">Action to execute.</param>
    /// <returns>The original sequence.</returns>
    /// <exception cref="ArgumentNullException">Thrown when source or action is null.</exception>
    public static IEnumerable<T> Tap<T>(this IEnumerable<T> source, Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(action);

        foreach (var item in source)
        {
            action(item);
            yield return item;
        }
    }

    /// <summary>
    /// Distinct by key selector for older target frameworks.
    /// On .NET 6+ you can also use <c>Enumerable.DistinctBy</c>.
    /// </summary>
    /// <typeparam name="T">Element type.</typeparam>
    /// <typeparam name="TKey">Key type.</typeparam>
    /// <param name="source">Sequence.</param>
    /// <param name="keySelector">Key selector.</param>
    /// <param name="comparer">Optional key comparer.</param>
    /// <returns>Sequence with distinct keys.</returns>
    public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector, IEqualityComparer<TKey>? comparer = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keySelector);

        var seen = new HashSet<TKey>(comparer);
        foreach (var item in source)
        {
            if (seen.Add(keySelector(item)))
                yield return item;
        }
    }
}
