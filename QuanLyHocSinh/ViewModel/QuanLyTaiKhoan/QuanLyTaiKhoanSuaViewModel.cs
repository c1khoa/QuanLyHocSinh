using MySql.Data.MySqlClient;
using QuanLyHocSinh.Model.Entities;
using QuanLyHocSinh.Service;
using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
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

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event Action<User> AccountEditedSuccessfully;
        public event Action CancelRequested;

        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;

        string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

        public QuanLyTaiKhoanSuaViewModel(User userToEdit, MainViewModel mainVM)
        {
            _mainVM = mainVM;
            _originalUser = userToEdit;
            EditedUser = userToEdit.Clone() as User;

            LoadRoles();

            SaveCommand = new RelayCommand(SaveChanges, CanSaveChanges);
            CancelCommand = new RelayCommand(CancelEdit);
        }

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
                   !string.IsNullOrWhiteSpace(EditedUser.VaiTroID); // << THÊM ĐIỀU KIỆN NÀY
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

                if (!Roles.Any(r => r.VaiTroID == EditedUser.VaiTroID))
                {
                    MessageBox.Show("Vai trò không hợp lệ!");
                    return;
                }

                using var conn = new MySqlConnection(connectionString);
                conn.Open();
                using var trans = conn.BeginTransaction();

                // 1. Lấy HoSoID từ UserID
                string getHoSoIdQuery = @"
            SELECT h.HoSoID
            FROM HOSO h
            JOIN HOSOGIAOVIEN hgv ON h.HoSoID = hgv.HoSoID
            JOIN GIAOVIEN gv ON gv.GiaoVienID = hgv.GiaoVienID
            WHERE gv.UserID = @UserID
            UNION
            SELECT h.HoSoID
            FROM HOSO h
            JOIN HOSOHOCSINH hhs ON h.HoSoID = hhs.HoSoID
            JOIN HOCSINH hs ON hs.HocSinhID = hhs.HocSinhID
            WHERE hs.UserID = @UserID
            LIMIT 1;
        ";

                string hoSoID = null;
                using (var getHoSoCmd = new MySqlCommand(getHoSoIdQuery, conn, trans))
                {
                    getHoSoCmd.Parameters.AddWithValue("@UserID", EditedUser.UserID);
                    var result = getHoSoCmd.ExecuteScalar();
                    if (result != null)
                        hoSoID = result.ToString();
                }

                if (hoSoID != null)
                {
                    // 2. Cập nhật bảng HOSO
                    string updateHoSoQuery = @"
                UPDATE HOSO 
                SET HoTen = @HoTen, NgayCapNhatGanNhat = @NgayCapNhat 
                WHERE HoSoID = @HoSoID;
            ";

                    using var cmd1 = new MySqlCommand(updateHoSoQuery, conn, trans);
                    cmd1.Parameters.AddWithValue("@HoTen", EditedUser.HoTen);
                    cmd1.Parameters.AddWithValue("@NgayCapNhat", DateTime.Now);
                    cmd1.Parameters.AddWithValue("@HoSoID", hoSoID);
                    cmd1.ExecuteNonQuery();
                }
                else
                {
                    MessageBox.Show("Không tìm thấy hồ sơ tương ứng với người dùng.");
                    trans.Rollback();
                    return;
                }

                // 3. Cập nhật bảng USERS
                string updateUserQuery = @"
            UPDATE USERS 
            SET TenDangNhap = @TenDangNhap, VaiTroID = @VaiTroID" +
                    (string.IsNullOrWhiteSpace(EditedUser.MatKhau) ? "" : ", MatKhau = @MatKhau") +
                    " WHERE UserID = @UserID;";

                using var cmd2 = new MySqlCommand(updateUserQuery, conn, trans);
                cmd2.Parameters.AddWithValue("@TenDangNhap", EditedUser.TenDangNhap);
                cmd2.Parameters.AddWithValue("@VaiTroID", EditedUser.VaiTroID);
                cmd2.Parameters.AddWithValue("@UserID", EditedUser.UserID);
                if (!string.IsNullOrWhiteSpace(EditedUser.MatKhau))
                {
                    cmd2.Parameters.AddWithValue("@MatKhau", EditedUser.MatKhau);
                }
                cmd2.ExecuteNonQuery();

                // 4. Commit transaction
                trans.Commit();
                // Cập nhật lại user trong danh sách (object gốc)
                _originalUser.HoTen = EditedUser.HoTen;
                _originalUser.TenDangNhap = EditedUser.TenDangNhap;
                _originalUser.VaiTroID = EditedUser.VaiTroID;
                _originalUser.MatKhau = EditedUser.MatKhau;
                OnPropertyChanged(nameof(_originalUser));

                MessageBox.Show("Cập nhật thành công!");
                AccountEditedSuccessfully?.Invoke(EditedUser);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật: {ex.Message}");
            }
        }





        private void CancelEdit()
        {
            CancelRequested?.Invoke();
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}
