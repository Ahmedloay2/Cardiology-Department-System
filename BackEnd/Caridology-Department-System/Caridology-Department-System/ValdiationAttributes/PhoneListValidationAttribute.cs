using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

public class PhoneListValidationAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var phoneNumbers = value as List<string>;

        if (phoneNumbers == null)
            return ValidationResult.Success; // Nothing to validate

        var phoneRegex = new Regex(@"^(?:\+20|0)?1[0125]\d{8}$");

        foreach (var phone in phoneNumbers)
        {
            if (!phoneRegex.IsMatch(phone))
            {
                return new ValidationResult("One or more phone numbers are invalid.");
            }
        }

        return ValidationResult.Success;
    }
}
