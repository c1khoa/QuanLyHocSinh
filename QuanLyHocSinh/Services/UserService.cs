using QuanLyHocSinh.Model.Entities;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Windows;
using DocumentFormat.OpenXml.Office.Word;
using DocumentFormat.OpenXml.Wordprocessing;

namespace QuanLyHocSinh.Service
{
    public static class UserService
    {
        public static string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
        public static void UpdateUserAndHoSo(User editedUser, string? newPassword, MySqlConnection conn, MySqlTransaction trans)
        {
            // Cập nhật bảng USERS
            using var cmdUser = new MySqlCommand
            {
                Connection = conn,
                Transaction = trans
            };

            cmdUser.Parameters.AddWithValue("@TenDangNhap", editedUser.TenDangNhap);
            cmdUser.Parameters.AddWithValue("@VaiTroID", editedUser.VaiTroID);
            cmdUser.Parameters.AddWithValue("@UserID", editedUser.UserID);

            if (!string.IsNullOrWhiteSpace(newPassword))
            {
                cmdUser.CommandText = @"
                UPDATE USERS 
                SET TenDangNhap = @TenDangNhap, 
                    VaiTroID = @VaiTroID, 
                    MatKhau = @MatKhau
                WHERE UserID = @UserID;";
                cmdUser.Parameters.AddWithValue("@MatKhau", editedUser.MatKhau);
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

            // Cập nhật bảng HOSO
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
            cmdHoSo.Parameters.AddWithValue("@HoTen", editedUser.HoTen);
            cmdHoSo.Parameters.AddWithValue("@NgayCapNhat", DateTime.Now);
            cmdHoSo.Parameters.AddWithValue("@UserID", editedUser.UserID);

            int rowsAffected = cmdHoSo.ExecuteNonQuery();

            if (rowsAffected == 0)
            {
                throw new Exception("Cảnh báo: Đã cập nhật tài khoản nhưng không tìm thấy hồ sơ liên quan.");
            }
        }
        public static string LayUserIDMoi()
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT MAX(UserID) FROM USERS WHERE UserID LIKE 'U%'";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    var result = cmd.ExecuteScalar()?.ToString();

                    if (!string.IsNullOrEmpty(result) && result.Length > 1)
                    {
                        string numberPart = result.Substring(1); // bỏ chữ 'U'
                        if (int.TryParse(numberPart, out int maxNumber))
                        {
                            // tăng số và format về 7 chữ số
                            return "U" + (maxNumber + 1).ToString("D7");
                        }
                    }

                    // Nếu chưa có User nào
                    return "U0000001";
                }
            }
        }
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
        public static List<MonHoc> LayDanhSachBoMon()
        {
            var danhSachBoMon = new List<MonHoc>();
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT * FROM MONHOC";
                using (var cmd = new MySqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var monHoc = new MonHoc
                        {
                            MonHocID = reader["MonHocID"].ToString(),
                            TenMonHoc = reader["TenMonHoc"].ToString(),
                        };
                        danhSachBoMon.Add(monHoc);
                    }
                }
            }
            return danhSachBoMon;
        }



        public static string LastErrorMessage = "";
        public static string LayDiemIDMoi(MySqlConnection conn, MySqlTransaction transaction)
        {
            string query = "SELECT DiemID FROM DIEM ORDER BY DiemID DESC LIMIT 1";
            using (var cmd = new MySqlCommand(query, conn, transaction))
            {
                var result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    string lastID = result.ToString(); // ví dụ: "D00123"
                    int number = int.Parse(lastID.Substring(1));
                    return $"D{(number + 1):D7}";
                }
                else
                {
                    return "D0000001"; // Trường hợp chưa có bản ghi nào
                }
            }
        }
        public static string LayChiTietDiemIDMoi(MySqlConnection conn, MySqlTransaction transaction)
        {
            string query = "SELECT ChiTietDiemID FROM CHITIETDIEM ORDER BY ChiTietDiemID DESC LIMIT 1";
            using (var cmd = new MySqlCommand(query, conn, transaction))
            {
                var result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    string lastID = result.ToString(); // ví dụ: "CTD00057"
                    int number = int.Parse(lastID.Substring(3));
                    return $"CTD{(number + 1):D9}";
                }
                else
                {
                    return "CTD000000001"; // Trường hợp chưa có bản ghi nào
                }
            }
        }


        public static bool ThemTaiKhoan(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrEmpty(user.TenDangNhap)) return false;
            if (CheckDuplicateUsername(user.TenDangNhap))
            {
                LastErrorMessage = "Tên đăng nhập đã tồn tại.";
                return false;
            }

            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string userID = user.UserID;
                        string hoSoID = user.MaHoSo;
                        string hoTen = user.HoTen;
                        string tenChucVu = user.ChucVu;

                        // 1. USERS
                        string userQuery = "INSERT INTO USERS (UserID, TenDangNhap, MatKhau, VaiTroID) VALUES (@UserID, @TenDangNhap, @MatKhau, @VaiTroID)";
                        using (var cmd = new MySqlCommand(userQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@UserID", userID);
                            cmd.Parameters.AddWithValue("@TenDangNhap", user.TenDangNhap);
                            cmd.Parameters.AddWithValue("@MatKhau", user.MatKhau ?? "");
                            cmd.Parameters.AddWithValue("@VaiTroID", user.VaiTroID ?? "");
                            if (cmd.ExecuteNonQuery() <= 0)
                            {
                                LastErrorMessage = "Không thể thêm vào bảng USERS.";
                                transaction.Rollback();
                                return false;
                            }
                        }

                        // 2. Bảng vai trò cụ thể
                        switch (user.VaiTroID?.ToUpper())
                        {
                            case "VT01": // Học sinh
                                using (var cmd = new MySqlCommand("INSERT INTO HOCSINH (HocSinhID, UserID) VALUES (@ID, @UserID)", conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@ID", user.HocSinhID);
                                    cmd.Parameters.AddWithValue("@UserID", userID);
                                    cmd.ExecuteNonQuery();
                                }
                                break;

                            case "VT02": // Giáo viên
                                using (var cmd = new MySqlCommand("INSERT INTO GIAOVIEN (GiaoVienID, UserID) VALUES (@ID, @UserID)", conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@ID", user.GiaoVienID);
                                    cmd.Parameters.AddWithValue("@UserID", userID);
                                    cmd.ExecuteNonQuery();
                                }
                                break;

                            case "VT03": // Giáo vụ
                                using (var cmd = new MySqlCommand("INSERT INTO GIAOVU (GiaoVuID, UserID) VALUES (@ID, @UserID)", conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@ID", user.GiaoVuID);
                                    cmd.Parameters.AddWithValue("@UserID", userID);
                                    cmd.ExecuteNonQuery();
                                }
                                break;

                            default:
                                throw new Exception("Vai trò không hợp lệ.");
                        }
                        string ChucVuID = null;
                        using (var cmd = new MySqlCommand("SELECT ChucVuID FROM CHUCVU WHERE TenChucVu = @TenChucVu", conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@TenChucVu", tenChucVu);
                            using (var reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                    ChucVuID = reader["ChucVuID"].ToString();
                            }
                        }

                        // 3. HOSO
                        string insertHoSo = @"INSERT INTO HOSO (HoSoID, HoTen, GioiTinh, NgaySinh, Email, DiaChi, ChucVuID, TrangThaiHoSo, NgayTao, NgayCapNhatGanNhat)
                                        VALUES (@HoSoID, @HoTen, @GioiTinh, @NgaySinh, @Email, @DiaChi, @ChucVuID, @TrangThai, NOW(), NOW())";
                        using (var cmd = new MySqlCommand(insertHoSo, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@HoSoID", hoSoID);
                            cmd.Parameters.AddWithValue("@HoTen", hoTen);
                            cmd.Parameters.AddWithValue("@GioiTinh", user.GioiTinh ?? "Nam");
                            cmd.Parameters.AddWithValue("@NgaySinh", user.NgaySinh ?? new DateTime(2000, 1, 1));
                            cmd.Parameters.AddWithValue("@Email", user.Email ?? "test@email.com");
                            cmd.Parameters.AddWithValue("@DiaChi", user.DiaChi ?? "Chưa cập nhật");
                            cmd.Parameters.AddWithValue("@ChucVuID", ChucVuID);
                            cmd.Parameters.AddWithValue("@TrangThai", "Đang hoạt động");
                            cmd.ExecuteNonQuery();
                        }

                        // 4. HOSO cụ thể
                        if (user.VaiTroID == "VT01")
                        {
                            string linkQuery = "INSERT INTO HOSOHOCSINH (HoSoHocSinhID, HocSinhID, HoSoID, LopHocID, NienKhoa) VALUES (@LinkID, @HocSinhID, @HoSoID, @LopHocID, @NienKhoa)";
                            using (var linkCmd = new MySqlCommand(linkQuery, conn, transaction))
                            {
                                linkCmd.Parameters.AddWithValue("@LinkID", user.MaHoSoCaNhan);
                                linkCmd.Parameters.AddWithValue("@HocSinhID", user.HocSinhID);
                                linkCmd.Parameters.AddWithValue("@HoSoID", hoSoID);
                                linkCmd.Parameters.AddWithValue("@LopHocID", user.LopHocID ?? "10A1");
                                linkCmd.Parameters.AddWithValue("@NienKhoa", 2025);
                                linkCmd.ExecuteNonQuery();
                            }
                            // Tạo các bản ghi điểm rỗng cho học sinh
                            using (var monCmd = new MySqlCommand("SELECT MonHocID FROM MONHOC", conn, transaction))
                            using (var monReader = monCmd.ExecuteReader())
                            {
                                List<string> monHocIDs = new List<string>();
                                while (monReader.Read())
                                {
                                    monHocIDs.Add(monReader.GetString("MonHocID"));
                                }
                                monReader.Close();
                                for (int i = 1; i < 3; i++)
                                {
                                    foreach (var monID in monHocIDs)
                                    {
                                        string diemID = LayDiemIDMoi(conn, transaction); // Tự tạo ID nếu cần
                                        string[] loaiDiem = new[] { "LD01", "LD02", "LD03", "LD04" }; // Ví dụ: Miệng, 15p, 1 tiết, thi
                                        using (var diemCmd = new MySqlCommand("INSERT INTO DIEM (DiemID, HocSinhID, MonHocID, HocKy, NamHocID, DiemTrungBinh, XepLoai) VALUES (@DiemID, @HocSinhID, @MonHocID, @HocKy, @NamHocID, NULL, NULL)", conn, transaction))
                                        {
                                            diemCmd.Parameters.AddWithValue("@DiemID", diemID);
                                            diemCmd.Parameters.AddWithValue("@HocSinhID", user.HocSinhID);
                                            diemCmd.Parameters.AddWithValue("@MonHocID", monID);
                                            diemCmd.Parameters.AddWithValue("@HocKy", i); // hoặc thêm cả 2 học kỳ nếu muốn
                                            diemCmd.Parameters.AddWithValue("@NamHocID", "NH2025"); // lấy theo năm học hiện tại
                                            diemCmd.ExecuteNonQuery();
                                        }
                                        foreach (string loai in loaiDiem)
                                        {
                                            string chiTietID = LayChiTietDiemIDMoi(conn, transaction);
                                            using (var ctCmd = new MySqlCommand("INSERT INTO CHITIETDIEM (ChiTietDiemID, DiemID, LoaiDiemID, GiaTri) VALUES (@ID, @DiemID, @LoaiDiemID, NULL)", conn, transaction))
                                            {
                                                ctCmd.Parameters.AddWithValue("@ID", chiTietID);
                                                ctCmd.Parameters.AddWithValue("@DiemID", diemID);
                                                ctCmd.Parameters.AddWithValue("@LoaiDiemID", loai);
                                                ctCmd.ExecuteNonQuery();
                                            }
                                        }
                                    }
                                }
                            }

                        }
                        else if (user.VaiTroID == "VT02")
                        {

                            // HOSOGIAOVIEN
                            using (var cmd = new MySqlCommand("INSERT INTO HOSOGIAOVIEN (HoSoGiaoVienID, GiaoVienID, HoSoID, NgayBatDauLamViec) VALUES (@ID, @GiaoVienID, @HoSoID, NOW())", conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@ID", user.MaHoSoCaNhan);
                                cmd.Parameters.AddWithValue("@GiaoVienID", user.GiaoVienID);
                                cmd.Parameters.AddWithValue("@HoSoID", hoSoID);
                                cmd.ExecuteNonQuery();
                            }

                            // PHANCONGDAY cho LopDayID1/2/3
                            string[] lopDayIDs = new[] { user.LopDayID1, user.LopDayID2, user.LopDayID3 };
                            foreach (string lopID in lopDayIDs)

                            {
                                string phanCongID = LayPhanCongDayIDMoi(conn, transaction);
                                if (!string.IsNullOrEmpty(lopID))
                                {
                                    using (var cmd = new MySqlCommand("INSERT INTO PHANCONGDAY (PhanCongDayID, GiaoVienID, LopID, MonHocID, NamHocID) VALUES (@PhanCongDayID, @GiaoVienID, @LopID, @MonHocID, @NamHocID)", conn, transaction))
                                    {
                                        cmd.Parameters.AddWithValue("@PhanCongDayID", phanCongID);
                                        cmd.Parameters.AddWithValue("@GiaoVienID", user.GiaoVienID);
                                        cmd.Parameters.AddWithValue("@LopID", lopID);
                                        cmd.Parameters.AddWithValue("@MonHocID", user.BoMon);
                                        cmd.Parameters.AddWithValue("@NamHocID", "NH2025");
                                        cmd.ExecuteNonQuery();
                                    }
                                }
                            }

                            // GVCN: cập nhật lớp
                            // GVCN: kiểm tra lớp đã có GVCN chưa
                            if (!string.IsNullOrEmpty(user.LopDayIDCN))
                            {
                                // 1. Kiểm tra lớp đã có GVCN chưa
                                using (var checkCmd = new MySqlCommand("SELECT GVCNID FROM LOP WHERE LopID = @LopID", conn, transaction))
                                {
                                    checkCmd.Parameters.AddWithValue("@LopID", user.LopDayIDCN);
                                    var existingGVCN = checkCmd.ExecuteScalar();

                                    if (existingGVCN != null && existingGVCN != DBNull.Value)
                                    {
                                        LastErrorMessage = $"Lớp {user.LopDayIDCN} đã có giáo viên chủ nhiệm.";
                                        transaction.Rollback();
                                        return false;
                                    }
                                }

                                // 2. Gán GVCN nếu lớp chưa có
                                using (var updateCmd = new MySqlCommand("UPDATE LOP SET GVCNID = @GiaoVienID WHERE LopID = @LopID", conn, transaction))
                                {
                                    updateCmd.Parameters.AddWithValue("@GiaoVienID", user.GiaoVienID);
                                    updateCmd.Parameters.AddWithValue("@LopID", user.LopDayIDCN);
                                    updateCmd.ExecuteNonQuery();
                                }
                            }

                        }
                        else if (user.VaiTroID == "VT03")
                        {
                            string linkQuery = "INSERT INTO HOSOGIAOVU (HoSoGiaoVuID, GiaoVuID, HoSoID) VALUES (@LinkID, @GiaoVuID, @HoSoID)";
                            using (var linkCmd = new MySqlCommand(linkQuery, conn, transaction))
                            {
                                linkCmd.Parameters.AddWithValue("@LinkID", user.MaHoSoCaNhan);
                                linkCmd.Parameters.AddWithValue("@GiaoVuID", user.GiaoVuID);
                                linkCmd.Parameters.AddWithValue("@HoSoID", hoSoID);
                                linkCmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        LastErrorMessage = ex.Message;
                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }

        private static bool CheckDuplicateUsername(string username)
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT COUNT(*) FROM USERS WHERE TenDangNhap = @TenDangNhap";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@TenDangNhap", username);
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
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
        public static List<string> LayDanhSachTenChucVuTheoVaiTro(string vaiTroID)
        {
            List<string> danhSachTenChucVu = new List<string>();

            string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
            string query = @"SELECT TenChucVu FROM CHUCVU WHERE VaiTroID = @VaiTroID";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@VaiTroID", vaiTroID);

                connection.Open();
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string tenChucVu = reader["TenChucVu"].ToString();
                        danhSachTenChucVu.Add(tenChucVu);
                    }
                }
            }

            return danhSachTenChucVu;
        }


        public static string LayHoSoIDMoi()
        {
            string newID = "HOSO00000001";
            string query = "SELECT MAX(HoSoID) FROM HOSO WHERE HoSoID LIKE 'HOSO%'";
            using var conn = new MySqlConnection(connectionString);
            {
                conn.Open();
                using (var cmd = new MySqlCommand(query, conn))
                {
                    var result = cmd.ExecuteScalar()?.ToString();
                    if (!string.IsNullOrEmpty(result))
                    {
                        string numberPart = new string(result.Where(char.IsDigit).ToArray());
                        if (int.TryParse(numberPart, out int num))
                        {
                            newID = $"HOSO{(num + 1):D8}";
                        }
                    }
                }
            }
            return newID;
        }
        public static string LayPhanCongDayIDMoi(MySqlConnection conn, MySqlTransaction transaction)
        {
            string newID = "PCD000000001";
            string query = "SELECT MAX(PhanCongDayID) FROM PHANCONGDAY WHERE PhanCongDayID LIKE 'PCD%'";

            using (var cmd = new MySqlCommand(query, conn, transaction))
            {
                var result = cmd.ExecuteScalar()?.ToString();
                if (!string.IsNullOrEmpty(result))
                {
                    string numberPart = new string(result.Where(char.IsDigit).ToArray());
                    if (int.TryParse(numberPart, out int num))
                    {
                        newID = $"PCD{(num + 1):D9}";
                    }
                }
            }

            return newID;
        }

        public static string LayMaHoSoCaNhanMoi(string vaiTroID)
        {
            string prefix, table, column;

            switch (vaiTroID)
            {
                case "VT01":
                    prefix = "HSHS";
                    table = "HOSOHOCSINH";
                    column = "HoSoHocSinhID";
                    break;
                case "VT02":
                    prefix = "HSGV";
                    table = "HOSOGIAOVIEN";
                    column = "HoSoGiaoVienID";
                    break;
                case "VT03":
                    prefix = "HSAD";
                    table = "HOSOGIAOVU";
                    column = "HoSoGiaoVuID";
                    break;
                default:
                    throw new ArgumentException("Vai trò không hợp lệ.");
            }

            string query = $"SELECT MAX({column}) FROM {table} WHERE {column} LIKE '{prefix}%'";
            using var conn = new MySqlConnection(connectionString);
            conn.Open();
            using var cmd = new MySqlCommand(query, conn);
            var result = cmd.ExecuteScalar()?.ToString();

            if (!string.IsNullOrEmpty(result))
            {
                // Tách số và tăng
                string numberPart = new string(result.SkipWhile(c => !char.IsDigit(c)).ToArray());
                if (int.TryParse(numberPart, out int number))
                {
                    return $"{prefix}{(number + 1):D6}";
                }
            }

            return $"{prefix}000001";
        }




        public static string GetNextLoginUsername(string vaiTroID)
        {
            if (string.IsNullOrEmpty(vaiTroID)) return "";

            string prefix = vaiTroID switch
            {
                "VT01" => "HS",
                "VT02" => "GV",
                "VT03" => "AD", // AD → GIAOVU
                _ => "U"
            };

            string table = vaiTroID switch
            {
                "VT01" => "HOCSINH",
                "VT02" => "GIAOVIEN",
                "VT03" => "GIAOVU",
                _ => null
            };

            string idColumn = table switch
            {
                "HOCSINH" => "HocSinhID",
                "GIAOVIEN" => "GiaoVienID",
                "GIAOVU" => "GiaoVuID",
                _ => null
            };

            if (table == null || idColumn == null) return prefix + "000001";

            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            string query = $"SELECT MAX({idColumn}) FROM {table} WHERE {idColumn} LIKE @prefix";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@prefix", prefix + "%");

            string maxID = cmd.ExecuteScalar()?.ToString();
            int nextNumber = 1;

            if (!string.IsNullOrEmpty(maxID) && maxID.Length > prefix.Length)
            {
                string numberPart = maxID.Substring(prefix.Length);
                if (int.TryParse(numberPart, out int currentMax))
                {
                    nextNumber = currentMax + 1;
                }
            }

            return prefix + nextNumber.ToString("D6");
        }




        public static string XoaTaiKhoanVaLienQuan(string userId)
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();
            using var tran = conn.BeginTransaction();

            try
            {
                // ❌ Nếu là giáo vụ
                string giaoVuID = null;
                using (var cmd = new MySqlCommand("SELECT GiaoVuID FROM GIAOVU WHERE UserID = @UserID", conn, tran))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    giaoVuID = cmd.ExecuteScalar()?.ToString();
                }
                if (!string.IsNullOrEmpty(giaoVuID))
                {
                    tran.Rollback();
                    return "Không thể xóa tài khoản giáo vụ.";
                }

                // 👨‍🎓 Nếu là học sinh
                string hocSinhID = null;
                using (var cmd = new MySqlCommand("SELECT HocSinhID FROM HOCSINH WHERE UserID = @UserID", conn, tran))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    hocSinhID = cmd.ExecuteScalar()?.ToString();
                }
                if (!string.IsNullOrEmpty(hocSinhID))
                {
                    new MySqlCommand(@"
                DELETE cnd FROM CAPNHATDIEM cnd
                JOIN CHITIETDIEM ctd ON cnd.ChiTietDiemID = ctd.ChiTietDiemID
                JOIN DIEM d ON ctd.DiemID = d.DiemID
                WHERE d.HocSinhID = @HocSinhID", conn, tran)
                        .WithParameter("@HocSinhID", hocSinhID).ExecuteNonQuery();

                    new MySqlCommand(@"
                DELETE ctd FROM CHITIETDIEM ctd
                JOIN DIEM d ON ctd.DiemID = d.DiemID
                WHERE d.HocSinhID = @HocSinhID", conn, tran)
                        .WithParameter("@HocSinhID", hocSinhID).ExecuteNonQuery();

                    new MySqlCommand("DELETE FROM DIEM WHERE HocSinhID = @HocSinhID", conn, tran)
                        .WithParameter("@HocSinhID", hocSinhID).ExecuteNonQuery();

                    new MySqlCommand("DELETE FROM HOSOHOCSINH WHERE HocSinhID = @HocSinhID", conn, tran)
                        .WithParameter("@HocSinhID", hocSinhID).ExecuteNonQuery();

                    new MySqlCommand("DELETE FROM HOCSINH WHERE HocSinhID = @HocSinhID", conn, tran)
                        .WithParameter("@HocSinhID", hocSinhID).ExecuteNonQuery();
                }

                // 👨‍🏫 Nếu là giáo viên
                string giaoVienID = null;
                using (var cmd = new MySqlCommand("SELECT GiaoVienID FROM GIAOVIEN WHERE UserID = @UserID", conn, tran))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    giaoVienID = cmd.ExecuteScalar()?.ToString();
                }
                if (!string.IsNullOrEmpty(giaoVienID))
                {
                    int count = Convert.ToInt32(new MySqlCommand("SELECT COUNT(*) FROM LOP WHERE GVCNID = @GiaoVienID", conn, tran)
                        .WithParameter("@GiaoVienID", giaoVienID).ExecuteScalar());

                    if (count > 0)
                    {
                        tran.Rollback();
                        return "Không thể xóa giáo viên này vì đang là GVCN của lớp học.";
                    }

                    new MySqlCommand("DELETE FROM CAPNHATDIEM WHERE GiaoVienID = @GiaoVienID", conn, tran)
                        .WithParameter("@GiaoVienID", giaoVienID).ExecuteNonQuery();

                    new MySqlCommand("DELETE FROM CHITIETMONHOC WHERE GiaoVienID = @GiaoVienID", conn, tran)
                        .WithParameter("@GiaoVienID", giaoVienID).ExecuteNonQuery();

                    new MySqlCommand("DELETE FROM PHANCONGDAY WHERE GiaoVienID = @GiaoVienID", conn, tran)
                        .WithParameter("@GiaoVienID", giaoVienID).ExecuteNonQuery();

                    new MySqlCommand("DELETE FROM HOSOGIAOVIEN WHERE GiaoVienID = @GiaoVienID", conn, tran)
                        .WithParameter("@GiaoVienID", giaoVienID).ExecuteNonQuery();

                    new MySqlCommand("DELETE FROM GIAOVIEN WHERE GiaoVienID = @GiaoVienID", conn, tran)
                        .WithParameter("@GiaoVienID", giaoVienID).ExecuteNonQuery();
                }

                // 🔁 Bảng liên quan người dùng
                new MySqlCommand("DELETE FROM PHANQUYEN WHERE GiaoVuPhanQuyenID = @UserID OR UserDuocPhanQuyenID = @UserID", conn, tran)
                    .WithParameter("@UserID", userId).ExecuteNonQuery();

                new MySqlCommand("DELETE FROM CAPNHAT WHERE UserID = @UserID", conn, tran)
                    .WithParameter("@UserID", userId).ExecuteNonQuery();

                new MySqlCommand("DELETE FROM USERS WHERE UserID = @UserID", conn, tran)
                    .WithParameter("@UserID", userId).ExecuteNonQuery();

                tran.Commit();
                return null;
            }
            catch (Exception ex)
            {
                tran.Rollback();
                return "Xảy ra lỗi khi xóa tài khoản: " + ex.Message;
            }
        }


        // Extension giúp gọn hơn
        private static MySqlCommand WithParameter(this MySqlCommand cmd, string name, object value)
        {
            cmd.Parameters.AddWithValue(name, value);
            return cmd;
        }

        public static List<Lop> LayDanhSachLopHoc()
        {
            var dsLop = new List<Lop>();
            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            string query = "SELECT LopID, TenLop FROM LOP ORDER BY TenLop";

            using var cmd = new MySqlCommand(query, conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                dsLop.Add(new Lop
                {
                    LopID = reader["LopID"]?.ToString(),
                    TenLop = reader["TenLop"]?.ToString()
                });
            }

            return dsLop;
        }



    }
}



