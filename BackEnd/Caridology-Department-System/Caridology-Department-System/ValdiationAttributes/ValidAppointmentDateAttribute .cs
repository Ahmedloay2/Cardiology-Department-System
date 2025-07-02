using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
namespace Caridology_Department_System.ValdiationAttributes
{
    public class ValidAppointmentDateAttribute: ValidationAttribute
    {
        private static readonly List<TimeSpan> ValidAppointmentTimes = Enumerable
                    .Range(0, 12) // From 16:00 to 21:30 in 30-min steps
                    .Select(i => new TimeSpan(16, 0, 0).Add(TimeSpan.FromMinutes(i * 30)))
                    .ToList();

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is not DateTime appDate)
            {
                return new ValidationResult("Invalid appointment date format.");
            }

            DateTime now = DateTime.Now.Date;
            int days = (appDate.Date - now).Days;

            if (days > 7)
            {
                return new ValidationResult("You can only make appointments up to 7 days in advance.");
            }
            if (days == 0)
            {
                return new ValidationResult("Same-day appointments are not allowed.");
            }
            if (days < 0)
            {
                return new ValidationResult("You cannot choose a past date.");
            }

            if (!ValidAppointmentTimes.Contains(appDate.TimeOfDay))
            {
                return new ValidationResult("Invalid appointment time. Available slots are every 30 minutes between 4:00 PM and 9:30 PM.");
            }

            return ValidationResult.Success;
        }
    }
}
