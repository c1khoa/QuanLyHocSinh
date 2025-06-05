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
            // Nếu giá trị hoặc tham số null => ẩn
            if (value == null || parameter == null)
                return Visibility.Collapsed;

            // Chuyển value và parameter thành string, trim để tránh khoảng trắng
            string userRole = value.ToString()?.Trim() ?? string.Empty;
            string paramString = parameter.ToString() ?? string.Empty;

            // Tách tham số theo dấu phẩy, bỏ các mục rỗng, trim từng mục
            var expectedRoles = paramString
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(r => r.Trim());

            // Kiểm tra vai trò có trong danh sách tham số không
            bool isVisible = expectedRoles.Contains(userRole, StringComparer.OrdinalIgnoreCase);

            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

