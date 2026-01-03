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
        var source = Array.Empty<int>();
        Assert.True(source.IsNullOrEmpty());
    }

    [Fact]
    public void IsNullOrEmpty_WithItems_ReturnsFalse()
    {
        var source = new[] { 1 };
        Assert.False(source.IsNullOrEmpty());
    }

    [Fact]
    public void WhereIf_ConditionFalse_ReturnsOriginal()
    {
        var source = new[] { 1, 2, 3 };
        var result = source.WhereIf(false, x => x > 1);

        Assert.True(ReferenceEquals(source, result));
    }

    [Fact]
    public void WhereIf_ConditionTrue_Filters()
    {
        var source = new[] { 1, 2, 3 };
        var result = source.WhereIf(true, x => x > 1).ToArray();

        Assert.Equal(new[] { 2, 3 }, result);
    }

    [Fact]
    public void WhereIf_IQueryable_AppliesPredicateWhenTrue()
    {
        var source = new[] { 1, 2, 3 }.AsQueryable();
        var filtered = source.WhereIf(true, x => x > 2).ToArray();

        Assert.Equal(new[] { 3 }, filtered);
    }

    [Fact]
    public void WhereIf_IQueryable_ReturnsOriginalWhenFalse()
    {
        var source = new[] { 1, 2, 3 }.AsQueryable();
        var filtered = source.WhereIf(false, x => x > 2);

        Assert.Equal(source, filtered);
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
        var values = new[] { 1, 2, 3 };
        int sum = 0;

        values.ForEach(x => sum += x);

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
    public void DistinctBy_RemovesDuplicates()
    {
        var values = new[]
        {
            new Item("a", 1),
            new Item("b", 1),
            new Item("c", 2)
        };

        var distinct = values.DistinctBy(x => x.Group).ToArray();

        Assert.Equal(2, distinct.Length);
        Assert.Equal("a", distinct[0].Name);
        Assert.Equal("c", distinct[1].Name);
    }

    private sealed record Item(string Name, int Group);
}
