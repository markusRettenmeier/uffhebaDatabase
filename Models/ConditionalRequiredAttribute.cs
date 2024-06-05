using System.ComponentModel.DataAnnotations;

namespace Sammlerplattform.Models
{
    public class ConditionalRequiredAttribute(string dependentPropertyName, object targetValue) : ValidationAttribute
    {
        public string DependentPropertyName { get; } = dependentPropertyName;
        public object TargetValue { get; } = targetValue;

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            System.Reflection.PropertyInfo? dependentProperty = validationContext.ObjectType.GetProperty(DependentPropertyName);
            if (dependentProperty == null)
            {
                return new ValidationResult($"Unknown property: {DependentPropertyName}");
            }

            object? dependentPropertyValue = dependentProperty.GetValue(validationContext.ObjectInstance);
            if (dependentPropertyValue != null && dependentPropertyValue.Equals(TargetValue))
            {
                // If dependent property matches the target value, require validation
                if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                {
                    return new ValidationResult(ErrorMessage);
                }
            }

            // If not required, return success
            return ValidationResult.Success ?? new ValidationResult("");
        }
    }
}