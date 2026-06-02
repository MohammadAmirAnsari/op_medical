using MudBlazor;
using OP.PORTAL.Locales;
using System.ComponentModel.DataAnnotations;
using System.Resources;
using System.Text.RegularExpressions;

namespace OP.PORTAL.Models
{
    public class Auditable
    {
        public string? LastModifiedBy { get; set; }
    }

    public class OnlyNumberAttribute : ValidationAttribute
    {
        private readonly int _min;
        private readonly int _max;

        public OnlyNumberAttribute(int min, int max)
        {
            _min = min;
            _max = max;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var str = value as string ?? string.Empty;
            var validatorFunc = OvmcValidator.OnlyNumber(_min, _max);
            // assuming the validator returns null/empty when valid, otherwise an error message
            var errorMessage = validatorFunc(str);

            if (string.IsNullOrEmpty(errorMessage))
                return ValidationResult.Success;

            // prefer attribute's ErrorMessage if set, otherwise use returned message
            var msg = string.IsNullOrEmpty(ErrorMessage) ? errorMessage : ErrorMessage;
            return new ValidationResult(msg);
        }
    }

    public class NoArabicAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var str = value as string ?? string.Empty;
            var errorMessage = OvmcValidator.NoArabic(str);

            if (string.IsNullOrEmpty(errorMessage))
                return ValidationResult.Success;

