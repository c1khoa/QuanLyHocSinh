using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace QuanLyHocSinh.View.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = (bool)value;
            if (parameter is string param && bool.TryParse(param, out bool invert) && invert)
                boolValue = !boolValue;

            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var visibility = (Visibility)value;
            bool result = visibility == Visibility.Visible;
            if (parameter is string param && bool.TryParse(param, out bool invert) && invert)
                result = !result;
            return result;
        }
    }

    public class EyeIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isVisible = (bool)value;
            return isVisible ? "EyeOff" : "Eye";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }

}