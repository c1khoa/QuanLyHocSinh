using QuanLyHocSinh.ViewModel;
using QuanLyHocSinh.ViewModel.QuanLyTaiKhoan;
using System.Windows;
using System.Windows.Controls;

namespace QuanLyHocSinh.View.Controls.QuanLyTaiKhoan
{
    public partial class QuanLyTaiKhoanThemUC : UserControl
    {
        private readonly QuanLyTaiKhoanThemViewModel _viewModel;

        public QuanLyTaiKhoanThemUC(QuanLyTaiKhoanThemViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;

            _viewModel.CancelRequested += () =>
            {
                var mainVM = Application.Current.MainWindow?.DataContext as MainViewModel;
                mainVM?.NavigateBack();
            };

            _viewModel.AccountAddedSuccessfully += user =>
            {
                var mainVM = Application.Current.MainWindow?.DataContext as MainViewModel;
                mainVM?.NavigateBack();
            };
        }

        private void btnShowHidePassword_Checked(object sender, RoutedEventArgs e)
        {
            txtUserPassWordVisible.Text = txtUserPassWord.Password;
            txtUserPassWord.Visibility = Visibility.Collapsed;
            txtUserPassWordVisible.Visibility = Visibility.Visible;
        }

        private void btnShowHidePassword_Unchecked(object sender, RoutedEventArgs e)
        {
            txtUserPassWord.Password = txtUserPassWordVisible.Text;
            txtUserPassWordVisible.Visibility = Visibility.Collapsed;
            txtUserPassWord.Visibility = Visibility.Visible;
        }

        private void txtUserPassWordVisible_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.MatKhau = txtUserPassWordVisible.Text;
            }
        }

        private void txtUserPassWord_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.MatKhau = txtUserPassWord.Password;
            }
        }
    }
}
