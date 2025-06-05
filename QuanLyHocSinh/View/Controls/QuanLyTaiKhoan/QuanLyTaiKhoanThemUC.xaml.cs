using QuanLyHocSinh.ViewModel;
using QuanLyHocSinh.ViewModel.QuanLyTaiKhoan;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace QuanLyHocSinh.View.Controls.QuanLyTaiKhoan
{
    public partial class QuanLyTaiKhoanThemUC : UserControl
    {
        public QuanLyTaiKhoanThemUC(MainViewModel mainVM)
        {
            if (mainVM == null)
            {
                throw new ArgumentNullException(nameof(mainVM));
            }

            InitializeComponent();

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                DataContext = new QuanLyTaiKhoanThemViewModel(mainVM);
            }
        }

        public void UpdatePassword()
        {
            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            if (DataContext is QuanLyTaiKhoanThemViewModel viewModel && txtUserPassWord != null)
            {
                viewModel.MatKhau = txtUserPassWord.Password;
            }
        }

        public void txtUserPassWord_PasswordChanged(object sender, RoutedEventArgs e)
        {
            UpdatePassword();
        }

        public void btnShowHidePassword_Checked(object sender, RoutedEventArgs e)
        {
            if (txtUserPassWord != null && txtUserPassWordVisible != null)
            {
                txtUserPassWord.Visibility = Visibility.Collapsed;
                txtUserPassWordVisible.Visibility = Visibility.Visible;
                txtUserPassWordVisible.Text = txtUserPassWord.Password;
            }
        }

        public void btnShowHidePassword_Unchecked(object sender, RoutedEventArgs e)
        {
            if (txtUserPassWord != null && txtUserPassWordVisible != null)
            {
                txtUserPassWord.Visibility = Visibility.Visible;
                txtUserPassWordVisible.Visibility = Visibility.Collapsed;
                txtUserPassWord.Password = txtUserPassWordVisible.Text;
                UpdatePassword();
            }
        }

        public void txtUserPassWordVisible_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtUserPassWord != null && txtUserPassWordVisible != null)
            {
                txtUserPassWord.Password = txtUserPassWordVisible.Text;
                UpdatePassword();
            }
        }
    }
}