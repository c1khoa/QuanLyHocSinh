using QuanLyHocSinh.Model.Entities;
using QuanLyHocSinh.Service;
using QuanLyHocSinh.ViewModel;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace QuanLyHocSinh.ViewModel.QuanLyTaiKhoan
{
    class QuanLyTaiKhoanCaNhanViewModel : BaseViewModel
    {
        private MainViewModel _mainVM;
        private User _currentUser;
        private string _newPassword;
        private string _confirmPassword;

        public User CurrentUser
        {
            get => _currentUser;
            set
            {
                _currentUser = value;
                OnPropertyChanged();
            }
        }

        public string NewPassword
        {
            get => _newPassword;
            set
            {
                _newPassword = value;
                OnPropertyChanged();
            }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                _confirmPassword = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<VaiTro> Roles { get; } = new ObservableCollection<VaiTro>();
        public ICommand SaveCommand { get; }

        public QuanLyTaiKhoanCaNhanViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM;
            CurrentUser = mainVM.CurrentUser?.Clone() as User;

            LoadRoles();
            SaveCommand = new RelayCommand(SaveChanges, CanSaveChanges);
        }

        private void LoadRoles()
        {
            Roles.Clear();
            var roles = UserService.GetAllRoles();
            foreach (var role in roles)
            {
                Roles.Add(role);
            }
        }

        private bool CanSaveChanges()
        {
            return CurrentUser != null &&
                   !string.IsNullOrWhiteSpace(CurrentUser.HoTen) &&
                   (string.IsNullOrEmpty(NewPassword) || NewPassword == ConfirmPassword);
        }

        private void SaveChanges()
        {
            try
            {
                // Cập nhật thông tin cơ bản
                UserService.CapNhatThongTinCaNhan(CurrentUser);

                // Cập nhật mật khẩu nếu có
                if (!string.IsNullOrWhiteSpace(NewPassword))
                {
                    UserService.DoiMatKhau(
                        CurrentUser.UserID,
                        HashPassword(NewPassword)
                    );
                }

                MessageBox.Show("Cập nhật thông tin thành công!");
                _mainVM.CurrentUser = CurrentUser; // Cập nhật thông tin người dùng hiện tại
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật: {ex.Message}");
            }
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}