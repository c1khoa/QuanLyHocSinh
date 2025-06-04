using QuanLyHocSinh.Model.Entities;
using QuanLyHocSinh.Service;
using QuanLyHocSinh.ViewModel; // Thêm namespace này
using System;
using System.Windows;
using System.Windows.Input;

namespace QuanLyHocSinh.ViewModel.QuanLyTaiKhoan
{
  
    public class QuanLyTaiKhoanThemViewModel : BaseViewModel
    {
        private string? _userID;
        private string? _hoTen;
        private string? _tenDangNhap;
        private string? _matKhau;
        private string? _vaiTroID;
        private readonly MainViewModel _mainVM;
        public string? UserID
        {
            get => _userID;
            set { _userID = value; OnPropertyChanged(); }
        }

        public string? HoTen
        {
            get => _hoTen;
            set { _hoTen = value; OnPropertyChanged(); }
        }

        public string TenDangNhap
        {
            get => _tenDangNhap ?? string.Empty;
            set { _tenDangNhap = value; OnPropertyChanged(); }
        }

        public string MatKhau
        {
            get => _matKhau ?? string.Empty;
            set { _matKhau = value; OnPropertyChanged(); }
        }

        public string VaiTroID
        {
            get => _vaiTroID ?? string.Empty;
            set { _vaiTroID = value; OnPropertyChanged(); }
        }

        public ICommand AddAccountCommand { get; }
        public ICommand CancelCommand { get; }

        public event Action<User>? AccountAddedSuccessfully;
        public event Action? CancelRequested;

        public QuanLyTaiKhoanThemViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM; // Khởi tạo mainVM
            UserID = Guid.NewGuid().ToString(); // Tạo UserID mặc định

            // Sử dụng RelayCommand không generic
            AddAccountCommand = new RelayCommand(ExecuteAddAccount);
            CancelCommand = new RelayCommand(ExecuteCancel);
        }

        private void ExecuteAddAccount()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(HoTen) ||
                    string.IsNullOrWhiteSpace(TenDangNhap) ||
                    string.IsNullOrWhiteSpace(MatKhau) ||
                    string.IsNullOrWhiteSpace(VaiTroID))
                {
                    MessageBox.Show("Vui lòng nhập đầy đủ thông tin bắt buộc (Họ tên, Tên đăng nhập, Mật khẩu và Vai trò)");
                    return;
                }

                var userMoi = new User
                {
                    UserID = string.IsNullOrWhiteSpace(UserID) ? Guid.NewGuid().ToString() : UserID!,
                    HoTen = this.HoTen,
                    TenDangNhap = this.TenDangNhap,
                    MatKhau = this.MatKhau,
                    VaiTroID = this.VaiTroID
                };

                var result = UserService.ThemTaiKhoan(userMoi);
                if (result)
                {
                    MessageBox.Show("Thêm tài khoản thành công!");
                    AccountAddedSuccessfully?.Invoke(userMoi);
                }
                else
                {
                    MessageBox.Show("Thêm tài khoản thất bại. Tên đăng nhập có thể đã tồn tại.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi thêm tài khoản: {ex.Message}");
            }
        }

        private void ExecuteCancel()
        {
            // Sửa: Quay lại view trước đó trong mainVM
            _mainVM.NavigateBack();
        }
    }
}
