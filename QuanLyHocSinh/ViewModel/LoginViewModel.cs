// Namespace hệ thống
using System;
using System.Collections.ObjectModel;
using System.Configuration;
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
        // Sự kiện thông báo đăng nhập thành công (nếu cần dùng)
        public event EventHandler<User> LoginSuccess;

        // Thuộc tính bind với View: tên đăng nhập
        private string _username;
        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }

        // Thuộc tính bind mật khẩu (chuỗi plain text)
        private string _password;
        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }

        // Thuộc tính bind hiện/ẩn mật khẩu (dùng để chuyển đổi Visibility giữa PasswordBox và TextBox)
        private bool _isPasswordVisible;
        public bool IsPasswordVisible
        {
            get => _isPasswordVisible;
            set
            {
                if (_isPasswordVisible != value)
                {
                    _isPasswordVisible = value;
                    OnPropertyChanged();

                    // Cập nhật lại PasswordText (nếu bind thêm)
                    OnPropertyChanged(nameof(PasswordText));
                }
            }
        }

        // Thuộc tính dùng để bind TextBox hiện mật khẩu (có thể dùng chung với Password)
        public string PasswordText
        {
            get => Password;
            set
            {
                Password = value;
                OnPropertyChanged();
            }
        }

        // Thuộc tính bind vai trò đăng nhập được chọn
        private string _selectedRole;
        public string SelectedRole
        {
            get => _selectedRole;
            set
            {
                _selectedRole = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(RoleImagePath)); // để trigger cập nhật ảnh
            }
        }

        public string RoleImagePath
        {
            get
            {
                return SelectedRole?.Trim() switch
                {
                    "Học sinh" => "pack://application:,,,/QuanLyHocSinh;component/Images/student_logo.png",
                    "Giáo viên" => "pack://application:,,,/QuanLyHocSinh;component/Images/teacher_logo.png",
                    "Giáo vụ" => "pack://application:,,,/QuanLyHocSinh;component/Images/admin_logo.png",
                    _ => string.Empty
                };
            }
        }

        // Danh sách các vai trò để lựa chọn trong UI (ComboBox)
        public ObservableCollection<string> Roles { get; set; }

        // ICommand cho nút đăng nhập
        public ICommand LoginCommand { get; set; }

        // ICommand cho nút thoát
        public ICommand LoginExitCommand { get; set; }

        // Constructor
        public LoginViewModel()
        {
            // Khởi tạo danh sách vai trò
            Roles = new ObservableCollection<string> { "Giáo vụ", "Giáo viên", "Học sinh" };

            // Khởi tạo command đăng nhập, kiểm tra điều kiện
            LoginCommand = new RelayCommand<object>(
                (p) => CanLogin(p),
                (p) => Login(p)
            );

            // Command thoát ứng dụng
            LoginExitCommand = new RelayCommand<object>(
    (p) => true,
    (p) =>
    {
        // Tìm LoginWindow hiện tại
        LoginWindow loginWindow = null;
        foreach (Window window in Application.Current.Windows)
        {
            if (window is LoginWindow lw)
            {
                loginWindow = lw;
                break;
            }
        }

        // Mở BeginWindow trước khi đóng LoginWindow
        var beginWindow = new BeginWindow();
        beginWindow.Show(); // QUAN TRỌNG: phải gọi trước Close()

        // Nếu có animation, có thể dùng async như sau:
        _ = WindowAnimationHelper.FadeInAsync(beginWindow);

        // Sau đó mới đóng LoginWindow
        loginWindow?.Close();
    }
);

        }

        /// <summary>
        /// Kiểm tra điều kiện có thể đăng nhập: username, password, role không rỗng
        /// </summary>
        /// <returns></returns>
        private bool CanLogin(object parameter)
        {
            return !string.IsNullOrEmpty(Username)
                && !string.IsNullOrEmpty(Password)
                && !string.IsNullOrEmpty(SelectedRole);
        }

        /// <summary>
        /// Hàm xử lý đăng nhập bất đồng bộ, kết nối database MySQL kiểm tra thông tin người dùng
        /// Nếu đúng sẽ mở MainWindow và đóng LoginWindow hiện tại
        /// </summary>
        private async void Login(object parameter)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

            string query = @"
                SELECT nd.UserID, nd.TenDangNhap, nd.MatKhau, vt.VaiTroID, vt.TenVaiTro
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
                    // Truyền tham số query tránh SQL Injection
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
                                    VaiTroID = reader["VaiTroID"].ToString(),
                                    TenVaiTro = reader["TenVaiTro"].ToString()
                                }
                            };


                            // Khởi tạo MainWindow với ViewModel có CurrentUser
                            var mainVM = new MainViewModel { CurrentUser = user };
                            var mainWindow = new MainWindow { DataContext = mainVM };

                            // Hiển thị mainWindow với animation (nếu có)
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

                            // Ẩn và đóng LoginWindow sau khi đăng nhập thành công
                            if (loginWindow != null)
                            {
                                await WindowAnimationHelper.FadeOutAsync(loginWindow);
                                loginWindow.Close();
                            }
                        }
                        else
                        {
                            // Sai thông tin đăng nhập, hiển thị lỗi
                            MessageBox.Show("Sai thông tin đăng nhập!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
        }
    }
}
