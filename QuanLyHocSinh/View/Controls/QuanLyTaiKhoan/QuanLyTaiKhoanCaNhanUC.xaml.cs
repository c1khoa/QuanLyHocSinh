using QuanLyHocSinh.ViewModel;
using QuanLyHocSinh.ViewModel.QuanLyTaiKhoan;
using System.Windows.Controls;

namespace QuanLyHocSinh.View.Controls.QuanLyTaiKhoan
{
    public partial class QuanLyTaiKhoanCaNhanUC : UserControl
    {
        public QuanLyTaiKhoanCaNhanUC(MainViewModel mainVM)
        {
            InitializeComponent();
            DataContext = new QuanLyTaiKhoanCaNhanViewModel(mainVM);
        }
    }
}