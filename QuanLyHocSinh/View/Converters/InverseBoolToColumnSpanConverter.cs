using System;
using System.Globalization;
using System.Windows.Data;

namespace QuanLyHocSinh.View.Converters
{
    public class InverseBoolToColumnSpanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isVisible = value is bool b && b;
            return isVisible ? 1 : 2; // Nếu là giáo viên → 1 cột, nếu không → 2 cột
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
