using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace SpectraUtils.Attributes.ValidationAttributes;

/// <summary>
/// Validates that a birth date (<see cref="DateOnly"/>, <see cref="DateTime"/> or <see cref="DateTimeOffset"/>)
/// belongs to someone who is at least <see cref="MinimumAge"/> years old.
/// <see langword="null"/> values are considered valid; combine with <see cref="RequiredAttribute"/> when the value is mandatory.
/// </summary>
/// <remarks>
/// <para>The error message supports <c>{0}</c> (display name) and <c>{1}</c> (minimum age) placeholders.</para>
/// <para>
/// "Today" comes from the <see cref="TimeProvider"/> registered in the validation context's service provider
/// (ASP.NET Core passes the request services), or <see cref="TimeProvider.System"/> when none is registered.
/// This makes the attribute testable and lets you control the time zone.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class MinimumAgeAttribute : ValidationAttribute
{
    private const string DefaultErrorMessage = "The {0} field requires a minimum age of {1}.";

    /// <summary>
    /// Initializes a new instance of the <see cref="MinimumAgeAttribute"/> class.
    /// </summary>
    /// <param name="minimumAge">The minimum age required.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when minimum age is negative.</exception>
    public MinimumAgeAttribute(int minimumAge)
        : base(DefaultErrorMessage)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(minimumAge);
        MinimumAge = minimumAge;
    }

    /// <summary>
    /// Gets the minimum age requirement.
    /// </summary>
    public int MinimumAge { get; }

    /// <inheritdoc />
    public override string FormatErrorMessage(string name)
        => string.Format(CultureInfo.CurrentCulture, ErrorMessageString, name, MinimumAge);

    /// <inheritdoc />
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        // validationContext is null when the attribute is used through IsValid(object).
        ValidationContext? context = validationContext;

        if (value is null)
            return ValidationResult.Success;

        DateOnly birthDate;
        switch (value)
        {
            case DateOnly dateOnly:
                birthDate = dateOnly;
                break;
            case DateTime dateTime:
                birthDate = DateOnly.FromDateTime(dateTime);
                break;
            case DateTimeOffset dateTimeOffset:
                birthDate = DateOnly.FromDateTime(dateTimeOffset.DateTime);
                break;
            default:
                return new ValidationResult(
                    $"The {context?.DisplayName ?? "value"} field must be a DateOnly, DateTime or DateTimeOffset value.",
                    MemberNames(context));
        }

        TimeProvider timeProvider = context?.GetService(typeof(TimeProvider)) as TimeProvider ?? TimeProvider.System;
        DateOnly today = DateOnly.FromDateTime(timeProvider.GetLocalNow().DateTime);

        return CalculateAge(birthDate, today) >= MinimumAge
            ? ValidationResult.Success
            : new ValidationResult(FormatErrorMessage(context?.DisplayName ?? "value"), MemberNames(context));
    }

    /// <summary>
    /// Calculates age in completed years. Someone born on 29 February turns a year older on 28 February in non-leap years.
    /// </summary>
    internal static int CalculateAge(DateOnly birthDate, DateOnly today)
    {
        int age = today.Year - birthDate.Year;

        if (birthDate.AddYears(age) > today)
            age--;

        return age;
    }

    private static string[]? MemberNames(ValidationContext? context)
        => context?.MemberName is { } memberName ? [memberName] : null;
}
