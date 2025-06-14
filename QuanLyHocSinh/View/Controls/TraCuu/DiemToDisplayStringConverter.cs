using System;
using System.Globalization;
using System.Windows.Data;

namespace QuanLyHocSinh.View.Controls.TraCuu
{
    public class DiemToDisplayStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "-";
            if (value is double d)
                return (d < 0 || double.IsNaN(d)) ? "-" : d.ToString("F2");
            if (value is float f)
                return (f < 0 || float.IsNaN(f)) ? "-" : f.ToString("F2");
            return value.ToString();
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrWhiteSpace(value as string) || (value as string) == "-") return -1f;
            if (float.TryParse(value as string, out float result)) return result;
            return -1f;
        }
    }
} 