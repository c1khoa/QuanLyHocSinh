using MySql.Data.MySqlClient;
using QuanLyHocSinh.Model.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace QuanLyHocSinh.ViewModel.QuanLyTaiKhoan
{
    public class DoiMatKhauViewModel : BaseViewModel
    {
        public User CurrentUser { get; set; }
        private string _oldPassword;
        public string OldPassword
        {
            get => _oldPassword;
            set
            {
                _oldPassword = value;
                OnPropertyChanged(nameof(OldPassword));
                ValidateFields();
            }
        }

        private string _newPassword;
        public string NewPassword
        {
            get => _newPassword;
            set
            {
                _newPassword = value;
                OnPropertyChanged(nameof(NewPassword));
                ValidateFields();
            }
        }

        private string _confirmPassword;
        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                _confirmPassword = value;
                OnPropertyChanged(nameof(ConfirmPassword));
                ValidateFields();
            }
        }


        private bool _isOldPasswordVisible;
        public bool IsOldPasswordVisible
        {
            get => _isOldPasswordVisible;
            set => SetProperty(ref _isOldPasswordVisible, value);
        }
        private bool _isPasswordVisible;
        public bool IsPasswordVisible
        {
            get => _isPasswordVisible;
            set => SetProperty(ref _isPasswordVisible, value);
        }
        private bool _isConfirmPasswordVisible;
        public bool IsConfirmPasswordVisible
        {
            get => _isConfirmPasswordVisible;
            set => SetProperty(ref _isConfirmPasswordVisible, value);
        }
        private bool _canSave;
        public bool CanSave
        {
            get => _canSave;
            set => SetProperty(ref _canSave, value);
        }

        public ICommand SaveCommand { get; set; }
        public ICommand CancelCommand { get; set; }
        private readonly MainViewModel _mainVM;
        private readonly User _originalUser;
        public DoiMatKhauViewModel(User currentUser)
        {
            CurrentUser = currentUser;
            SaveCommand = new RelayCommand<object>(Save);
            CancelCommand = new RelayCommand<object>(Cancel);
        }
        private void ValidateFields()
        {
            CanSave = !string.IsNullOrWhiteSpace(OldPassword)
                   && !string.IsNullOrWhiteSpace(NewPassword)
                   && !string.IsNullOrWhiteSpace(ConfirmPassword);
        }
        private async Task ShowErrorDialog(string title, string message)
        {
            var dialog = new View.Dialogs.MessageBox.ErrorDialog(title, message);
            await MaterialDesignThemes.Wpf.DialogHost.Show(dialog, "RootDialog_ChangePass");
        }
        private async Task ShowNotifyDialog(string title, string message)
        {
            var dialog = new View.Dialogs.MessageBox.NotifyDialog(title, message);
            await MaterialDesignThemes.Wpf.DialogHost.Show(dialog, "RootDialog_ChangePass");
        }
        private void Cancel(object obj)
        {
            CloseDialog();
        }


        private async void Save(object obj)
        {
            if (string.IsNullOrEmpty(OldPassword) || string.IsNullOrEmpty(NewPassword) || string.IsNullOrEmpty(ConfirmPassword))
            {
                await ShowErrorDialog("Lỗi", "Vui lòng nhập đầy đủ thông tin.");
                return;
            }

            if (NewPassword.Length < 5)
            {
                await ShowErrorDialog("Lỗi", "Mật khẩu mới phải có ít nhất 5 ký tự.");
                return;
            }

            if (NewPassword == CurrentUser.TenDangNhap)
            {
                await ShowErrorDialog("Lỗi", "Mật khẩu không được trùng với tên đăng nhập.");
                return;
            }

            string hashedOldPassword = OldPassword;
            if (CurrentUser.MatKhau != hashedOldPassword)
            {
                await ShowErrorDialog("Lỗi", "Mật khẩu cũ không đúng.");
                return;
            }

            if (NewPassword != ConfirmPassword)
            {
                await ShowErrorDialog("Lỗi", "Mật khẩu xác nhận không khớp.");
                return;
            }


            string hashedNewPassword = NewPassword; // HashPassword(NewPassword); // Uncomment if you want to hash the new password
            try
            {
                string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "UPDATE USERS SET MatKhau = @NewPassword WHERE UserID = @UserID";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@NewPassword", hashedNewPassword);
                        cmd.Parameters.AddWithValue("@UserID", CurrentUser.UserID);
                        cmd.ExecuteNonQuery();
                    }
                }

                await ShowNotifyDialog("Thông báo", "Đổi mật khẩu thành công!");
                CloseDialog();
            }
            catch (Exception ex)
            {
                await ShowErrorDialog("Lỗi", "Lỗi khi đổi mật khẩu: " + ex.Message);
            }
        }
        private void CloseDialog()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window is Window && window.Title == "Đổi mật khẩu")
                {
                    window.Close();
                    break;
                }
            }
        }

    }

}
