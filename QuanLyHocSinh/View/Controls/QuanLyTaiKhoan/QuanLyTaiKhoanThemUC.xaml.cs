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
using MySql.Data.MySqlClient;

namespace QuanLyHocSinh.View.Controls.QuanLyTaiKhoan
{
    /// <summary>
    /// Interaction logic for QuanLyTaiKhoan_ThemUC.xaml
    /// </summary>
    public partial class QuanLyTaiKhoanThemUC : UserControl
    {
        private string connectionString = "server=localhost;user=root;password=your_password;database=your_database;";
        public event EventHandler AccountAddedSuccessfully;
        public QuanLyTaiKhoanThemUC()
        {
            InitializeComponent();
        }

        private void ADD_ACCOUNT(object sender, RoutedEventArgs e)
        {

            string userId = txtUserID.Text.Trim();
            string hoTen = txtUserName.Text.Trim();
            string tenDangNhap = txtUserLogin.Text.Trim();
            string matKhau = txtUserPassWord.Password;
            string vaiTro = (txtUserFunction.SelectedItem as ComboBoxItem)?.Content.ToString();


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

                    string query = "INSERT INTO users (UserID, HoTen, TenDangNhap, MatKhau, VaiTro) " +
                                   "VALUES (@UserID, @HoTen, @TenDangNhap, @MatKhau, @VaiTro)";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserID", userId);
                        cmd.Parameters.AddWithValue("@HoTen", hoTen);
                        cmd.Parameters.AddWithValue("@TenDangNhap", tenDangNhap);
                        cmd.Parameters.AddWithValue("@MatKhau", matKhau);
                        cmd.Parameters.AddWithValue("@VaiTro", vaiTro);

                        int result = cmd.ExecuteNonQuery();
                        if (result > 0)
                        {
                            MessageBox.Show("Thêm tài khoản thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                            AccountAddedSuccessfully?.Invoke(this, EventArgs.Empty);
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
        private void ClearForm()
        {
            txtUserID.Text = "";
            txtUserName.Text = "";
            txtUserLogin.Text = "";
            txtUserPassWord.Password = "";
            txtUserFunction.SelectedIndex = -1;
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
        private void txtUserPassWordVisible_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (txtUserPassWordVisible.Visibility == Visibility.Visible)
            {
                txtUserPassWord.Password = txtUserPassWordVisible.Text;
            }
        }


        private void txtUserPassWord_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (txtUserPassWordVisible.Visibility == Visibility.Visible)
            {
                txtUserPassWordVisible.Text = txtUserPassWord.Password;
            }
        }
    }
}
