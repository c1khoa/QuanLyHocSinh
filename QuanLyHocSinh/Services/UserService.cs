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
        public static async Task UpdateUserAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user), "Đối tượng người dùng không được null.");

            if (string.IsNullOrWhiteSpace(user.UserID))
                throw new ArgumentException("UserID không được để trống.");

            using var conn = new MySqlConnection(connectionString);
            await conn.OpenAsync();

            string query;
            if (string.IsNullOrWhiteSpace(user.MatKhau))
            {
                // Không cập nhật mật khẩu nếu người dùng không thay đổi
                query = @"UPDATE USERS SET TenDangNhap = @TenDangNhap, VaiTroID = @VaiTroID WHERE UserID = @UserID";
            }
            else
            {
                query = @"UPDATE USERS SET TenDangNhap = @TenDangNhap, MatKhau = @MatKhau, VaiTroID = @VaiTroID WHERE UserID = @UserID";
            }

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@TenDangNhap", user.TenDangNhap);
            cmd.Parameters.AddWithValue("@VaiTroID", user.VaiTroID);
            cmd.Parameters.AddWithValue("@UserID", user.UserID);

            if (!string.IsNullOrWhiteSpace(user.MatKhau))
                cmd.Parameters.AddWithValue("@MatKhau", user.MatKhau);

            int affectedRows = await cmd.ExecuteNonQueryAsync();
            if (affectedRows == 0)
            {
                throw new InvalidOperationException($"Không tìm thấy UserID {user.UserID} để cập nhật.");
            }
        }

        // Lấy danh sách tài khoản từ DB
        public static List<User> LayDanhSachTaiKhoan()
        {
            var danhSach = new List<User>();

            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                string query = @"SELECT 
                                u.UserID,
                                u.TenDangNhap,
                                u.MatKhau,
                                h.HoTen,
                                u.VaiTroID,
                                v.TenVaiTro
                            FROM USERS u
                            JOIN VAITRO v ON u.VaiTroID = v.VaiTroID
                            LEFT JOIN GIAOVIEN gv ON gv.UserID = u.UserID
                            LEFT JOIN HOSOGIAOVIEN hgv ON hgv.GiaoVienID = gv.GiaoVienID
                            LEFT JOIN HOCSINH hs ON hs.UserID = u.UserID
                            LEFT JOIN HOSOHOCSINH hhs ON hhs.HocSinhID = hs.HocSinhID
                            LEFT JOIN HOSO h ON h.HoSoID = COALESCE(hgv.HoSoID, hhs.HoSoID)";

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
                            HoTen = reader["HoTen"]?.ToString() ?? string.Empty,
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
            if (string.IsNullOrEmpty(user.TenDangNhap)) return false;
            if (CheckDuplicateUsername(user.TenDangNhap)) return false;

            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string userID = user.UserID ?? Guid.NewGuid().ToString();
                        string roleID = "";
                        string hoSoID = "HSO" + userID.Substring(1);

                        // 1. Thêm vào bảng USERS
                        string userQuery = @"INSERT INTO USERS (UserID, TenDangNhap, MatKhau, VaiTroID) 
                                     VALUES (@UserID, @TenDangNhap, @MatKhau, @VaiTroID)";
                        using (var cmd = new MySqlCommand(userQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@UserID", userID);
                            cmd.Parameters.AddWithValue("@TenDangNhap", user.TenDangNhap);
                            cmd.Parameters.AddWithValue("@MatKhau", user.MatKhau ?? string.Empty);
                            cmd.Parameters.AddWithValue("@VaiTroID", user.VaiTroID ?? string.Empty);
                            if (cmd.ExecuteNonQuery() <= 0)
                            {
                                transaction.Rollback();
                                return false;
                            }
                        }

                        // 2. Thêm vào bảng vai trò
                        // Replace the `.With` usage with direct parameter addition and execution
                        switch (user.VaiTroID?.ToUpper())
                        {
                            case "VT01": // Học sinh
                                roleID = "HS" + userID.Substring(1);
                                using (var cmd = new MySqlCommand("INSERT INTO HOCSINH (HocSinhID, UserID) VALUES (@ID, @UserID)", conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@ID", roleID);
                                    cmd.Parameters.AddWithValue("@UserID", userID);
                                    cmd.ExecuteNonQuery();
                                }
                                break;

                            case "VT02": // Giáo viên
                                roleID = "GV" + userID.Substring(1);
                                using (var cmd = new MySqlCommand("INSERT INTO GIAOVIEN (GiaoVienID, UserID) VALUES (@ID, @UserID)", conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@ID", roleID);
                                    cmd.Parameters.AddWithValue("@UserID", userID);
                                    cmd.ExecuteNonQuery();
                                }
                                break;

                            case "VT03": // Giáo vụ
                                roleID = "GVU" + userID.Substring(1);
                                using (var cmd = new MySqlCommand("INSERT INTO GIAOVU (GiaoVuID, UserID) VALUES (@ID, @UserID)", conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@ID", roleID);
                                    cmd.Parameters.AddWithValue("@UserID", userID);
                                    cmd.ExecuteNonQuery();
                                }
                                break;
                        }


                        // 3. Tạo HOSO
                        string insertHoSo = @"
                    INSERT INTO HOSO (HoSoID, HoTen, GioiTinh, NgaySinh, Email, DiaChi, ChucVuID, TrangThaiHoSo, NgayTao, NgayCapNhatGanNhat)
                    VALUES (@HoSoID, @HoTen, 'Nam', NOW(), 'test@email.com', 'Chua cap nhat', 'CV001', 'danghoatdong', NOW(), NOW())";
                        using (var cmd = new MySqlCommand(insertHoSo, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@HoSoID", hoSoID);
                            cmd.Parameters.AddWithValue("@HoTen", user.HoTen ?? "Không rõ");
                            cmd.ExecuteNonQuery();
                        }

                        // 4. Liên kết HOSO với vai trò
                        if (user.VaiTroID == "VT01") // học sinh
                        {
                            string linkQuery = @"INSERT INTO HOSOHOCSINH (HoSoHocSinhID, HocSinhID, HoSoID, LopHocID, NienKhoa)
                                         VALUES (@LinkID, @HocSinhID, @HoSoID, '10A1', 2025)";
                            using (var linkCmd = new MySqlCommand(linkQuery, conn, transaction))
                            {
                                linkCmd.Parameters.AddWithValue("@LinkID", "HSHS" + userID.Substring(1));
                                linkCmd.Parameters.AddWithValue("@HocSinhID", roleID);
                                linkCmd.Parameters.AddWithValue("@HoSoID", hoSoID);
                                linkCmd.ExecuteNonQuery();
                            }
                        }
                        else if (user.VaiTroID == "VT02") // giáo viên
                        {
                            string linkQuery = @"INSERT INTO HOSOGIAOVIEN (HoSoGiaoVienID, GiaoVienID, HoSoID, LopDayID, NgayBatDauLamViec)
                                         VALUES (@LinkID, @GiaoVienID, @HoSoID, '10A1', NOW())";
                            using (var linkCmd = new MySqlCommand(linkQuery, conn, transaction))
                            {
                                linkCmd.Parameters.AddWithValue("@LinkID", "HSGV" + userID.Substring(1));
                                linkCmd.Parameters.AddWithValue("@GiaoVienID", roleID);
                                linkCmd.Parameters.AddWithValue("@HoSoID", hoSoID);
                                linkCmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine("Lỗi khi thêm tài khoản: " + ex.Message);
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

                // 1. Cập nhật bảng USERS
                string updateUserQuery = @"
            UPDATE USERS 
            SET TenDangNhap = @TenDangNhap, 
                MatKhau = @MatKhau, 
                VaiTroID = @VaiTroID
            WHERE UserID = @UserID";

                using (var cmd = new MySqlCommand(updateUserQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@TenDangNhap", user.TenDangNhap);
                    cmd.Parameters.AddWithValue("@MatKhau", user.MatKhau ?? string.Empty);
                    cmd.Parameters.AddWithValue("@VaiTroID", user.VaiTroID ?? string.Empty);
                    cmd.Parameters.AddWithValue("@UserID", user.UserID);
                    cmd.ExecuteNonQuery();
                }

                // 2. Cập nhật HoTen trong bảng HOSO (nếu có)
                string updateHoSoQuery = @"
            UPDATE HOSO h
            JOIN HOSOGIAOVIEN hgv ON h.HoSoID = hgv.HoSoID
            JOIN GIAOVIEN gv ON hgv.GiaoVienID = gv.GiaoVienID
            SET h.HoTen = @HoTen
            WHERE gv.UserID = @UserID;

            UPDATE HOSO h
            JOIN HOSOHOCSINH hhs ON h.HoSoID = hhs.HoSoID
            JOIN HOCSINH hs ON hhs.HocSinhID = hs.HocSinhID
            SET h.HoTen = @HoTen
            WHERE hs.UserID = @UserID;
        ";

                using (var hoSoCmd = new MySqlCommand(updateHoSoQuery, conn))
                {
                    hoSoCmd.Parameters.AddWithValue("@UserID", user.UserID);
                    hoSoCmd.Parameters.AddWithValue("@HoTen", user.HoTen ?? string.Empty);
                    hoSoCmd.ExecuteNonQuery();
                }

                return true; // ✅ Bổ sung return ở cuối
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
            using var conn = new MySqlConnection(connectionString);
            conn.Open();
            string sql = "SELECT COUNT(*) FROM HOCSINH WHERE UserID = @UserID";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@UserID", userId);
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

    
        public static void XoaTaiKhoanVaHocSinh(string userId)
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();
            using var tran = conn.BeginTransaction();
            try
            {
                // 1. Lấy HocSinhID từ UserID
                string hocSinhID = null;
                using (var getHs = new MySqlCommand("SELECT HocSinhID FROM HOCSINH WHERE UserID = @UserID", conn, tran))
                {
                    getHs.Parameters.AddWithValue("@UserID", userId);
                    var result = getHs.ExecuteScalar();
                    hocSinhID = result?.ToString();
                }

                // Nếu là học sinh → xoá toàn bộ dữ liệu liên quan
                if (!string.IsNullOrEmpty(hocSinhID))
                {
                    // 1.1. Xoá CAPNHATDIEM trước CHITIETDIEM
                    var cmdCapNhatDiem = new MySqlCommand(@"
                        DELETE cnd FROM CAPNHATDIEM cnd
                        JOIN CHITIETDIEM ctd ON cnd.ChiTietDiemID = ctd.ChiTietDiemID
                        JOIN DIEM d ON ctd.DiemID = d.DiemID
                        WHERE d.HocSinhID = @HocSinhID", conn, tran);
                    cmdCapNhatDiem.Parameters.AddWithValue("@HocSinhID", hocSinhID);
                    cmdCapNhatDiem.ExecuteNonQuery();

                    // 1.2. Xoá CHITIETDIEM
                    var cmdCTD = new MySqlCommand(@"
                        DELETE ctd FROM CHITIETDIEM ctd
                        JOIN DIEM d ON ctd.DiemID = d.DiemID
                        WHERE d.HocSinhID = @HocSinhID", conn, tran);
                    cmdCTD.Parameters.AddWithValue("@HocSinhID", hocSinhID);
                    cmdCTD.ExecuteNonQuery();

                    // 1.3. Xoá DIEM
                    var cmdDiem = new MySqlCommand("DELETE FROM DIEM WHERE HocSinhID = @HocSinhID", conn, tran);
                    cmdDiem.Parameters.AddWithValue("@HocSinhID", hocSinhID);
                    cmdDiem.ExecuteNonQuery();

                    // 1.4. Xoá HOSOHOCSINH
                    var cmdHHS = new MySqlCommand("DELETE FROM HOSOHOCSINH WHERE HocSinhID = @HocSinhID", conn, tran);
                    cmdHHS.Parameters.AddWithValue("@HocSinhID", hocSinhID);
                    cmdHHS.ExecuteNonQuery();

                    // 1.5. Xoá HOCSINH
                    var cmdHS = new MySqlCommand("DELETE FROM HOCSINH WHERE HocSinhID = @HocSinhID", conn, tran);
                    cmdHS.Parameters.AddWithValue("@HocSinhID", hocSinhID);
                    cmdHS.ExecuteNonQuery();
                }

                // 2. Xoá PHANQUYEN, CAPNHAT theo UserID
                var cmdPQ = new MySqlCommand("DELETE FROM PHANQUYEN WHERE UserDuocPhanQuyenID = @UserID OR GiaoVuPhanQuyenID = @UserID", conn, tran);
                cmdPQ.Parameters.AddWithValue("@UserID", userId);
                cmdPQ.ExecuteNonQuery();

                var cmdCN = new MySqlCommand("DELETE FROM CAPNHAT WHERE UserID = @UserID", conn, tran);
                cmdCN.Parameters.AddWithValue("@UserID", userId);
                cmdCN.ExecuteNonQuery();

                // 3. Xoá USERS
                var cmdUser = new MySqlCommand("DELETE FROM USERS WHERE UserID = @UserID", conn, tran);
                cmdUser.Parameters.AddWithValue("@UserID", userId);
                cmdUser.ExecuteNonQuery();

                tran.Commit();
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }




    }
}



