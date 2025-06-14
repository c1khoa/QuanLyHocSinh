// ✅ FILE: QuanLyTaiKhoanThemViewModel.cs
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
        private string _userID = string.Empty;
        private string _hoTen = string.Empty;
        private string _tenDangNhap = string.Empty;
        private string _matKhau = string.Empty;
        private string _vaiTroID = string.Empty;

        private readonly MainViewModel _mainVM;

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
                Roles.Clear();
                var rolesList = UserService.GetAllRoles();
                foreach (var role in rolesList)
                {
                    Roles.Add(role);
                }

                if (Roles.Any())
                    VaiTroID = Roles.First().VaiTroID;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách vai trò: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    MessageBox.Show("Mã người dùng đã tồn tại! Vui lòng nhập mã khác.", "Trùng mã", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (UserService.CheckDuplicateUsername(TenDangNhap))
                {
                    MessageBox.Show("Tên đăng nhập đã tồn tại! Vui lòng chọn tên khác.", "Trùng tên", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var userMoi = new User
                {
                    UserID = UserID,
                    HoTen = HoTen,
                    TenDangNhap = TenDangNhap,
                    MatKhau = MatKhau,
                    VaiTroID = VaiTroID
                };

                var result = UserService.ThemTaiKhoan(userMoi);

                if (result)
                {
                    MessageBox.Show("Thêm tài khoản thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    AccountAddedSuccessfully?.Invoke(userMoi);
                    ClearForm();
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

        public void ExecuteCancel()
        {
            CancelRequested?.Invoke();
        }

        private void ClearForm()
        {
            UserID = string.Empty;
            HoTen = string.Empty;
            TenDangNhap = string.Empty;
            MatKhau = string.Empty;
            VaiTroID = Roles.Any() ? Roles.First().VaiTroID : string.Empty;
        }

        public string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}
