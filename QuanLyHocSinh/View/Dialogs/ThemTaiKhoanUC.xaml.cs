using QuanLyHocSinh.ViewModel.QuanLyTaiKhoan;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MySql.Data.MySqlClient;
using System.Configuration;

namespace QuanLyHocSinh.View.Dialogs // ✅ Đổi namespace để tránh lỗi trùng tên MessageBox
{
    public partial class ThemTaiKhoanUC : UserControl
    {
        string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
        private QuanLyTaiKhoanThemViewModel _mainVM;

        public ThemTaiKhoanUC(QuanLyTaiKhoanThemViewModel mainVM)
        {
            InitializeComponent();
            _mainVM = mainVM;
            this.DataContext = _mainVM;
        }

        private void BtnTiep_Click(object sender, RoutedEventArgs e)
        {
            _mainVM.CurrentControl = new ThemHoSoUC(_mainVM); // Chuyển sang UserControl tiếp theo
        }

        private void BtnHuy_Click(object sender, RoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow != null)
            {
                parentWindow.Close();
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is QuanLyTaiKhoanThemViewModel vm)
            {
                txtUserPassWord.Password = vm.MatKhau;
                txtUserPassWordVisible.Text = vm.MatKhau;
            }
        }

        private void UpdatePassword()
        {
            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            if (DataContext is QuanLyTaiKhoanThemViewModel viewModel && txtUserPassWord != null)
            {
                viewModel.MatKhau = txtUserPassWord.Password;
            }
        }

        private void txtUserPassWord_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (txtUserPassWordVisible.Visibility == Visibility.Visible)
            {
                txtUserPassWordVisible.Text = txtUserPassWord.Password;
            }
            UpdatePassword();
        }

        private void btnShowHidePassword_Checked(object sender, RoutedEventArgs e)
        {
            EyeIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.EyeOff;
            txtUserPassWordVisible.Visibility = Visibility.Visible;
            txtUserPassWord.Visibility = Visibility.Collapsed;
            txtUserPassWordVisible.Text = txtUserPassWord.Password;
        }

        private void btnShowHidePassword_Unchecked(object sender, RoutedEventArgs e)
        {
            EyeIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Eye;
            txtUserPassWordVisible.Visibility = Visibility.Collapsed;
            txtUserPassWord.Visibility = Visibility.Visible;
            txtUserPassWord.Password = txtUserPassWordVisible.Text;
        }

        private void txtUserPassWordVisible_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtUserPassWordVisible.Visibility == Visibility.Visible)
            {
                txtUserPassWord.Password = txtUserPassWordVisible.Text;
            }
        }

        // Nếu cần xử lý thêm tài khoản tại đây (dù nên làm ở ViewModel theo MVVM)
        private void ADD_ACCOUNT(object sender, RoutedEventArgs e)
        {
            string userId = txtUserID.Text.Trim();
            string tenDangNhap = txtUserLogin.Text.Trim();
            string matKhau = txtUserPassWord.Password;
            string vaiTro = txtUserFunction.Text;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(tenDangNhap) ||
                string.IsNullOrEmpty(matKhau) || string.IsNullOrEmpty(vaiTro))
            {
                System.Windows.MessageBox.Show("Vui lòng nhập đầy đủ thông tin!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "INSERT INTO users (UserID, TenDangNhap, MatKhau, VaiTro) VALUES (@UserID, @TenDangNhap, @MatKhau, @VaiTro)";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserID", userId);
                        cmd.Parameters.AddWithValue("@TenDangNhap", tenDangNhap);
                        cmd.Parameters.AddWithValue("@MatKhau", matKhau);
                        cmd.Parameters.AddWithValue("@VaiTro", vaiTro);

                        int result = cmd.ExecuteNonQuery();
                        if (result > 0)
                        {
                            System.Windows.MessageBox.Show("Thêm tài khoản thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                            ClearForm();
                        }
                        else
                        {
                            System.Windows.MessageBox.Show("Thêm tài khoản thất bại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Lỗi khi kết nối cơ sở dữ liệu:\n" + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearForm()
        {
            txtUserID.Text = "";
            txtUserLogin.Text = "";
            txtUserPassWord.Password = "";
            txtUserPassWordVisible.Text = "";
            txtUserFunction.SelectedIndex = -1;
        }
    }
}
