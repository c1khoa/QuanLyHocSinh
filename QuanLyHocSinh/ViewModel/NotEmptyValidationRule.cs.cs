using System.Globalization;
using System.Windows.Controls;

namespace QuanLyHocSinh
{
    public class NotEmptyValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (string.IsNullOrWhiteSpace(value as string))
                return new ValidationResult(false, "Không được để trống");
            return ValidationResult.ValidResult;
        }
    }
}
