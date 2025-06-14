using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;

namespace QuanLyHocSinh.View.Converters
{
    public class RoleToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return Visibility.Collapsed;

            string userRole = value.ToString().Trim();

            // Cho phép parameter có nhiều vai trò, phân tách bởi dấu phẩy
            var expectedRoles = parameter.ToString()
                                            .Split(',')
                                            .Select(r => r.Trim());

            return expectedRoles.Contains(userRole) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

