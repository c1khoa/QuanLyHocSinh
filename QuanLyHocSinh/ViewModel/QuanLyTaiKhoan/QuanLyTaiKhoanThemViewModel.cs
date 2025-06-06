using QuanLyHocSinh.Model.Entities;
using QuanLyHocSinh.Service;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace QuanLyHocSinh.ViewModel.QuanLyTaiKhoan
{
    public class QuanLyTaiKhoanThemViewModel : BaseViewModel
    {
        // Các trường lưu thông tin người dùng cần nhập
        private string _userID = string.Empty;
        private string _hoTen = string.Empty;
        private string _tenDangNhap = string.Empty;
        private string _matKhau = string.Empty;
        private string _vaiTroID = string.Empty;

        // ViewModel chính, để thao tác giao tiếp ngược khi cần
        private readonly MainViewModel _mainVM;

        // Danh sách vai trò hiển thị trong combobox
        public ObservableCollection<VaiTro> Roles { get; } = new ObservableCollection<VaiTro>();

        // Các thuộc tính được binding từ View (TextBox, ComboBox...)
        public string UserID
        {
            get => _userID;
            set { _userID = value; OnPropertyChanged(); }
        }

        public string HoTen
        {
            get => _hoTen;
            set { _hoTen = value; OnPropertyChanged(); }
        }

        public string TenDangNhap
        {
            get => _tenDangNhap;
            set { _tenDangNhap = value; OnPropertyChanged(); }
        }

        public string MatKhau
        {
            get => _matKhau;
            set { _matKhau = value; OnPropertyChanged(); }
        }

        public string VaiTroID
        {
            get => _vaiTroID;
            set { _vaiTroID = value; OnPropertyChanged(); }
        }

        // Các command xử lý sự kiện từ View
        public ICommand AddAccountCommand { get; }
        public ICommand CancelCommand { get; }

        // Sự kiện được bắn ra để View xử lý sau khi thêm tài khoản thành công hoặc huỷ
        public event Action<User>? AccountAddedSuccessfully;
        public event Action? CancelRequested;

        // Constructor, inject MainViewModel (có thể null trong test hoặc standalone view)
        public QuanLyTaiKhoanThemViewModel(MainViewModel? mainVM = null)
        {
            _mainVM = mainVM ?? throw new ArgumentNullException(nameof(mainVM));

            // Tải danh sách vai trò từ CSDL hoặc mẫu cứng
            LoadRoles();

            // Khởi tạo command với logic ràng buộc
            AddAccountCommand = new RelayCommand(ExecuteAddAccount, CanExecuteAddAccount);
            CancelCommand = new RelayCommand(ExecuteCancel);
        }

        /// <summary>
        /// Tải danh sách vai trò để chọn từ dropdown
        /// </summary>
        private void LoadRoles()
        {
            try
            {
                Roles.Clear();

                // TODO: Thay thế đoạn này bằng gọi dữ liệu từ database (vai trò thật)
                Roles.Add(new VaiTro { VaiTroID = "ADMIN", TenVaiTro = "Quản trị viên" });
                Roles.Add(new VaiTro { VaiTroID = "GV", TenVaiTro = "Giáo viên" });
                Roles.Add(new VaiTro { VaiTroID = "NV", TenVaiTro = "Nhân viên" });

                // Mặc định chọn vai trò đầu tiên nếu có
                if (Roles.Any())
                    VaiTroID = Roles.First().VaiTroID;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách vai trò: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Kiểm tra xem thông tin nhập đã đủ để cho phép thêm tài khoản chưa
        /// </summary>
        public bool CanExecuteAddAccount()
        {
            return !string.IsNullOrWhiteSpace(UserID) &&
                   !string.IsNullOrWhiteSpace(HoTen) &&
                   !string.IsNullOrWhiteSpace(TenDangNhap) &&
                   !string.IsNullOrWhiteSpace(MatKhau) &&
                   !string.IsNullOrWhiteSpace(VaiTroID);
        }

        /// <summary>
        /// Xử lý khi người dùng bấm nút "Thêm tài khoản"
        /// </summary>
        public void ExecuteAddAccount()
        {
            try
            {
                // Kiểm tra trùng mã người dùng
                if (UserService.CheckDuplicateUserID(UserID))
                {
                    MessageBox.Show("Mã người dùng đã tồn tại! Vui lòng nhập mã khác.", "Trùng mã", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Kiểm tra trùng tên đăng nhập
                if (UserService.CheckDuplicateUsername(TenDangNhap))
                {
                    MessageBox.Show("Tên đăng nhập đã tồn tại! Vui lòng chọn tên khác.", "Trùng tên", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Mã hoá mật khẩu trước khi lưu
                var hashedPassword = HashPassword(MatKhau);

                // Tạo đối tượng người dùng mới
                var userMoi = new User
                {
                    UserID = UserID,
                    HoTen = HoTen,
                    TenDangNhap = TenDangNhap,
                    MatKhau = hashedPassword,
                    VaiTroID = VaiTroID
                };

                // Gọi service để thêm tài khoản vào DB
                var result = UserService.ThemTaiKhoan(userMoi);

                if (result)
                {
                    MessageBox.Show("Thêm tài khoản thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Gửi sự kiện thông báo thành công
                    AccountAddedSuccessfully?.Invoke(userMoi);
                }
                else
                {
                    MessageBox.Show("Thêm tài khoản thất bại. Vui lòng thử lại.", "Thất bại", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi thêm tài khoản: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Huỷ thao tác thêm tài khoản
        /// </summary>
        public void ExecuteCancel()
        {
            CancelRequested?.Invoke();
        }

        /// <summary>
        /// Mã hoá mật khẩu bằng SHA-256
        /// </summary>
        public string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}
