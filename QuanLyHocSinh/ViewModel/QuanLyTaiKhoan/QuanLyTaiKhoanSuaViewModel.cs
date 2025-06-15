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

        // Hai biến này được gán giá trị từ code-behind do PasswordBox không hỗ trợ binding
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;

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
        private void SaveChanges()
        {
            try
            {
                // Kiểm tra mật khẩu nếu có nhập
                if (!string.IsNullOrWhiteSpace(NewPassword))
                {
                    if (NewPassword != ConfirmPassword)
                    {
                        MessageBox.Show("Mật khẩu xác nhận không khớp!");
                        return;
                    }

                    EditedUser.MatKhau = NewPassword;
                }

                if (!Roles.Any(r => r.VaiTroID == EditedUser.VaiTroID))
                {
                    MessageBox.Show("Vai trò không hợp lệ!");
                    return;
                }

                using var conn = new MySqlConnection(connectionString);
                conn.Open();
                using var trans = conn.BeginTransaction();

                try
                {
                    // Tạo lệnh cập nhật USERS tuỳ theo có đổi mật khẩu hay không
                    using var cmdUser = new MySqlCommand();
                    cmdUser.Connection = conn;
                    cmdUser.Transaction = trans;

                    cmdUser.Parameters.AddWithValue("@TenDangNhap", EditedUser.TenDangNhap);
                    cmdUser.Parameters.AddWithValue("@VaiTroID", EditedUser.VaiTroID);
                    cmdUser.Parameters.AddWithValue("@UserID", EditedUser.UserID);

                    if (!string.IsNullOrWhiteSpace(NewPassword))
                    {
                        cmdUser.CommandText = @"
                            UPDATE USERS 
                            SET TenDangNhap = @TenDangNhap, 
                                VaiTroID = @VaiTroID, 
                                MatKhau = @MatKhau
                            WHERE UserID = @UserID;";
                        cmdUser.Parameters.AddWithValue("@MatKhau", EditedUser.MatKhau);
                    }
                    else
                    {
                        cmdUser.CommandText = @"
                            UPDATE USERS 
                            SET TenDangNhap = @TenDangNhap, 
                                VaiTroID = @VaiTroID
                            WHERE UserID = @UserID;";
                    }

                    cmdUser.ExecuteNonQuery();

                    // Cập nhật HOSO nếu có (GV hoặc HS)
                    string updateHoSoQuery = @"
                        UPDATE HOSO h
                        JOIN (
                            SELECT HoSoID FROM HOSOGIAOVIEN hgv
                            JOIN GIAOVIEN gv ON gv.GiaoVienID = hgv.GiaoVienID
                            WHERE gv.UserID = @UserID

                            UNION

                            SELECT HoSoID FROM HOSOHOCSINH hhs
                            JOIN HOCSINH hs ON hs.HocSinhID = hhs.HocSinhID
                            WHERE hs.UserID = @UserID
                        ) AS sub ON h.HoSoID = sub.HoSoID
                        SET h.HoTen = @HoTen, 
                            h.NgayCapNhatGanNhat = @NgayCapNhat";

                    using var cmdHoSo = new MySqlCommand(updateHoSoQuery, conn, trans);
                    cmdHoSo.Parameters.AddWithValue("@HoTen", EditedUser.HoTen);
                    cmdHoSo.Parameters.AddWithValue("@NgayCapNhat", DateTime.Now);
                    cmdHoSo.Parameters.AddWithValue("@UserID", EditedUser.UserID);

                    int rowsAffected = cmdHoSo.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        MessageBox.Show("Cảnh báo: Đã cập nhật tài khoản nhưng không tìm thấy hồ sơ liên quan.");
                    }

                    // Commit giao dịch
                    trans.Commit();

                    // Cập nhật đối tượng gốc
                    _originalUser.HoTen = EditedUser.HoTen;
                    _originalUser.TenDangNhap = EditedUser.TenDangNhap;
                    _originalUser.VaiTroID = EditedUser.VaiTroID;
                    if (!string.IsNullOrWhiteSpace(NewPassword))
                        _originalUser.MatKhau = EditedUser.MatKhau;

                    MessageBox.Show("Cập nhật thành công!");
                    AccountEditedSuccessfully?.Invoke(EditedUser);
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    MessageBox.Show("Lỗi khi cập nhật: " + ex.Message);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối: " + ex.Message);
            }
        }

        private void CancelEdit()
        {
            CancelRequested?.Invoke();
        }
    }
}
