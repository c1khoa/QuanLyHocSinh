using QuanLyHocSinh.Model.Entities;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System;
using System.Configuration;

namespace QuanLyHocSinh.Service
{
    public static class UserService
    {
        public static string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

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
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Thêm vào bảng USERS
                        string userQuery = @"INSERT INTO USERS (UserID, TenDangNhap, MatKhau, VaiTroID) 
                                 VALUES (@UserID, @TenDangNhap, @MatKhau, @VaiTroID)";

                        using (var cmd = new MySqlCommand(userQuery, conn, transaction))
                        {
                            string userID = user.UserID ?? Guid.NewGuid().ToString();
                            cmd.Parameters.AddWithValue("@UserID", userID);
                            cmd.Parameters.AddWithValue("@TenDangNhap", user.TenDangNhap);
                            cmd.Parameters.AddWithValue("@MatKhau", user.MatKhau ?? string.Empty);
                            cmd.Parameters.AddWithValue("@VaiTroID", user.VaiTroID ?? string.Empty);

                            int result = cmd.ExecuteNonQuery();
                            if (result <= 0)
                            {
                                transaction.Rollback();
                                return false;
                            }

                            // Thêm vào bảng tương ứng dựa vào vai trò
                            if (!string.IsNullOrEmpty(user.VaiTroID))
                            {
                                string roleQuery = "";
                                string roleID = "";

                                switch (user.VaiTroID.ToUpper())
                                {
                                    case "VT03": // Giáo vụ
                                        roleQuery = "INSERT INTO GIAOVU (GiaoVuID, UserID) VALUES (@RoleID, @UserID)";
                                        roleID = "GVU" + userID.Substring(1); // GVU001 từ U001
                                        break;
                                    case "VT02": // Giáo viên 
                                        roleQuery = "INSERT INTO GIAOVIEN (GiaoVienID, UserID) VALUES (@RoleID, @UserID)";
                                        roleID = "GV" + userID.Substring(1); // GV001 từ U001
                                        break;
                                    case "VT01": // Học sinh
                                        roleQuery = "INSERT INTO HOCSINH (HocSinhID, UserID) VALUES (@RoleID, @UserID)";
                                        roleID = "HS" + userID.Substring(1); // HS001 từ U001
                                        break;
                                }

                                if (!string.IsNullOrEmpty(roleQuery))
                                {
                                    using (var roleCmd = new MySqlCommand(roleQuery, conn, transaction))
                                    {
                                        roleCmd.Parameters.AddWithValue("@RoleID", roleID);
                                        roleCmd.Parameters.AddWithValue("@UserID", userID);
                                        roleCmd.ExecuteNonQuery();
                                    }
                                }
                            }
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        return false;
                    }
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
        public static void CapNhatThongTinCaNhan(User user)
        {
            using (var db = new QuanLyHocSinh.Model.Entities.QLHocSinhEntities()) // Fully qualify the type name
            {
                var existing = db.Users.Find(user.UserID);
                if (existing != null)
                {
                    existing.HoTen = user.HoTen;
                    db.SaveChanges();
                }
            }
        }

        public static void DoiMatKhau(string userId, string newPassword)
        {
            using (var db = new QuanLyHocSinh.Model.Entities.QLHocSinhEntities()) // Fully qualify the type name
            {
                var user = db.Users.Find(userId);
                if (user != null)
                {
                    user.MatKhau = newPassword;
                    db.SaveChanges();
                }
            }
        }

        public static void CapNhatTaiKhoan(User user)
        {
            using (var db = new QuanLyHocSinh.Model.Entities.QLHocSinhEntities()) // Fully qualify the type name
            {
                var existing = db.Users.Find(user.UserID);
                if (existing != null)
                {
                    existing.HoTen = user.HoTen;
                    existing.TenDangNhap = user.TenDangNhap;
                    existing.VaiTroID = user.VaiTroID;

                    if (!string.IsNullOrEmpty(user.MatKhau))
                    {
                        existing.MatKhau = user.MatKhau;
                    }

                    db.SaveChanges();
                }
            }
        }
        public static bool HasRelatedStudents(string userId)
        {
            using (var connection = new MySqlConnection(connectionString)) // Fixed: Use 'connectionString' instead of 'ConnectionString'
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM hocsinh WHERE UserID = @UserID";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
        }

        public static void XoaTaiKhoanVaHocSinh(string userId)
        {
            using (var connection = new MySqlConnection(connectionString)) // Fixed: Use 'connectionString' instead of 'ConnectionString'
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Xóa học sinh liên quan trước
                        string deleteStudentsQuery = "DELETE FROM hocsinh WHERE UserID = @UserID";
                        using (var cmd = new MySqlCommand(deleteStudentsQuery, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@UserID", userId);
                            cmd.ExecuteNonQuery();
                        }

                        // Sau đó xóa tài khoản
                        string deleteUserQuery = "DELETE FROM users WHERE UserID = @UserID";
                        using (var cmd = new MySqlCommand(deleteUserQuery, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@UserID", userId);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

    }
}



