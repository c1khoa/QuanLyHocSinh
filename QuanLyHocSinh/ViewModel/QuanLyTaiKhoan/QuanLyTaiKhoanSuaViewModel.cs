using QuanLyHocSinh.Model.Entities;
using QuanLyHocSinh.Service;
using QuanLyHocSinh.ViewModel;
using System;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace QuanLyHocSinh.ViewModel.QuanLyTaiKhoan
{
    public class QuanLyTaiKhoanSuaViewModel : BaseViewModel
    {
        private readonly MainViewModel _mainVM;
        private readonly User _originalUser;

        public User EditedUser { get; set; }
        public ObservableCollection<VaiTro> Roles { get; } = new ObservableCollection<VaiTro>();

        public bool CanEditRole => _mainVM.CurrentUser.VaiTroID == "GVU";
        public bool IsEditableUserID => _mainVM.CurrentUser.VaiTroID == "GVU";

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; } // Added Cancel Command

        // Events to communicate back to the main view model
        public event Action<User> AccountEditedSuccessfully;
        public event Action CancelRequested;

        // Tạm lưu mật khẩu từ UI truyền qua
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;

        public QuanLyTaiKhoanSuaViewModel(User userToEdit, MainViewModel mainVM)
        {
            _mainVM = mainVM;
            _originalUser = userToEdit;
            EditedUser = userToEdit.Clone() as User; // Ensure User class has a Clone method

            LoadRoles();
            SaveCommand = new RelayCommand(SaveChanges, CanSaveChanges);
            CancelCommand = new RelayCommand(CancelEdit); // Initialize Cancel Command

            // Kiểm tra quyền sửa
            if (!HasEditPermission())
            {
                MessageBox.Show("Bạn không có quyền sửa tài khoản này!");
                CancelRequested?.Invoke(); // Request cancellation if no permission
            }
        }

        private bool HasEditPermission()
        {
            if (_mainVM.CurrentUser.VaiTroID == "GVU") return true;
            return _mainVM.CurrentUser.UserID == EditedUser.UserID;
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
            return EditedUser != null &&
                   !string.IsNullOrWhiteSpace(EditedUser.HoTen) &&
                   !string.IsNullOrWhiteSpace(EditedUser.TenDangNhap);
        }

        private void SaveChanges()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(NewPassword))
                {
                    if (NewPassword != ConfirmPassword)
                    {
                        MessageBox.Show("Mật khẩu xác nhận không khớp!");
                        return;
                    }

                    EditedUser.MatKhau = HashPassword(NewPassword);
                }

                UserService.CapNhatTaiKhoan(EditedUser);
                MessageBox.Show("Cập nhật tài khoản thành công!");
                AccountEditedSuccessfully?.Invoke(EditedUser); // Notify success and pass the edited user
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật: {ex.Message}");
            }
        }

        private void CancelEdit()
        {
            CancelRequested?.Invoke(); // Notify cancellation
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}