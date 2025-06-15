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
using QuanLyHocSinh.ViewModel.QuanLyTaiKhoan;
using System.Windows;
using System.Windows.Controls;

namespace QuanLyHocSinh.View.Controls.QuanLyTaiKhoan
{
    public partial class QuanLyTaiKhoanCaNhanUC : UserControl
    {
        public QuanLyTaiKhoanCaNhanUC()
        {
            InitializeComponent();
        }

        private void PwdNewPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;
            if (passwordBox != null)
            {
                // Update the ViewModel property for the new password
                var viewModel = DataContext as QuanLyTaiKhoanCaNhanViewModel;
                if (viewModel != null)
                {
                    viewModel.NewPassword = passwordBox.Password;
                }
            }
        }

        private void PwdConfirmPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;
            if (passwordBox != null)
            {
                // Update the ViewModel property for the confirm password
                var viewModel = DataContext as QuanLyTaiKhoanCaNhanViewModel;
                if (viewModel != null)
                {
                    viewModel.ConfirmPassword = passwordBox.Password;
                }
            }
        }
    }
}
