using System.ComponentModel.DataAnnotations;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class MinimumAgeAttribute : ValidationAttribute
{
    private readonly int _minimumAge;

    public MinimumAgeAttribute(int minimumAge)
    {
        _minimumAge = minimumAge;
    }

    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        if (value is DateTime birthDate)
        {
            DateTime minimumDate = DateTime.Today.AddYears(-_minimumAge);
            if (birthDate <= minimumDate)
            {
                return ValidationResult.Success!;
            }
        }

        string errorMessage = $"Birth date must be {_minimumAge} years or older.";
        return new ValidationResult(errorMessage);
    }
}
