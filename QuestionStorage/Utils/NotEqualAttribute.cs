using System.ComponentModel.DataAnnotations;

namespace QuestionStorage.Utils
{
    public class NotEqualAttribute : ValidationAttribute
    {
        private string OtherProperty { get; set; }

        public NotEqualAttribute(string otherProperty) 
        {
            OtherProperty = otherProperty;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var otherPropertyInfo = validationContext.ObjectType.GetProperty(OtherProperty);
            if (otherPropertyInfo is null)
            {
                return new ValidationResult($"{validationContext.MemberName} is required");
            }
            var otherValue = otherPropertyInfo.GetValue(validationContext.ObjectInstance);
            if (value is null)
            {
                return new ValidationResult($"{validationContext.MemberName} is required");
            }
            return value.ToString().Equals(otherValue.ToString()) ? new ValidationResult(
                $"{validationContext.MemberName} should not be equal to {OtherProperty}.") : ValidationResult.Success;
        }
    }
}