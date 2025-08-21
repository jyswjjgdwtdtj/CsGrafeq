using System.ComponentModel.DataAnnotations;

namespace CsGrafeqApp.Attributes.Validation;

internal class ShouldBeDoubleAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string s && double.TryParse(s, out _)) return ValidationResult.Success;
        if (value is double) return ValidationResult.Success;
        return new ValidationResult($"[{validationContext.DisplayName}] should be double type");
    }
}