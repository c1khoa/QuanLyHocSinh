using QuanLyHocSinh.ViewModel;
using QuanLyHocSinh.ViewModel.QuanLyTaiKhoan;
using System;
using System.Windows.Controls;

namespace QuanLyHocSinh.View.Controls.QuanLyTaiKhoan
{
    public partial class QuanLyTaiKhoanMainUC : UserControl
    {
        public QuanLyTaiKhoanMainUC(MainViewModel mainVM)
        {
            InitializeComponent();

            if (mainVM == null)
                throw new ArgumentNullException(nameof(mainVM));

            DataContext = new QuanLyTaiKhoanMainViewModel(mainVM);
        }
    }
}
