using MySql.Data.MySqlClient;
using QuanLyHocSinh.Model.Entities;
using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Threading.Tasks;
using System.Windows.Input;
using QuanLyHocSinh.Service;
using MaterialDesignThemes.Wpf;
using QuanLyHocSinh.View.Dialogs.MessageBox;

namespace QuanLyHocSinh.ViewModel.QuanLyTaiKhoan
{
    public class QuanLyTaiKhoanSuaViewModel : BaseViewModel
    {
        private string _newPassword;
        public string NewPassword
        {
            get => _newPassword;
            set => SetProperty(ref _newPassword, value);
        }

        private string _confirmPassword;
        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => SetProperty(ref _confirmPassword, value);
        }

        private bool _isPasswordVisible;
        public bool IsPasswordVisible
        {
            get => _isPasswordVisible;
            set => SetProperty(ref _isPasswordVisible, value);
        }
        private readonly MainViewModel _mainVM;
        private readonly User _originalUser;

        public User EditedUser { get; set; }
        public ObservableCollection<VaiTro> Roles { get; } = new ObservableCollection<VaiTro>();

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event Action<User> AccountEditedSuccessfully;
        public event Action CancelRequested;

        // Hai biến này được gán giá trị từ code-behind do PasswordBox không hỗ trợ binding

        string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

        public QuanLyTaiKhoanSuaViewModel(User userToEdit, MainViewModel mainVM)
        {
            _mainVM = mainVM;
            _originalUser = userToEdit;
            EditedUser = userToEdit.Clone() as User; // Tạo bản sao để chỉnh sửa

            LoadRoles();

            SaveCommand = new RelayCommand(SaveChanges, CanSaveChanges);
            CancelCommand = new RelayCommand(CancelEdit);
        }

        // Load danh sách vai trò từ database
        private void LoadRoles()
        {
            try
            {
                using var conn = new MySqlConnection(connectionString);
                conn.Open();
                var query = "SELECT VaiTroID, TenVaiTro FROM VAITRO";
                using var cmd = new MySqlCommand(query, conn);
                using var reader = cmd.ExecuteReader();
                Roles.Clear();

                while (reader.Read())
                {
                    Roles.Add(new VaiTro
                    {
                        VaiTroID = reader["VaiTroID"].ToString(),
                        TenVaiTro = reader["TenVaiTro"].ToString()
                    });
                }

                // Đảm bảo có VaiTro hợp lệ
                if (string.IsNullOrEmpty(EditedUser.VaiTroID) || !Roles.Any(r => r.VaiTroID == EditedUser.VaiTroID))
                    EditedUser.VaiTroID = Roles.FirstOrDefault()?.VaiTroID;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải vai trò: " + ex.Message);
            }
        }

        private bool CanSaveChanges()
        {
            return EditedUser != null &&
                   !string.IsNullOrWhiteSpace(EditedUser.HoTen) &&
                   !string.IsNullOrWhiteSpace(EditedUser.TenDangNhap) &&
                   !string.IsNullOrWhiteSpace(EditedUser.VaiTroID);
        }

        // Lưu thay đổi tài khoản
        private async void SaveChanges()
        {
            try
            {
                // Kiểm tra mật khẩu nếu có nhập
                if (!string.IsNullOrWhiteSpace(NewPassword))
                {
                    if (NewPassword != ConfirmPassword)
                    {
                        await DialogHost.Show(new ErrorDialog("Lỗi", "Mật khẩu xác nhận không khớp!"), "RootDialog_SuaAccount");
                        return;
                    }

                    EditedUser.MatKhau = NewPassword;
                }

                // Kết nối DB và xử lý transaction
                using var conn = new MySqlConnection(connectionString);
                conn.Open();
                using var trans = conn.BeginTransaction();

                try
                {
                    // Gọi hàm xử lý chính trong UserService
                    UserService.UpdateUserAndHoSo(EditedUser, NewPassword, conn, trans);

                    // Commit nếu thành công
                    trans.Commit();

                    // Cập nhật lại bản gốc
                    _originalUser.HoTen = EditedUser.HoTen;
                    _originalUser.TenDangNhap = EditedUser.TenDangNhap;
                    _originalUser.VaiTroID = EditedUser.VaiTroID;
                    if (!string.IsNullOrWhiteSpace(NewPassword))
                        _originalUser.MatKhau = EditedUser.MatKhau;

                    await DialogHost.Show(new NotifyDialog("Thành công", "Đã cập nhật!"), "RootDialog_SuaAccount");
                    AccountEditedSuccessfully?.Invoke(EditedUser);
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    await DialogHost.Show(new ErrorDialog("Lỗi" ,"Cập nhật không thành công: " + GetFullExceptionMessage(ex)), "RootDialog_SuaAccount");
                }
            }
            catch (Exception ex)
            {
                await DialogHost.Show(new ErrorDialog("Lỗi", "Cập nhật không thành công: " + GetFullExceptionMessage(ex)), "RootDialog_SuaAccount");
            }
        }
        private string GetFullExceptionMessage(Exception ex)
        {
            var sb = new StringBuilder();
            while (ex != null)
            {
                sb.AppendLine(ex.Message);
                ex = ex.InnerException;
            }
            return sb.ToString();
        }


        private void CancelEdit()
        {
            CancelRequested?.Invoke();
        }
    }
}
