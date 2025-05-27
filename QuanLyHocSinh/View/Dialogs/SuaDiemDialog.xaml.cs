using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using QuanLyHocSinh.Model.Entities;
using QuanLyHocSinh.ViewModel.TraCuu;
using System.Globalization;
using System.Windows.Data;

namespace QuanLyHocSinh.View.Dialogs
{
    public partial class SuaDiemDialog : Window
    {
        //Khởi tạo điểm
        public SuaDiemDialog(Diem diem)
        {
            InitializeComponent();
            var vm = new SuaDiemViewModel(diem);
            vm.CloseDialog += (result) =>
            {
                this.DialogResult = result;
                this.Close();
            };
            DataContext = vm;
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex(@"^[0-9]*(?:\.[0-9]*)?$");
            e.Handled = !regex.IsMatch(e.Text);
        }
    }

    public class DiemToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "";
            float diem = (float)value;
            return diem < 0 ? "" : diem.ToString("F2");
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrWhiteSpace(value as string)) return -1f;
            if (float.TryParse(value as string, out float result)) return result;
            return -1f;
        }
    }
}
