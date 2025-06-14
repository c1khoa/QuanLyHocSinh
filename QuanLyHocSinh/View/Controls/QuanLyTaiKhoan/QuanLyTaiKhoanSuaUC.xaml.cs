using QuanLyHocSinh.Model.Entities;
using QuanLyHocSinh.ViewModel;
using QuanLyHocSinh.ViewModel.QuanLyTaiKhoan;
using System.Windows;
using System.Windows.Controls;

namespace QuanLyHocSinh.View.Controls.QuanLyTaiKhoan
{
    public partial class QuanLyTaiKhoanSuaUC : UserControl
    {
        private QuanLyTaiKhoanSuaViewModel _viewModel;

        public QuanLyTaiKhoanSuaUC(User currentUser, MainViewModel mainVM)
        {
            InitializeComponent();
            _viewModel = new QuanLyTaiKhoanSuaViewModel(currentUser, mainVM);
            DataContext = _viewModel;
        }

        private void PwdNewPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is QuanLyTaiKhoanSuaViewModel vm)
            {
                vm.NewPassword = ((PasswordBox)sender).Password;
            }
        }

        private void PwdConfirmPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is QuanLyTaiKhoanSuaViewModel vm)
            {
                vm.ConfirmPassword = ((PasswordBox)sender).Password;
            }
        }
        // Mật khẩu mới
        private void BtnToggleNewPassword_Checked(object sender, RoutedEventArgs e)
        {
            TxtNewPasswordVisible.Text = PwdNewPassword.Password;
            PwdNewPassword.Visibility = Visibility.Collapsed;
            TxtNewPasswordVisible.Visibility = Visibility.Visible;
            IconNewPasswordEye.Kind = MaterialDesignThemes.Wpf.PackIconKind.EyeOff;
        }

        private void BtnToggleNewPassword_Unchecked(object sender, RoutedEventArgs e)
        {
            PwdNewPassword.Password = TxtNewPasswordVisible.Text;
            PwdNewPassword.Visibility = Visibility.Visible;
            TxtNewPasswordVisible.Visibility = Visibility.Collapsed;
            IconNewPasswordEye.Kind = MaterialDesignThemes.Wpf.PackIconKind.Eye;
        }

        private void TxtNewPasswordVisible_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (BtnToggleNewPassword.IsChecked == true)
                ((QuanLyTaiKhoanSuaViewModel)DataContext).NewPassword = TxtNewPasswordVisible.Text;
        }

        // Xác nhận mật khẩu
        private void BtnToggleConfirmPassword_Checked(object sender, RoutedEventArgs e)
        {
            TxtConfirmPasswordVisible.Text = PwdConfirmPassword.Password;
            PwdConfirmPassword.Visibility = Visibility.Collapsed;
            TxtConfirmPasswordVisible.Visibility = Visibility.Visible;
            IconConfirmPasswordEye.Kind = MaterialDesignThemes.Wpf.PackIconKind.EyeOff;
        }

        private void BtnToggleConfirmPassword_Unchecked(object sender, RoutedEventArgs e)
        {
            PwdConfirmPassword.Password = TxtConfirmPasswordVisible.Text;
            PwdConfirmPassword.Visibility = Visibility.Visible;
            TxtConfirmPasswordVisible.Visibility = Visibility.Collapsed;
            IconConfirmPasswordEye.Kind = MaterialDesignThemes.Wpf.PackIconKind.Eye;
        }

     

        private void TxtConfirmPasswordVisible_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (BtnToggleConfirmPassword.IsChecked == true)
                ((QuanLyTaiKhoanSuaViewModel)DataContext).ConfirmPassword = TxtConfirmPasswordVisible.Text;
        }

    }
}