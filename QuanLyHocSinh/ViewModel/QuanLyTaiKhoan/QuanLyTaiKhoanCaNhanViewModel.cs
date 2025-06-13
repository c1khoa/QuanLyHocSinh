using QuanLyHocSinh.Model.Entities;
using QuanLyHocSinh.Service;
using QuanLyHocSinh.ViewModel;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls; 



namespace QuanLyHocSinh.ViewModel.QuanLyTaiKhoan
{
    class QuanLyTaiKhoanCaNhanViewModel : BaseViewModel
    {
        private MainViewModel _mainVM;
        private User _currentUser;

        public User CurrentUser
        {
            get => _currentUser;
            set
            {
                _currentUser = value;
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
                   !string.IsNullOrWhiteSpace(CurrentUser.HoTen);
        }

        private PasswordBox _pwdNewPassword;
        private PasswordBox _pwdConfirmPassword;

        public PasswordBox PwdNewPassword
        {
            get => _pwdNewPassword;
            set
            {
                _pwdNewPassword = value;
                OnPropertyChanged();
            }
        }

        public PasswordBox PwdConfirmPassword
        {
            get => _pwdConfirmPassword;
            set
            {
                _pwdConfirmPassword = value;
                OnPropertyChanged();
            }
        }

        private void SaveChanges()
        {
            try
            {
                // Cập nhật thông tin cơ bản
                UserService.CapNhatThongTinCaNhan(CurrentUser);

                // Cập nhật mật khẩu nếu có
                if (!string.IsNullOrWhiteSpace(PwdNewPassword.Password))
                {
                    if (PwdNewPassword.Password != PwdConfirmPassword.Password)
                    {
                        MessageBox.Show("Mật khẩu xác nhận không khớp!");
                        return;
                    }

                    UserService.DoiMatKhau(
                        CurrentUser.UserID,
                        HashPassword(PwdNewPassword.Password)
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


        

