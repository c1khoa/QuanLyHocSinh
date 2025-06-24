using QuanLyHocSinh.View.Controls.QuanLyTaiKhoan;
using QuanLyHocSinh.ViewModel;
using QuanLyHocSinh.ViewModel.QuanLyTaiKhoan;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

namespace QuanLyHocSinh.View.Dialogs
{
    public partial class ThemHoSoUC : UserControl
    {
        private QuanLyTaiKhoanThemViewModel _mainVM; // hoặc kiểu tương ứng bạn truyền vào

        public ThemHoSoUC(QuanLyTaiKhoanThemViewModel mainVM)
        {
            InitializeComponent();
            this.DataContext = mainVM; // hoặc ViewModel phù hợp
        }


        private void DatePicker_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is DatePicker datePicker)
            {
                var textBox = FindVisualChild<DatePickerTextBox>(datePicker);
                if (textBox != null)
                {
                    textBox.IsReadOnly = true;           // Không cho nhập
                    textBox.IsHitTestVisible = false;    // Không cho click chuột
                    textBox.Cursor = null;               // Không hiện con trỏ nhập
                }
            }
        }

        private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                    return typedChild;

                var result = FindVisualChild<T>(child);
                if (result != null)
                    return result;
            }
            return null;
        }
        public class InverseBoolToColumnSpanConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                bool isTeacher = (bool)value;
                return isTeacher ? 1 : 2; // nếu không phải giáo viên => span 2 cột (chiếm hết)
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

    }
}
