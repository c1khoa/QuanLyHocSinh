using System;
using System.Globalization;
using System.Windows.Data;

namespace QuanLyHocSinh.View.Converters
{
    public class NullableDoubleToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value == DBNull.Value)
                return "-";
            if (value is double d)
            {
                if (double.IsNaN(d)) return "-";
                string format = parameter as string;
                return format != null ? d.ToString(format) : d.ToString();
            }
            if (value is double?)
            {
                var v = (double?)value;
                if (!v.HasValue) return "-";
                string format = parameter as string;
                return format != null ? v.Value.ToString(format) : v.Value.ToString();
            }
            return value.ToString();
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value.ToString() == "-") return null;
            if (double.TryParse(value.ToString(), out double d)) return d;
            return null;
        }
    }
} 