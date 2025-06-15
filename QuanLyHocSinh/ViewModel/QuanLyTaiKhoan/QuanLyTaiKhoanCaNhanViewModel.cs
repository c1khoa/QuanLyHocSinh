using QuanLyHocSinh.View.Controls.QuanLyTaiKhoan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using QuanLyHocSinh.Model.Entities;
using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace QuanLyHocSinh.ViewModel.QuanLyTaiKhoan
{
    public class QuanLyTaiKhoanCaNhanViewModel : BaseViewModel
    {
        private readonly MainViewModel _mainVM;
        private User _currentUser;
        private string _newPassword;
        private string _confirmPassword;

        public HoSo HoSoCaNhan { get; set; } = new();

        public User CurrentUser
        {
            get => _currentUser;
            set { _currentUser = value; OnPropertyChanged(); }
        }

        public string NewPassword
        {
            get => _newPassword;
            set { _newPassword = value; OnPropertyChanged(); }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set { _confirmPassword = value; OnPropertyChanged(); }
        }

        public ICommand SaveCommand { get; }

        private readonly string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

        public QuanLyTaiKhoanCaNhanViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM;
            CurrentUser = mainVM.CurrentUser?.Clone() as User;

            SaveCommand = new RelayCommand(SaveChanges, CanSaveChanges);
            LoadHoSoCaNhan();
        }

        private void LoadHoSoCaNhan()
        {
            if (CurrentUser == null) return;

            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            string query = @"
                SELECT h.*
                FROM HOSO h
                JOIN (
                    SELECT hgv.HoSoID
                    FROM GIAOVIEN gv
                    JOIN HOSOGIAOVIEN hgv ON gv.GiaoVienID = hgv.GiaoVienID
                    WHERE gv.UserID = @UserID
                    UNION
                    SELECT hhs.HoSoID
                    FROM HOCSINH hs
                    JOIN HOSOHOCSINH hhs ON hs.HocSinhID = hhs.HocSinhID
                    WHERE hs.UserID = @UserID
                ) AS sub ON h.HoSoID = sub.HoSoID
                LIMIT 1;";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@UserID", CurrentUser.UserID);
            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                HoSoCaNhan.HoSoID = reader["HoSoID"]?.ToString();
                HoSoCaNhan.HoTen = reader["HoTen"]?.ToString();
                HoSoCaNhan.GioiTinh = reader["GioiTinh"]?.ToString();
                HoSoCaNhan.NgaySinh = reader.GetDateTime("NgaySinh");
                HoSoCaNhan.Email = reader["Email"]?.ToString();
                HoSoCaNhan.DiaChi = reader["DiaChi"]?.ToString();
                HoSoCaNhan.ChucVuID = reader["ChucVuID"]?.ToString();
                HoSoCaNhan.TrangThaiHoSo = reader["TrangThaiHoSo"]?.ToString();
            }
        }

        private bool CanSaveChanges()
        {
            return CurrentUser != null &&
                   !string.IsNullOrWhiteSpace(HoSoCaNhan.HoTen) &&
                   (string.IsNullOrWhiteSpace(NewPassword) || NewPassword == ConfirmPassword);
        }

        private void SaveChanges()
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();
            using var trans = conn.BeginTransaction();

            try
            {
                // ✅ Cập nhật HOSO bằng JOIN như ViewModel sửa chung
                string updateHoSo = @"
            UPDATE HOSO h
            JOIN (
                SELECT hgv.HoSoID
                FROM GIAOVIEN gv
                JOIN HOSOGIAOVIEN hgv ON gv.GiaoVienID = hgv.GiaoVienID
                WHERE gv.UserID = @UserID

                UNION

                SELECT hhs.HoSoID
                FROM HOCSINH hs
                JOIN HOSOHOCSINH hhs ON hs.HocSinhID = hhs.HocSinhID
                WHERE hs.UserID = @UserID
            ) AS sub ON h.HoSoID = sub.HoSoID
            SET h.HoTen = @HoTen,
                h.GioiTinh = @GioiTinh,
                h.NgaySinh = @NgaySinh,
                h.Email = @Email,
                h.DiaChi = @DiaChi,
                h.NgayCapNhatGanNhat = NOW();";

                using (var cmd = new MySqlCommand(updateHoSo, conn, trans))
                {
                    cmd.Parameters.AddWithValue("@UserID", CurrentUser.UserID);
                    cmd.Parameters.AddWithValue("@HoTen", HoSoCaNhan.HoTen);
                    cmd.Parameters.AddWithValue("@GioiTinh", HoSoCaNhan.GioiTinh);
                    cmd.Parameters.AddWithValue("@NgaySinh", HoSoCaNhan.NgaySinh);
                    cmd.Parameters.AddWithValue("@Email", HoSoCaNhan.Email);
                    cmd.Parameters.AddWithValue("@DiaChi", HoSoCaNhan.DiaChi);
                    cmd.ExecuteNonQuery();
                }

                // ✅ Cập nhật mật khẩu nếu có nhập
                if (!string.IsNullOrWhiteSpace(NewPassword))
                {
                    string updatePass = "UPDATE USERS SET MatKhau = @MatKhau WHERE UserID = @UserID";
                    using (var cmd = new MySqlCommand(updatePass, conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@MatKhau", NewPassword); // hoặc HashPassword(NewPassword) nếu bạn muốn mã hóa
                        cmd.Parameters.AddWithValue("@UserID", CurrentUser.UserID);
                        cmd.ExecuteNonQuery();
                    }
                }

                trans.Commit();
                MessageBox.Show("Cập nhật thành công!");

                _mainVM.CurrentUser = CurrentUser;
            }
            catch (Exception ex)
            {
                trans.Rollback();
                MessageBox.Show($"Lỗi khi cập nhật: {ex.Message}");
            }
        }
    }
}
