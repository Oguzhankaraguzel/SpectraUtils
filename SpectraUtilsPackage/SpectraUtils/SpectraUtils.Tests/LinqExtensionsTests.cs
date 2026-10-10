using SpectraUtils.Extensions;
using Xunit;

namespace SpectraUtils.Tests;

public sealed class LinqExtensionsTests
{
    [Fact]
    public void IsNullOrEmpty_Null_ReturnsTrue()
    {
        IEnumerable<int>? source = null;
        Assert.True(source.IsNullOrEmpty());
    }

    [Fact]
    public void IsNullOrEmpty_Empty_ReturnsTrue()
    {
        Assert.True(Array.Empty<int>().IsNullOrEmpty());
        Assert.True(new List<int>().IsNullOrEmpty());
        Assert.True(Enumerable.Empty<int>().IsNullOrEmpty());
    }

    [Fact]
    public void IsNullOrEmpty_WithItems_ReturnsFalse()
    {
        Assert.False(new[] { 1 }.IsNullOrEmpty());
    }

    [Fact]
    public void IsNullOrEmpty_LazySequence_StopsAfterFirstElement()
    {
        int enumerated = 0;
        IEnumerable<int> source = Enumerable.Range(0, 1_000).Select(x => { enumerated++; return x; }).Where(_ => true);

        Assert.False(source.IsNullOrEmpty());
        Assert.Equal(1, enumerated);
    }

    [Fact]
    public void IsNullOrEmpty_Queryable_Works()
    {
        Assert.False(new[] { 1, 2 }.AsQueryable().Where(x => x > 1).IsNullOrEmpty());
        Assert.True(new[] { 1, 2 }.AsQueryable().Where(x => x > 5).IsNullOrEmpty());
    }

    [Fact]
    public void WhereIf_ConditionFalse_ReturnsOriginal()
    {
        var source = new[] { 1, 2, 3 };
        Assert.Same(source, source.WhereIf(false, x => x > 1));
    }

    [Fact]
    public void WhereIf_ConditionTrue_Filters()
    {
        Assert.Equal(new[] { 2, 3 }, new[] { 1, 2, 3 }.WhereIf(true, x => x > 1).ToArray());
    }

    [Fact]
    public void WhereIf_IQueryable_AppliesPredicateWhenTrue()
    {
        Assert.Equal(new[] { 3 }, new[] { 1, 2, 3 }.AsQueryable().WhereIf(true, x => x > 2).ToArray());
    }

    [Fact]
    public void WhereIf_IQueryable_ReturnsOriginalWhenFalse()
    {
        var source = new[] { 1, 2, 3 }.AsQueryable();
        Assert.Same(source, source.WhereIf(false, x => x > 2));
    }

    [Fact]
    public void WhereIf_NullArguments_Throw()
    {
        IEnumerable<int> source = new[] { 1 };
        Assert.Throws<ArgumentNullException>(() => LinqExtensions.WhereIf<int>(null!, true, _ => true));
        Assert.Throws<ArgumentNullException>(() => source.WhereIf(true, null!));
    }

    [Fact]
    public void ForEach_ExecutesAction()
    {
        int sum = 0;
        new[] { 1, 2, 3 }.AsEnumerable().ForEach(x => sum += x);
        Assert.Equal(6, sum);
    }

    [Fact]
    public void ForEach_NullArguments_Throw()
    {
        IEnumerable<int> source = new[] { 1 };
        Assert.Throws<ArgumentNullException>(() => LinqExtensions.ForEach<int>(null!, _ => { }));
        Assert.Throws<ArgumentNullException>(() => source.ForEach(null!));
    }

    [Fact]
    public void Tap_ExecutesActionAndReturnsSequence()
    {
        var values = new[] { 1, 2, 3 };
        int count = 0;

        var result = values.Tap(_ => count++).ToArray();

        Assert.Equal(values, result);
        Assert.Equal(values.Length, count);
    }

    [Fact]
    public void Tap_IsDeferred()
    {
        int count = 0;
        _ = new[] { 1, 2, 3 }.Tap(_ => count++);
        Assert.Equal(0, count);
    }

    [Fact]
    public void Tap_NullArguments_ThrowImmediately()
    {
        // Previously the checks ran only when the result was enumerated.
        IEnumerable<int> source = new[] { 1 };
        Assert.Throws<ArgumentNullException>(() => LinqExtensions.Tap<int>(null!, _ => { }));
        Assert.Throws<ArgumentNullException>(() => source.Tap(null!));
    }

    [Fact]
    public void DistinctBy_WithComparer_ResolvesToBclWithoutAmbiguity()
    {
        // SpectraUtils 2.x shipped its own DistinctBy, which made this call fail with CS0121.
        var values = new[] { "a", "A", "b" };
        Assert.Equal(2, values.DistinctBy(x => x, StringComparer.OrdinalIgnoreCase).Count());
    }
}
