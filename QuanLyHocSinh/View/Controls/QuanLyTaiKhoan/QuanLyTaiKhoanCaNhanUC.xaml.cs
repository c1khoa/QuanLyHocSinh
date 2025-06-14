// Add the following event handler methods in the code-behind file (QuanLyTaiKhoanCaNhanUC.xaml.cs)
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
