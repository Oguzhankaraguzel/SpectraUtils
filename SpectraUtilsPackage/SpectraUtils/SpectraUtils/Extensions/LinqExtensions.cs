using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace SpectraUtils.Extensions;

/// <summary>
/// Provides useful LINQ-like extensions.
/// </summary>
public static class LinqExtensions
{
    /// <summary>
    /// Returns <see langword="true"/> when the sequence is <see langword="null"/> or contains no elements.
    /// </summary>
    /// <remarks>
    /// Collections are checked through their count without enumeration. An <see cref="IQueryable{T}"/> is
    /// checked with <see cref="Queryable.Any{TSource}(IQueryable{TSource})"/> so the provider can translate it
    /// (e.g. to <c>EXISTS</c> in SQL). Other lazy sequences are enumerated up to their first element.
    /// </remarks>
    /// <typeparam name="T">Element type.</typeparam>
    /// <param name="source">Sequence.</param>
    public static bool IsNullOrEmpty<T>([NotNullWhen(false)] this IEnumerable<T>? source)
    {
        if (source is null)
            return true;

        if (source.TryGetNonEnumeratedCount(out int count))
            return count == 0;

        if (source is IQueryable<T> queryable)
            return !queryable.Any();

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
    /// <param name="source">Query.</param>
    /// <param name="condition">Condition to apply the predicate.</param>
    /// <param name="predicate">Filter predicate.</param>
    /// <returns>Filtered query when condition is true; otherwise returns source.</returns>
    /// <exception cref="ArgumentNullException">Thrown when source or predicate is null.</exception>
    public static IQueryable<T> WhereIf<T>(this IQueryable<T> source, bool condition, Expression<Func<T, bool>> predicate)
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

        foreach (T item in source)
            action(item);
    }

    /// <summary>
    /// Executes an action for each element as the sequence is enumerated and yields the element unchanged.
    /// Useful for logging or debugging inside fluent pipelines. Execution is deferred.
    /// </summary>
    /// <typeparam name="T">Element type.</typeparam>
    /// <param name="source">Sequence.</param>
    /// <param name="action">Action to execute.</param>
    /// <returns>The original sequence.</returns>
    /// <exception cref="ArgumentNullException">Thrown immediately when source or action is null.</exception>
    public static IEnumerable<T> Tap<T>(this IEnumerable<T> source, Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(action);

        return TapIterator(source, action);
    }

    private static IEnumerable<T> TapIterator<T>(IEnumerable<T> source, Action<T> action)
    {
        foreach (T item in source)
        {
            action(item);
            yield return item;
        }
    }
}
