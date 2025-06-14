using QuanLyHocSinh.ViewModel;
using QuanLyHocSinh.ViewModel.QuanLyTaiKhoan;
using System.ComponentModel;
using System;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;
using System.Configuration;

namespace QuanLyHocSinh.View.Controls.QuanLyTaiKhoan
{
    public partial class QuanLyTaiKhoanThemUC : UserControl
    {
        // Chuỗi kết nối MySQL - thay your_password, your_database tương ứng
        string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

        // Sự kiện báo đã thêm tài khoản thành công, để ViewModel hoặc MainWindow bắt
        public event EventHandler AccountAddedSuccessfully;
        public QuanLyTaiKhoanThemUC()
        {
            InitializeComponent();
        }

        public QuanLyTaiKhoanThemUC(MainViewModel mainVM)
        {
            if (mainVM != null)
                DataContext = new QuanLyTaiKhoanThemViewModel(mainVM);
            else
                DataContext = new QuanLyTaiKhoanThemViewModel(new MainViewModel()); // hoặc xử lý null phù hợp

            InitializeComponent();
        }

        // Cập nhật mật khẩu từ PasswordBox vào ViewModel mỗi khi mật khẩu thay đổi
        private void UpdatePassword()
        {
            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            if (DataContext is QuanLyTaiKhoanThemViewModel viewModel && txtUserPassWord != null)
            {
                viewModel.MatKhau = txtUserPassWord.Password;
            }
        }

        // Bắt sự kiện khi PasswordBox bị thay đổi mật khẩu, cập nhật ViewModel
        private void txtUserPassWord_PasswordChanged(object sender, RoutedEventArgs e)
        {
            // Đồng bộ mật khẩu khi đang ở chế độ hiện text mật khẩu
            if (txtUserPassWordVisible.Visibility == Visibility.Visible)
            {
                txtUserPassWordVisible.Text = txtUserPassWord.Password;
            }
            UpdatePassword();
        }

        // Sự kiện Checked của checkbox (hoặc toggle button) để hiển thị mật khẩu dạng text
        private void btnShowHidePassword_Checked(object sender, RoutedEventArgs e)
        {
            // Thay đổi icon mắt thành "EyeOff" (ẩn mật khẩu)
            EyeIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.EyeOff;

            // Hiển thị TextBox (hiện mật khẩu dạng text)
            txtUserPassWordVisible.Visibility = Visibility.Visible;
            txtUserPassWord.Visibility = Visibility.Collapsed;

            // Đồng bộ mật khẩu từ PasswordBox sang TextBox
            txtUserPassWordVisible.Text = txtUserPassWord.Password;
        }

        // Sự kiện Unchecked của checkbox để ẩn mật khẩu (hiển thị dấu chấm)
        private void btnShowHidePassword_Unchecked(object sender, RoutedEventArgs e)
        {
            // Thay đổi icon mắt thành "Eye" (hiện mật khẩu)
            EyeIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Eye;

            // Ẩn TextBox, hiển thị PasswordBox
            txtUserPassWordVisible.Visibility = Visibility.Collapsed;
            txtUserPassWord.Visibility = Visibility.Visible;

            // Đồng bộ mật khẩu từ TextBox sang PasswordBox
            txtUserPassWord.Password = txtUserPassWordVisible.Text;
        }

        // Khi TextBox hiển thị mật khẩu được thay đổi nội dung
        private void txtUserPassWordVisible_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtUserPassWordVisible.Visibility == Visibility.Visible)
            {
                // Cập nhật mật khẩu trong PasswordBox theo TextBox
                txtUserPassWord.Password = txtUserPassWordVisible.Text;
            }
        }

        // Xử lý sự kiện khi người dùng click nút thêm tài khoản
        private void ADD_ACCOUNT(object sender, RoutedEventArgs e)
        {
            string userId = txtUserID.Text.Trim();
            string hoTen = txtUserName.Text.Trim();
            string tenDangNhap = txtUserLogin.Text.Trim();
            string matKhau = txtUserPassWord.Password;
            string vaiTro = (txtUserFunction.SelectedItem as ComboBoxItem)?.Content.ToString();

            // Kiểm tra nhập liệu bắt buộc
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(hoTen) ||
                string.IsNullOrEmpty(tenDangNhap) || string.IsNullOrEmpty(matKhau) ||
                string.IsNullOrEmpty(vaiTro))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    // Câu lệnh INSERT tài khoản mới
                    string query = "INSERT INTO users (UserID, HoTen, TenDangNhap, MatKhau, VaiTro) " +
                                   "VALUES (@UserID, @HoTen, @TenDangNhap, @MatKhau, @VaiTro)";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        // Truyền tham số để tránh SQL Injection
                        cmd.Parameters.AddWithValue("@UserID", userId);
                        cmd.Parameters.AddWithValue("@HoTen", hoTen);
                        cmd.Parameters.AddWithValue("@TenDangNhap", tenDangNhap);
                        cmd.Parameters.AddWithValue("@MatKhau", matKhau);
                        cmd.Parameters.AddWithValue("@VaiTro", vaiTro);

                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Thêm tài khoản thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                            // Kích hoạt sự kiện báo đã thêm thành công
                            AccountAddedSuccessfully?.Invoke(this, EventArgs.Empty);

                            // Xóa trắng form nhập liệu
                            ClearForm();
                        }
                        else
                        {
                            MessageBox.Show("Thêm tài khoản thất bại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi kết nối cơ sở dữ liệu:\n" + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Xóa trắng form sau khi thêm tài khoản thành công
        private void ClearForm()
        {
            txtUserID.Text = "";
            txtUserName.Text = "";
            txtUserLogin.Text = "";
            txtUserPassWord.Password = "";
            txtUserPassWordVisible.Text = "";
            txtUserFunction.SelectedIndex = -1;
        }
    }
}