            var msg = string.IsNullOrEmpty(ErrorMessage) ? errorMessage : ErrorMessage;
            return new ValidationResult(msg);
        }
    }
    public class GenderCheckAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var str = value as string ?? string.Empty;
            var errorMessage = OvmcValidator.GenderCheck(str);

            if (string.IsNullOrEmpty(errorMessage))
                return ValidationResult.Success;

            var msg = string.IsNullOrEmpty(ErrorMessage) ? errorMessage : ErrorMessage;
            return new ValidationResult(msg);
        }
    }
    public class DateOfBirthAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var str = value as DateTime? ?? null;
            var errorMessage = OvmcValidator.DateOfBirth(str);

            if (string.IsNullOrEmpty(errorMessage))
                return ValidationResult.Success;

            var msg = string.IsNullOrEmpty(ErrorMessage) ? errorMessage : ErrorMessage;
            return new ValidationResult(msg);
        }
    }
    public class PassportExpiryDateAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var str = value as DateTime? ?? null;
            var errorMessage = OvmcValidator.PassportExpiryDate(str);

            if (string.IsNullOrEmpty(errorMessage))
                return ValidationResult.Success;

            var msg = string.IsNullOrEmpty(ErrorMessage) ? errorMessage : ErrorMessage;
            return new ValidationResult(msg);
        }
    }

    public class OvmcValidator 
    {
        public static string NoArabic(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return value;

            return System.Text.RegularExpressions.Regex.IsMatch(value, @"[\u0600-\u06FF]")
                ? Resource.ResxArabicInputValidation
                : string.Empty;
        }

        public static string CheckPasswordStrength(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return value;

            if (value.Length < 8)
                return Resource.ResxPassword8Length;
            if (!Regex.IsMatch(value, @"[A-Z]"))
                return Resource.ResxPassword1Upper;
            if (!Regex.IsMatch(value, @"[a-z]"))
                return Resource.ResxPassword1Lower;
            if (!Regex.IsMatch(value, @"[0-9]"))
                return Resource.ResxPassword1Number;
            if (!Regex.IsMatch(value, @"[!@#$%^&*]"))
                return Resource.ResxPassword1Special;

            return string.Empty;
        }

        public static void UppercaseField<T>(T model, string propertyName)
        {
            var prop = typeof(T).GetProperty(propertyName);
            if (prop?.PropertyType == typeof(string) && prop.CanWrite)
            {
                var value = prop.GetValue(model) as string;
                if (!string.IsNullOrEmpty(value))
                {
                    var cleaned = Regex.Replace(value, @"[\u0600-\u06FF]", "");
                    prop.SetValue(model, cleaned.ToUpperInvariant());
                }
            }
        }

        public static Func<string, string> OnlyNumber(int min, int max)
        {
            return value =>
            {
                if (string.IsNullOrWhiteSpace(value))
                    return value;

                if (!value.All(char.IsDigit))
                    return "Only numbers allowed.";

                if (value.Length < min)
                    return $"Minimum {min} digits required.";

                if (value.Length > max)
                    return $"Maximum {max} digits allowed.";

                return string.Empty;
            };
        }

        public static string Email(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return value;

            if (!Regex.IsMatch(value, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                return Resource.ResxInvalidEmail;

            return Regex.IsMatch(value, @"[\u0600-\u06FF]")
                ? Resource.ResxArabicInputValidation
                : string.Empty;
        }

        public static string GenderCheck(string value)
        {
            List<string> genders = new List<string> { "Male", "Female" };
            return genders.Contains(value)?string.Empty:"Gender should be 'Male' or 'Female'";
        }

        public static string DateOfBirth(DateTime? value)
        {
            if (!value.HasValue)
                return Resource.ResxRequired;

            var age = DateTime.Today.Year - value.Value.Year;
            if (value.Value.Date > DateTime.Today.AddYears(-age)) age--;

            if (age < 18 || age > 60)
                return "You must be between 18 and 60 years old";

            return string.Empty;
        }

        public static string PassportExpiryDate(DateTime? value)
        {
            if (!value.HasValue)
                return Resource.ResxRequired;

            if (value.Value.Date < DateTime.Today.AddMonths(6))
                return "Passport Expiry Date must not be less than 6 months";

            return string.Empty;
        }
    }

    public static class OvmcFormat
    {
        public static readonly string AMOUNTFORMAT = "N3";
        public static readonly string DATEFORMAT = "dd/MM/yyyy";
        public static readonly string DATETIMEFORMAT = "dd/MM/yyyy hh:mm tt";

    }

    public static class MasterTypes
    {
        public static readonly string GENDER = "GENDER";
        public static readonly string MARITALSTATUS = "MARITALSTATUS";
        public static readonly string SPONSORTYPE = "SPONSORTYPE";
        public static readonly string VISATYPE = "VISATYPE";
        public static readonly string REQUESTSTATUS = "REQUESTSTATUS";
        public static readonly string PAYMENTSTATUS = "PAYMENTSTATUS";
        public static readonly string COUNTRY = "COUNTRY";
        public static readonly string CITY = "CITY";
    }

    public static class OvmcRequestStatus
    {
        public static readonly string SUBMITTED = "Submitted";
        public static readonly string INPROCESS = "In-Process";
        public static readonly string NOTAPPAIRED = "Not-Appeared";
        public static readonly string COMPLETED = "Completed";
        public static readonly string CANCELLED = "Cancelled";
    }

    public static class OvmcPaymentStatus
    {
        public static readonly string PENDING = "Pending";
        public static readonly string INITIATED = "Initiated";
        public static readonly string SUCCESS = "Success";
        public static readonly string SHIPPED = "Shipped";
        public static readonly string FAILED = "Failure";
        public static readonly string CANCELLED = "Cancelled";

        public static bool IsPaymentSuccess(string status)
        {
            if(string.IsNullOrEmpty(status))
                return false;
            return status.ToLower() == SUCCESS.ToLower() || status.ToLower() == SHIPPED.ToLower();
        }
    }

    public static class RequestStatusColor
    {
        public static Color GetStatusColor(string status)
        {
            return status switch
            {
                "Submitted" => Color.Primary,
                "Pending" => Color.Primary,

                "Initiated" => Color.Warning,
                "In-Process" => Color.Warning,
                
                "Success" => Color.Success,
                "Shipped" => Color.Success,
                "Completed" => Color.Success, 

                _ => Color.Error
            };
        }
    }
}
