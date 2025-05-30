// Namespace hệ thống
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

// Thư viện bên thứ ba
using MySql.Data.MySqlClient;

// Namespace nội bộ (project)
using QuanLyHocSinh.Model.Entities;
using QuanLyHocSinh.View.Converters;
using QuanLyHocSinh.View.Windows;

namespace QuanLyHocSinh.ViewModel
{
    public class LoginViewModel : BaseViewModel
    {
        // Sự kiện thông báo đăng nhập thành công
        public event EventHandler<User> LoginSuccess;

        // Các thuộc tính bind với View
        private string _username;
        private string _password;
        private string _selectedRole;

        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }

        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }

        public string SelectedRole
        {
            get => _selectedRole;
            set { _selectedRole = value; OnPropertyChanged(); }
        }

        // Danh sách các vai trò có thể đăng nhập
        public ObservableCollection<string> Roles { get; set; }

        // Các ICommand để bind với nút trong giao diện
        public ICommand LoginCommand { get; set; }
        public ICommand ExitCommand { get; set; }
        public ICommand LoginExitCommand { get; set; }

        public LoginViewModel()
        {
            // Khởi tạo danh sách vai trò
            Roles = new ObservableCollection<string> { "Giáo vụ", "Giáo viên", "Học sinh" };

            // Lệnh đăng nhập
            LoginCommand = new RelayCommand<object>(
                (p) => CanLogin(p),
                (p) => Login(p)
            );

            // Lệnh thoát ứng dụng
            LoginExitCommand = new RelayCommand<object>(
                (p) => true,
                (p) => Application.Current.Shutdown()
            );
        }

        // Kiểm tra điều kiện có thể đăng nhập hay chưa
        private bool CanLogin(object parameter)
        {
            return !string.IsNullOrEmpty(Username) &&
                   !string.IsNullOrEmpty(Password) &&
                   !string.IsNullOrEmpty(SelectedRole);
        }

        // Hàm xử lý đăng nhập
        private async void Login(object parameter)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

            string query = @"
                SELECT nd.UserID, nd.TenDangNhap, nd.MatKhau, vt.TenVaiTro
                FROM USERS nd
                JOIN VAITRO vt ON nd.VaiTroID = vt.VaiTroID
                WHERE nd.TenDangNhap = @Username 
                  AND nd.MatKhau = @Password 
                  AND vt.TenVaiTro = @Role
            ";

            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Username", Username);
                    cmd.Parameters.AddWithValue("@Password", Password);
                    cmd.Parameters.AddWithValue("@Role", SelectedRole);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Tạo đối tượng người dùng từ dữ liệu DB
                            var user = new User
                            {
                                UserID = reader["UserID"].ToString(),
                                TenDangNhap = reader["TenDangNhap"].ToString(),
                                MatKhau = reader["MatKhau"].ToString(),
                                VaiTro = new VaiTro
                                {
                                    TenVaiTro = reader["TenVaiTro"].ToString()
                                }
                            };

                            // Tạo và hiển thị MainWindow
                            var mainVM = new MainViewModel { CurrentUser = user };
                            var mainWindow = new MainWindow { DataContext = mainVM };

                            await WindowAnimationHelper.FadeInAsync(mainWindow);

                            // Tìm LoginWindow đang mở
                            LoginWindow loginWindow = null;
                            foreach (Window window in Application.Current.Windows)
                            {
                                if (window is LoginWindow lw)
                                {
                                    loginWindow = lw;
                                    break;
                                }
                            }

                            // Đóng LoginWindow sau khi fade out
                            if (loginWindow != null)
                            {
                                await WindowAnimationHelper.FadeOutAsync(loginWindow);
                                loginWindow.Close();
                            }
                        }
                        else
                        {
                            MessageBox.Show("Sai thông tin đăng nhập!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
        }
    }
}
