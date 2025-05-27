using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using QuanLyHocSinh.View.Windows;
using QuanLyHocSinh.Model.Entities;
using System.Configuration;
using MySql.Data.MySqlClient;

namespace QuanLyHocSinh.ViewModel
{
    public class LoginViewModel : BaseViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<User> LoginSuccess;
        public ICommand LoginExitCommand { get; set; }

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

        public ObservableCollection<string> Roles { get; set; }

        public ICommand LoginCommand { get; set; }
        public ICommand ExitCommand { get; set; }

        public LoginViewModel()
        {
            Roles = new ObservableCollection<string> { "Giáo vụ", "Giáo viên", "Học sinh" };
            LoginCommand = new RelayCommand<object>(
                (p) => CanLogin(null),
                (p) => Login(null)
            );
            LoginExitCommand = new RelayCommand<object>(
                (p) => true,              // Luôn luôn có thể bấm
                (p) => Application.Current.Shutdown()  // Thực thi thoát app
            );
        }
        private bool CanLogin(object parameter)
        {
            return !string.IsNullOrEmpty(Username) &&
                   !string.IsNullOrEmpty(Password) &&
                   !string.IsNullOrEmpty(SelectedRole);
        }


        private void Login(object parameter)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

            string query = @"
        SELECT nd.UserID, nd.TenDangNhap, nd.MatKhau, vt.TenVaiTro
        FROM USERS nd
        JOIN VAITRO vt ON nd.VaiTroID = vt.VaiTroID
        WHERE nd.TenDangNhap = @Username AND nd.MatKhau = @Password AND vt.TenVaiTro = @Role
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
                            // Tạo đối tượng người dùng (nếu có class User)
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

                            // Mở MainWindow
                            var mainVM = new MainViewModel { CurrentUser = user };
                            var mainWindow = new MainWindow { DataContext = mainVM };
                            mainWindow.Show();

                            // Đóng LoginWindow hiện tại
                            foreach (Window window in Application.Current.Windows)
                            {
                                if (window is LoginWindow)
                                {
                                    window.Close();
                                    break;
                                }
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


        //private void Login(object parameter)
        //{
        //    if (Username == "admin" && Password == "123456" && SelectedRole == "Giáo vụ")
        //    {
        //        var user = new User
        //        {
        //            TenDangNhap = Username,
        //            VaiTro = new VaiTro { TenVaiTro = SelectedRole }
        //        };

        //        LoginSuccess?.Invoke(this, user);
        //    }
        //    else
        //    {
        //        MessageBox.Show("Sai thông tin đăng nhập!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //}




        private void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

