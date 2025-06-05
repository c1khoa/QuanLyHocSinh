using QuanLyHocSinh.Model.Entities;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System;

namespace QuanLyHocSinh.Service
{
    public static class UserService
    {
        private static readonly string connectionString = "server=localhost;user=root;password=221005;database=quanlyhocsinh;";

        // Lấy danh sách tài khoản từ DB
        public static List<User> LayDanhSachTaiKhoan()
        {
            var danhSach = new List<User>();

            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                string query = @"SELECT u.UserID, u.TenDangNhap, u.MatKhau, u.VaiTroID, v.TenVaiTro
                         FROM USERS u
                         JOIN VAITRO v ON u.VaiTroID = v.VaiTroID";

                using (var cmd = new MySqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var user = new User
                        {
                            UserID = reader["UserID"]?.ToString() ?? string.Empty,
                            TenDangNhap = reader["TenDangNhap"]?.ToString() ?? string.Empty,
                            MatKhau = reader["MatKhau"]?.ToString() ?? string.Empty,
                            VaiTroID = reader["VaiTroID"]?.ToString() ?? string.Empty,
                            VaiTro = new VaiTro
                            {
                                VaiTroID = reader["VaiTroID"]?.ToString() ?? string.Empty,
                                TenVaiTro = reader["TenVaiTro"]?.ToString() ?? string.Empty
                            }
                        };
                        danhSach.Add(user);
                    }
                }
            }
            return danhSach;
        }

        public static bool ThemTaiKhoan(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrEmpty(user.TenDangNhap)) return false; // có thể check thêm
            if (CheckDuplicateUsername(user.TenDangNhap))
                return false;

            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = @"INSERT INTO USERS (UserID, TenDangNhap, MatKhau, VaiTroID) 
                         VALUES (@UserID, @TenDangNhap, @MatKhau, @VaiTroID)";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", user.UserID ?? Guid.NewGuid().ToString()); // nếu UserID có thể null thì cấp mới
                    cmd.Parameters.AddWithValue("@TenDangNhap", user.TenDangNhap);
                    cmd.Parameters.AddWithValue("@MatKhau", user.MatKhau ?? string.Empty);
                    cmd.Parameters.AddWithValue("@VaiTroID", user.VaiTroID ?? string.Empty);

                    int result = cmd.ExecuteNonQuery();
                    return result > 0;
                }
            }
        }

        public static bool SuaTaiKhoan(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (!CheckDuplicateUserID(user.UserID))
                return false;

            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = @"UPDATE USERS 
                         SET TenDangNhap = @TenDangNhap, MatKhau = @MatKhau, VaiTroID = @VaiTroID
                         WHERE UserID = @UserID";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@TenDangNhap", user.TenDangNhap);
                    cmd.Parameters.AddWithValue("@MatKhau", user.MatKhau ?? string.Empty);
                    cmd.Parameters.AddWithValue("@VaiTroID", user.VaiTroID ?? string.Empty);
                    cmd.Parameters.AddWithValue("@UserID", user.UserID);

                    int result = cmd.ExecuteNonQuery();
                    return result > 0;
                }
            }
        }

        public static bool XoaTaiKhoan(string userID)
        {
            if (string.IsNullOrEmpty(userID))
                return false;
            if (!CheckDuplicateUserID(userID))
                return false;

            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "DELETE FROM USERS WHERE UserID = @UserID";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userID);

                    int result = cmd.ExecuteNonQuery();
                    return result > 0;
                }
            }
        }

        public static bool CheckDuplicateUserID(string userID)
        {
            if (string.IsNullOrEmpty(userID))
                return false;

            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT COUNT(*) FROM USERS WHERE UserID = @UserID";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userID);
                    long count = (long)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        public static bool CheckDuplicateUsername(string username)
        {
            if (string.IsNullOrEmpty(username))
                return false;

            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT COUNT(*) FROM USERS WHERE TenDangNhap = @TenDangNhap";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@TenDangNhap", username);
                    long count = (long)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        public static string LayTenVaiTroTuDB(string vaiTroID)
        {
            if (string.IsNullOrEmpty(vaiTroID))
                return string.Empty;

            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT TenVaiTro FROM VAITRO WHERE VaiTroID = @VaiTroID";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@VaiTroID", vaiTroID);
                    var result = cmd.ExecuteScalar();
                    return result?.ToString() ?? string.Empty;
                }
            }
        }

        public static List<VaiTro> GetAllRoles()
        {
            var list = new List<VaiTro>();

            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT VaiTroID, TenVaiTro FROM VAITRO";

                using (var cmd = new MySqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new VaiTro
                        {
                            VaiTroID = reader["VaiTroID"]?.ToString() ?? string.Empty,
                            TenVaiTro = reader["TenVaiTro"]?.ToString() ?? string.Empty
                        });
                    }
                }
            }

            return list;
        }

    }
}
