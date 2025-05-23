using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using QuanLyHocSinh.Model.Entities;
using QuanLyHocSinh.ViewModel.TraCuu;

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
}
