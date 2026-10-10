using System.ComponentModel.DataAnnotations;

namespace SpectraUtils.Tests.TestAssets;

/// <summary>
/// A <see cref="TimeProvider"/> that always returns the same instant in UTC.
/// </summary>
internal sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => now;

    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
}

/// <summary>
/// Minimal <see cref="IServiceProvider"/> that serves a single <see cref="TimeProvider"/>.
/// </summary>
internal sealed class TimeServiceProvider(TimeProvider timeProvider) : IServiceProvider
{
    public object? GetService(Type serviceType) => serviceType == typeof(TimeProvider) ? timeProvider : null;
}

internal static class ModelValidator
{
    public static (bool IsValid, List<ValidationResult> Results) Validate(object model, IServiceProvider? services = null)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(model, services, items: null);
        bool ok = Validator.TryValidateObject(model, context, results, validateAllProperties: true);
        return (ok, results);
    }
}

internal static class ValidationResultAssert
{
    public static void Single(List<ValidationResult> results, string memberName)
    {
        ValidationResult result = Xunit.Assert.Single(results);
        Xunit.Assert.Equal(new[] { memberName }, result.MemberNames);
    }
}
