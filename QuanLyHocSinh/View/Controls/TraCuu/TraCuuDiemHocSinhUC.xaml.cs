using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using QuanLyHocSinh.ViewModel.TraCuu;
using System.Globalization;

namespace QuanLyHocSinh.View.Controls.TraCuu
{
    public partial class TraCuuDiemHocSinhUC : UserControl
    {
        public TraCuuDiemHocSinhUC()
        {
            InitializeComponent();
            this.DataContext = new TraCuuDiemHocSinhViewModel();
        }
    }

    public class DiemToDisplayStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "";
            float diem = (float)value;
            return diem < 0 ? "-" : diem.ToString("F2");
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrWhiteSpace(value as string) || (value as string) == "-") return -1f;
            if (float.TryParse(value as string, out float result)) return result;
            return -1f;
        }
    }
}
