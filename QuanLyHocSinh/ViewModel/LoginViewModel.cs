// Các Namespace của hệ thống
using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Windows;
using System.Windows.Input;

// Thư viện bên thứ ba
using MaterialDesignThemes.Wpf;
using MySql.Data.MySqlClient;

// Các Namespace nội bộ của dự án
using QuanLyHocSinh.Model.Entities;
using QuanLyHocSinh.View.Converters; // Không sử dụng trong code này, có thể xóa nếu không cần
using QuanLyHocSinh.View.Dialogs.MessageBox;
using QuanLyHocSinh.View.Windows;

namespace QuanLyHocSinh.ViewModel
{
    /// <summary>
    /// ViewModel cho màn hình đăng nhập.
    /// Xử lý logic nghiệp vụ liên quan đến việc xác thực người dùng và điều hướng ứng dụng sau khi đăng nhập.
    /// </summary>
    public class LoginViewModel : BaseViewModel
    {
        #region Events (Sự kiện)

        /// <summary>
        /// Sự kiện được kích hoạt khi đăng nhập thành công.
        /// Có thể được sử dụng để truyền thông tin người dùng đã đăng nhập cho các thành phần khác.
        /// </summary>
        public event EventHandler<User> LoginSuccess;

        #endregion

        #region Properties (Thuộc tính)

        private string _username;
        /// <summary>
        /// Thuộc tính bind với trường nhập tên đăng nhập trên View.
        /// </summary>
        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }

