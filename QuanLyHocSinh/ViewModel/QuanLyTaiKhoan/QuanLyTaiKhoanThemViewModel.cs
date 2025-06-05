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
        public string _userID = null!;
        public string _hoTen = null!;
        public string _tenDangNhap = null!;
        public string _matKhau = null!;
        public string _vaiTroID = null!;
        public readonly MainViewModel _mainVM;

        public ObservableCollection<VaiTro> Roles { get; } = new ObservableCollection<VaiTro>();

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

        public ICommand AddAccountCommand { get; }
        public ICommand CancelCommand { get; }

        public event Action<User>? AccountAddedSuccessfully;
        public event Action? CancelRequested;

        // Updated constructor with optional parameter
        public QuanLyTaiKhoanThemViewModel(MainViewModel? mainVM = null)
        {
            _mainVM = mainVM ?? throw new ArgumentNullException(nameof(mainVM));
            LoadRoles();
            AddAccountCommand = new RelayCommand(ExecuteAddAccount, CanExecuteAddAccount);
            CancelCommand = new RelayCommand(ExecuteCancel);
        }

        private void LoadRoles()
        {
            try
            {
                // Replace with actual role loading logic
                Roles.Clear();

                // Sample roles - replace with database call
                Roles.Add(new VaiTro { VaiTroID = "ADMIN", TenVaiTro = "Quản trị viên" });
                Roles.Add(new VaiTro { VaiTroID = "GV", TenVaiTro = "Giáo viên" });
                Roles.Add(new VaiTro { VaiTroID = "NV", TenVaiTro = "Nhân viên" });

                if (Roles.Any())
                {
                    VaiTroID = Roles.First().VaiTroID;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách vai trò: {ex.Message}");
            }
        }

        public bool CanExecuteAddAccount()
        {
            return !string.IsNullOrWhiteSpace(UserID) &&
                   !string.IsNullOrWhiteSpace(HoTen) &&
                   !string.IsNullOrWhiteSpace(TenDangNhap) &&
                   !string.IsNullOrWhiteSpace(MatKhau) &&
                   !string.IsNullOrWhiteSpace(VaiTroID);
        }

        public void ExecuteAddAccount()
        {
            try
            {
                if (UserService.CheckDuplicateUserID(UserID))
                {
                    MessageBox.Show("Mã người dùng đã tồn tại! Vui lòng nhập mã khác.");
                    return;
                }

                if (UserService.CheckDuplicateUsername(TenDangNhap))
                {
                    MessageBox.Show("Tên đăng nhập đã tồn tại! Vui lòng chọn tên khác.");
                    return;
                }

                var hashedPassword = HashPassword(MatKhau);

                var userMoi = new User
                {
                    UserID = UserID,
                    HoTen = HoTen,
                    TenDangNhap = TenDangNhap,
                    MatKhau = hashedPassword,
                    VaiTroID = VaiTroID
                };

                var result = UserService.ThemTaiKhoan(userMoi);
                if (result)
                {
                    MessageBox.Show("Thêm tài khoản thành công!");
                    AccountAddedSuccessfully?.Invoke(userMoi);
                }
                else
                {
                    MessageBox.Show("Thêm tài khoản thất bại. Vui lòng thử lại.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi thêm tài khoản: {ex.Message}");
            }
        }

        public void ExecuteCancel()
        {

            CancelRequested?.Invoke();
        }

        public string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}