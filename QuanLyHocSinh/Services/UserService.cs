using QuanLyHocSinh.Model.Entities;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Windows;

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
                                COALESCE(hs_info.HoTen, gv_info.HoTen, gvus_info.HoTen) AS HoTen,
                                u.VaiTroID,
                                v.TenVaiTro

                            FROM USERS u
                            JOIN VAITRO v ON u.VaiTroID = v.VaiTroID

                            -- Học sinh
                            LEFT JOIN HOCSINH hs ON hs.UserID = u.UserID
                            LEFT JOIN HOSOHOCSINH hhs ON hhs.HocSinhID = hs.HocSinhID
                            LEFT JOIN HOSO hs_info ON hs_info.HoSoID = hhs.HoSoID

                            -- Giáo viên
                            LEFT JOIN GIAOVIEN gv ON gv.UserID = u.UserID
                            LEFT JOIN HOSOGIAOVIEN hgv ON hgv.GiaoVienID = gv.GiaoVienID
                            LEFT JOIN HOSO gv_info ON gv_info.HoSoID = hgv.HoSoID

                            -- Giáo vụ
                            LEFT JOIN GIAOVU gvus ON gvus.UserID = u.UserID
                            LEFT JOIN HOSOGIAOVU hgvus ON hgvus.GiaoVuID = gvus.GiaoVuID
                            LEFT JOIN HOSO gvus_info ON gvus_info.HoSoID = hgvus.HoSoID;
                            ";

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

        //public static bool XoaTaiKhoan(string userID)
        //{
        //    if (string.IsNullOrEmpty(userID))
        //        return false;
        //    if (!CheckDuplicateUserID(userID))
        //        return false;

        //    using (var conn = new MySqlConnection(connectionString))
        //    {
        //        conn.Open();
        //        string query = "DELETE FROM USERS WHERE UserID = @UserID";

        //        using (var cmd = new MySqlCommand(query, conn))
        //        {
        //            cmd.Parameters.AddWithValue("@UserID", userID);

        //            int result = cmd.ExecuteNonQuery();
        //            return result > 0;
        //        }
        //    }
        //}


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


        public static void XoaTaiKhoanVaLienQuan(string userId)
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();
            using var tran = conn.BeginTransaction();

            try
            {
                // 🔒 Kiểm tra nếu là giáo vụ → KHÔNG CHO XOÁ
                string giaoVuID = null;
                using (var cmdCheckGVu = new MySqlCommand("SELECT GiaoVuID FROM GIAOVU WHERE UserID = @UserID", conn, tran))
                {
                    cmdCheckGVu.Parameters.AddWithValue("@UserID", userId);
                    giaoVuID = cmdCheckGVu.ExecuteScalar()?.ToString();
                }

                if (!string.IsNullOrEmpty(giaoVuID))
                {
                    MessageBox.Show("Không thể xóa tài khoản giáo vụ.");
                    tran.Rollback();
                    return;
                }

                // --- Học sinh ---
                string hocSinhID = null;
                using (var getHs = new MySqlCommand("SELECT HocSinhID FROM HOCSINH WHERE UserID = @UserID", conn, tran))
                {
                    getHs.Parameters.AddWithValue("@UserID", userId);
                    hocSinhID = getHs.ExecuteScalar()?.ToString();
                }

                if (!string.IsNullOrEmpty(hocSinhID))
                {
                    var cmdXoaCNDiem = new MySqlCommand(@"
                DELETE cnd FROM CAPNHATDIEM cnd
                JOIN CHITIETDIEM ctd ON cnd.ChiTietDiemID = ctd.ChiTietDiemID
                JOIN DIEM d ON ctd.DiemID = d.DiemID
                WHERE d.HocSinhID = @HocSinhID", conn, tran);
                    cmdXoaCNDiem.Parameters.AddWithValue("@HocSinhID", hocSinhID);
                    cmdXoaCNDiem.ExecuteNonQuery();

                    var cmdXoaCTD = new MySqlCommand(@"
                DELETE ctd FROM CHITIETDIEM ctd
                JOIN DIEM d ON ctd.DiemID = d.DiemID
                WHERE d.HocSinhID = @HocSinhID", conn, tran);
                    cmdXoaCTD.Parameters.AddWithValue("@HocSinhID", hocSinhID);
                    cmdXoaCTD.ExecuteNonQuery();

                    var cmdXoaDiem = new MySqlCommand("DELETE FROM DIEM WHERE HocSinhID = @HocSinhID", conn, tran);
                    cmdXoaDiem.Parameters.AddWithValue("@HocSinhID", hocSinhID);
                    cmdXoaDiem.ExecuteNonQuery();

                    var cmdXoaHS = new MySqlCommand("DELETE FROM HOSOHOCSINH WHERE HocSinhID = @HocSinhID", conn, tran);
                    cmdXoaHS.Parameters.AddWithValue("@HocSinhID", hocSinhID);
                    cmdXoaHS.ExecuteNonQuery();

                    var cmdDelHS = new MySqlCommand("DELETE FROM HOCSINH WHERE HocSinhID = @HocSinhID", conn, tran);
                    cmdDelHS.Parameters.AddWithValue("@HocSinhID", hocSinhID);
                    cmdDelHS.ExecuteNonQuery();
                }

                // --- Giáo viên ---
                string giaoVienID = null;
                using (var getGV = new MySqlCommand("SELECT GiaoVienID FROM GIAOVIEN WHERE UserID = @UserID", conn, tran))
                {
                    getGV.Parameters.AddWithValue("@UserID", userId);
                    giaoVienID = getGV.ExecuteScalar()?.ToString();
                }

                if (!string.IsNullOrEmpty(giaoVienID))
                {
                    // 🔍 Kiểm tra nếu giáo viên đang là GVCN → không cho xoá
                    var cmdCheckGVCN = new MySqlCommand("SELECT COUNT(*) FROM LOP WHERE GVCNID = @GiaoVienID", conn, tran);
                    cmdCheckGVCN.Parameters.AddWithValue("@GiaoVienID", giaoVienID);
                    int count = Convert.ToInt32(cmdCheckGVCN.ExecuteScalar());

                    if (count > 0)
                    {
                        MessageBox.Show("Không thể xóa giáo viên này vì đang là GVCN của lớp học.");
                        tran.Rollback();
                        return;
                    }

                    var cmdDelCNDGV = new MySqlCommand("DELETE FROM CAPNHATDIEM WHERE GiaoVienID = @GiaoVienID", conn, tran);
                    cmdDelCNDGV.Parameters.AddWithValue("@GiaoVienID", giaoVienID);
                    cmdDelCNDGV.ExecuteNonQuery();

                    var cmdDelCTMH = new MySqlCommand("DELETE FROM CHITIETMONHOC WHERE GiaoVienID = @GiaoVienID", conn, tran);
                    cmdDelCTMH.Parameters.AddWithValue("@GiaoVienID", giaoVienID);
                    cmdDelCTMH.ExecuteNonQuery();

                    var cmdDelHSGV = new MySqlCommand("DELETE FROM HOSOGIAOVIEN WHERE GiaoVienID = @GiaoVienID", conn, tran);
                    cmdDelHSGV.Parameters.AddWithValue("@GiaoVienID", giaoVienID);
                    cmdDelHSGV.ExecuteNonQuery();

                    var cmdDelGV = new MySqlCommand("DELETE FROM GIAOVIEN WHERE GiaoVienID = @GiaoVienID", conn, tran);
                    cmdDelGV.Parameters.AddWithValue("@GiaoVienID", giaoVienID);
                    cmdDelGV.ExecuteNonQuery();
                }

                // --- Các bảng liên quan chung ---
                var cmdPQ = new MySqlCommand("DELETE FROM PHANQUYEN WHERE GiaoVuPhanQuyenID = @UserID OR UserDuocPhanQuyenID = @UserID", conn, tran);
                cmdPQ.Parameters.AddWithValue("@UserID", userId);
                cmdPQ.ExecuteNonQuery();

                var cmdCapNhat = new MySqlCommand("DELETE FROM CAPNHAT WHERE UserID = @UserID", conn, tran);
                cmdCapNhat.Parameters.AddWithValue("@UserID", userId);
                cmdCapNhat.ExecuteNonQuery();

                var cmdDelUser = new MySqlCommand("DELETE FROM USERS WHERE UserID = @UserID", conn, tran);
                cmdDelUser.Parameters.AddWithValue("@UserID", userId);
                cmdDelUser.ExecuteNonQuery();

                tran.Commit();
            }
            catch (Exception ex)
            {
                tran.Rollback();
                MessageBox.Show("Xảy ra lỗi khi xoá tài khoản: " + ex.Message);
                throw;
            }
        }


    }
}