        private string _password;
        /// <summary>
        /// Thuộc tính bind với trường nhập mật khẩu trên View (dạng chuỗi văn bản thuần).
        /// </summary>
        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }

        private bool _isPasswordVisible;
        /// <summary>
        /// Thuộc tính điều khiển việc hiển thị/ẩn mật khẩu trên View.
        /// Khi giá trị thay đổi, sẽ cập nhật lại thuộc tính <see cref="PasswordText"/>.
        /// </summary>
        public bool IsPasswordVisible
        {
            get => _isPasswordVisible;
            set
            {
                if (_isPasswordVisible != value)
                {
                    _isPasswordVisible = value;
                    OnPropertyChanged();
                    // Cập nhật lại PasswordText để View có thể phản ứng với sự thay đổi này
                    OnPropertyChanged(nameof(PasswordText));
                }
            }
        }

        /// <summary>
        /// Thuộc tính dùng để bind với TextBox khi mật khẩu được hiển thị.
        /// Giá trị này đồng bộ với thuộc tính <see cref="Password"/>.
        /// </summary>
        public string PasswordText
        {
            get => Password;
            set
            {
                Password = value;
                OnPropertyChanged();
            }
        }

        private string _selectedRole;
        /// <summary>
        /// Thuộc tính bind với vai trò người dùng được chọn từ ComboBox trên View.
        /// Khi vai trò thay đổi, hình ảnh biểu tượng vai trò cũng sẽ được cập nhật.
        /// </summary>
        public string SelectedRole
        {
            get => _selectedRole;
            set
            {
                _selectedRole = value;
                OnPropertyChanged();
                // Cập nhật lại đường dẫn ảnh vai trò khi SelectedRole thay đổi
                OnPropertyChanged(nameof(RoleImagePath));
            }
        }

        /// <summary>
        /// Thuộc tính trả về đường dẫn hình ảnh tương ứng với vai trò đã chọn.
        /// </summary>
        public string RoleImagePath
        {
            get
            {
                return SelectedRole?.Trim() switch
                {
                    "Học sinh" => "pack://application:,,,/QuanLyHocSinh;component/Images/student_logo.png",
                    "Giáo viên" => "pack://application:,,,/QuanLyHocSinh;component/Images/teacher_logo.png",
                    "Giáo vụ" => "pack://application:,,,/QuanLyHocSinh;component/Images/admin_logo.png",
                    _ => string.Empty // Trả về chuỗi rỗng nếu không có vai trò nào được chọn hoặc vai trò không hợp lệ
                };
            }
        }

        /// <summary>
        /// Danh sách các vai trò có sẵn để người dùng lựa chọn trong UI (ví dụ: ComboBox).
        /// </summary>
        public ObservableCollection<string> Roles { get; set; }

        #endregion

        #region Commands (Lệnh)

        /// <summary>
        /// Command xử lý hành động đăng nhập.
        /// </summary>
        public ICommand LoginCommand { get; set; }

        /// <summary>
        /// Command xử lý hành động thoát khỏi màn hình đăng nhập và quay về màn hình khởi đầu.
        /// </summary>
        public ICommand LoginExitCommand { get; set; }

        #endregion

        #region Constructor (Hàm tạo)

        /// <summary>
        /// Khởi tạo một thể hiện mới của <see cref="LoginViewModel"/>.
        /// </summary>
        public LoginViewModel()
        {
            // Khởi tạo danh sách các vai trò người dùng
            Roles = new ObservableCollection<string> { "Giáo vụ", "Giáo viên", "Học sinh" };

            // Khởi tạo LoginCommand với các phương thức CanLogin và Login
            LoginCommand = new RelayCommand<object>(
                (p) => CanLogin(p), // Phương thức kiểm tra điều kiện để thực thi Login
                (p) => Login(p)     // Phương thức thực thi Login
            );

            // Khởi tạo LoginExitCommand
            LoginExitCommand = new RelayCommand<object>(
                (p) => true, // Luôn cho phép thoát
                (p) => HandleLoginExit(p) // Phương thức xử lý thoát
            );
        }

        #endregion

        #region Command Methods (Phương thức xử lý lệnh)

        /// <summary>
        /// Kiểm tra điều kiện để có thể thực hiện đăng nhập.
        /// Đăng nhập được phép nếu tên đăng nhập, mật khẩu và vai trò đã chọn không rỗng.
        /// </summary>
        /// <param name="parameter">Tham số truyền vào từ Command (không sử dụng ở đây).</param>
        /// <returns>True nếu có thể đăng nhập, ngược lại là False.</returns>
        private bool CanLogin(object parameter)
        {
            return !string.IsNullOrEmpty(Username)
                && !string.IsNullOrEmpty(Password)
                && !string.IsNullOrEmpty(SelectedRole);
        }

        /// <summary>
        /// Xử lý logic đăng nhập bất đồng bộ.
        /// Kết nối đến cơ sở dữ liệu MySQL để xác thực thông tin người dùng.
        /// Nếu thông tin đúng, sẽ mở cửa sổ MainWindow và đóng cửa sổ LoginWindow hiện tại.
        /// Nếu sai, sẽ hiển thị thông báo lỗi.
        /// </summary>
        /// <param name="parameter">Tham số truyền vào từ Command (không sử dụng ở đây).</param>
        private async void Login(object parameter)
        {
            // Lấy chuỗi kết nối từ file cấu hình ứng dụng
            string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

            // Câu truy vấn SQL để lấy thông tin người dùng và các thông tin liên quan từ nhiều bảng
            // Sử dụng CTEs (Common Table Expressions) để tổ chức truy vấn rõ ràng hơn
            string query = @"
                WITH LopChuNhiem AS (
                    SELECT
                        l.GVCNID AS GiaoVienID,
                        l.LopID AS LopChuNhiemID,
                        l.TenLop AS TenLopChuNhiem
                    FROM LOP l
                    WHERE l.GVCNID IS NOT NULL
                ),
                PhanCongKhongChuNhiem AS (
                    SELECT
                        pc.GiaoVienID,
                        pc.LopID,
                        l.TenLop AS TenLopDay,
                        ROW_NUMBER() OVER (PARTITION BY pc.GiaoVienID ORDER BY pc.LopID) AS rn
                    FROM PHANCONGDAY pc
                    JOIN LOP l ON pc.LopID = l.LopID
                    LEFT JOIN LopChuNhiem lcn ON lcn.GiaoVienID = pc.GiaoVienID
                    WHERE pc.LopID != IFNULL(lcn.LopChuNhiemID, -1)
                ),
                MonDayDauTien AS (
                    SELECT
                        pc.GiaoVienID,
                        mh.TenMonHoc,
                        ROW_NUMBER() OVER (PARTITION BY pc.GiaoVienID ORDER BY pc.LopID) AS rn
                    FROM PHANCONGDAY pc
                    JOIN MONHOC mh ON mh.MonHocID = pc.MonHocID
                )
                SELECT
                    u.UserID, u.TenDangNhap, u.MatKhau,
                    vt.VaiTroID, vt.TenVaiTro,

                    hs.HocSinhID, gv.GiaoVienID, gv2.GiaoVuID,

                    hsHS.HoSoHocSinhID, gvHS.HoSoGiaoVienID, gvGV.HoSoGiaoVuID,

                    l_hs.TenLop AS TenLopHocSinh,

                    pc1.TenLopDay AS LopDay1,
                    pc2.TenLopDay AS LopDay2,
                    pc3.TenLopDay AS LopDay3,

                    lcn.TenLopChuNhiem AS LopChuNhiem,

                    md.TenMonHoc AS BoMon,

                    h.HoSoID AS MaHoSo, h.HoTen, h.GioiTinh, h.NgaySinh, h.Email, h.DiaChi,
                    h.ChucVuID, cv.TenChucVu AS ChucVuTen

                FROM USERS u
                JOIN VAITRO vt ON u.VaiTroID = vt.VaiTroID

                LEFT JOIN HOCSINH hs ON u.UserID = hs.UserID
                LEFT JOIN HOSOHOCSINH hsHS ON hs.HocSinhID = hsHS.HocSinhID
                LEFT JOIN LOP l_hs ON hsHS.LopHocID = l_hs.LopID

                LEFT JOIN GIAOVIEN gv ON u.UserID = gv.UserID
                LEFT JOIN HOSOGIAOVIEN gvHS ON gv.GiaoVienID = gvHS.GiaoVienID

                LEFT JOIN GIAOVU gv2 ON u.UserID = gv2.UserID
                LEFT JOIN HOSOGIAOVU gvGV ON gv2.GiaoVuID = gvGV.GiaoVuID

                LEFT JOIN HOSO h ON h.HoSoID IN (hsHS.HoSoID, gvHS.HoSoID, gvGV.HoSoID)
                LEFT JOIN CHUCVU cv ON h.ChucVuID = cv.ChucVuID

                LEFT JOIN LopChuNhiem lcn ON lcn.GiaoVienID = gv.GiaoVienID

                LEFT JOIN PhanCongKhongChuNhiem pc1 ON pc1.GiaoVienID = gv.GiaoVienID AND pc1.rn = 1
                LEFT JOIN PhanCongKhongChuNhiem pc2 ON pc2.GiaoVienID = gv.GiaoVienID AND pc2.rn = 2
                LEFT JOIN PhanCongKhongChuNhiem pc3 ON pc3.GiaoVienID = gv.GiaoVienID AND pc3.rn = 3

                LEFT JOIN MonDayDauTien md ON md.GiaoVienID = gv.GiaoVienID AND md.rn = 1

                WHERE u.TenDangNhap = @Username
                    AND u.MatKhau = @Password
                    AND vt.TenVaiTro = @Role;";

            // Sử dụng khối using để đảm bảo kết nối và lệnh được giải phóng đúng cách
            using (var conn = new MySqlConnection(connectionString))
            {
                try
                {
                    await conn.OpenAsync(); // Mở kết nối bất đồng bộ
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        // Thêm tham số vào câu lệnh để tránh SQL Injection
                        cmd.Parameters.AddWithValue("@Username", Username);
                        cmd.Parameters.AddWithValue("@Password", Password);
                        cmd.Parameters.AddWithValue("@Role", SelectedRole);

                        using (var reader = await cmd.ExecuteReaderAsync()) // Thực thi lệnh bất đồng bộ
                        {
                            if (reader.Read())
                            {
                                // Đọc dữ liệu từ kết quả truy vấn và tạo đối tượng User
                                var user = new User
                                {
                                    UserID = reader["UserID"].ToString(),
                                    TenDangNhap = reader["TenDangNhap"].ToString(),
                                    MatKhau = reader["MatKhau"].ToString(),
                                    VaiTro = new VaiTro
                                    {
                                        VaiTroID = reader["VaiTroID"].ToString(),
                                        TenVaiTro = reader["TenVaiTro"].ToString()
                                    },

                                    HocSinhID = reader["HocSinhID"]?.ToString(),
                                    GiaoVienID = reader["GiaoVienID"]?.ToString(),
                                    GiaoVuID = reader["GiaoVuID"]?.ToString(),

                                    MaHoSo = reader["MaHoSo"]?.ToString(),
                                    HoTen = reader["HoTen"]?.ToString(),
                                    GioiTinh = reader["GioiTinh"]?.ToString(),
                                    NgaySinh = (reader["NgaySinh"] as DateTime?)?.Date,
                                    Email = reader["Email"]?.ToString(),
                                    DiaChi = reader["DiaChi"]?.ToString(),
                                    ChucVu = reader["ChucVuTen"]?.ToString(),

                                    // Xác định MaHoSoCaNhan dựa trên TenVaiTro
                                    MaHoSoCaNhan = reader["TenVaiTro"] switch
                                    {
                                        "Học sinh" => reader["HoSoHocSinhID"]?.ToString(),
                                        "Giáo viên" => reader["HoSoGiaoVienID"]?.ToString(),
                                        "Giáo vụ" => reader["HoSoGiaoVuID"]?.ToString(),
                                        _ => null
                                    },

                                    // Lưu ý: Các trường LopHocID, LopDayID1, LopDayID2, LopDayID3, LopDayIDCN không có trong câu truy vấn của bạn.
                                    // Tôi đã thêm các trường tương ứng từ câu truy vấn (TenLopHocSinh, LopDay1, LopDay2, LopDay3, LopChuNhiem)
                                    // Bạn cần điều chỉnh tên các thuộc tính trong lớp User nếu cần.
                                    LopHocID = reader["TenLopHocSinh"]?.ToString(),
                                    LopDayID1 = reader["LopDay1"]?.ToString(),
                                    LopDayID2 = reader["LopDay2"]?.ToString(),
                                    LopDayID3 = reader["LopDay3"]?.ToString(),
                                    LopDayIDCN = reader["LopChuNhiem"]?.ToString(),
                                    BoMon = reader["BoMon"]?.ToString()
                                };

                                // Khởi tạo MainWindowViewModel với thông tin người dùng hiện tại
                                var mainVM = new MainViewModel { CurrentUser = user };
                                // Tạo MainWindow và gán DataContext
                                var mainWindow = new MainWindow { DataContext = mainVM };

                                // Hiển thị MainWindow với hiệu ứng chuyển động
                                await WindowAnimationHelper.FadeInAsync(mainWindow);

                                // Tìm cửa sổ đăng nhập hiện tại để đóng nó
                                LoginWindow loginWindow = Application.Current.Windows
                                    .OfType<LoginWindow>()
                                    .FirstOrDefault();

                                // Ẩn và đóng LoginWindow sau khi đăng nhập thành công với hiệu ứng
                                if (loginWindow != null)
                                {
                                    await WindowAnimationHelper.FadeOutAsync(loginWindow);
                                    loginWindow.Close();
                                }
                            }
                            else
                            {
                                // Hiển thị thông báo lỗi nếu thông tin đăng nhập không đúng
                                await DialogHost.Show(new ErrorDialog("Lỗi", "Sai tài khoản hoặc mật khẩu"), "RootDialog_Login");
                            }
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    // Xử lý các lỗi liên quan đến cơ sở dữ liệu
                    await DialogHost.Show(new ErrorDialog("Lỗi kết nối CSDL", $"Không thể kết nối đến cơ sở dữ liệu: {ex.Message}"), "RootDialog_Login");
                }
                catch (Exception ex)
                {
                    // Xử lý các lỗi chung khác
                    await DialogHost.Show(new ErrorDialog("Lỗi", $"Đã xảy ra lỗi: {ex.Message}"), "RootDialog_Login");
                }
            }
        }

        /// <summary>
        /// Xử lý hành động thoát khỏi màn hình đăng nhập.
        /// Mở lại <see cref="BeginWindow"/> và đóng <see cref="LoginWindow"/>.
        /// </summary>
        /// <param name="parameter">Tham số truyền vào từ Command (không sử dụng ở đây).</param>
        private async void HandleLoginExit(object parameter)
        {
            // Tìm cửa sổ LoginWindow hiện tại đang mở
            LoginWindow loginWindow = Application.Current.Windows
                .OfType<LoginWindow>()
                .FirstOrDefault();

            // Tạo và hiển thị BeginWindow
            var beginWindow = new BeginWindow();
            // Sử dụng async/await để đảm bảo hiệu ứng hoàn tất trước khi đóng cửa sổ
            await WindowAnimationHelper.FadeInAsync(beginWindow);

            // Đóng LoginWindow nếu nó tồn tại
            loginWindow?.Close();
        }

        #endregion
    }
}