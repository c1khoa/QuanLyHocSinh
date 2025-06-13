using QuanLyHocSinh.Model.Entities;
using QuanLyHocSinh.ViewModel;
using QuanLyHocSinh.ViewModel.QuanLyTaiKhoan;
using System.Windows.Controls;

namespace QuanLyHocSinh.View.Controls.QuanLyTaiKhoan
{
    public partial class QuanLyTaiKhoanSuaUC : UserControl
    {
        public QuanLyTaiKhoanSuaUC()
        {
            InitializeComponent();
        }

        public QuanLyTaiKhoanSuaUC(User userToEdit, MainViewModel mainVM)
        {
            InitializeComponent();
            DataContext = new QuanLyTaiKhoanSuaViewModel(userToEdit, mainVM);
        }
    }

}