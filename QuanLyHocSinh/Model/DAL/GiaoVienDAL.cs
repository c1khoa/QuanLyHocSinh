using System;
using System.Collections.Generic;
using System.Configuration;
using MySql.Data.MySqlClient;
using QuanLyHocSinh.Model.Entities;
using System.Configuration;

public class GiaoVienDAL
{
    public static List<GiaoVien> GetAllGiaoVien()
    {
        List<GiaoVien> list = new List<GiaoVien>();
        string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
        string query = @"SELECT gv.GiaoVienID AS MaGV, 
                                ho.HoTen, 
                                ho.NgaySinh, 
                                ho.GioiTinh, 
                                ho.Email, 
                                ho.DiaChi, 
                                GROUP_CONCAT(DISTINCT mh.TenMonHoc SEPARATOR ', ') AS BoMon,
                                GROUP_CONCAT(DISTINCT l.TenLop SEPARATOR ', ') AS LopDayID
                            FROM GIAOVIEN gv
                            JOIN HOSOGIAOVIEN hsgv ON gv.GiaoVienID = hsgv.GiaoVienID
                            JOIN HOSO ho ON hsgv.HoSoID = ho.HoSoID
                            LEFT JOIN PHANCONGDAY pcd ON pcd.GiaoVienID = gv.GiaoVienID
                            LEFT JOIN MONHOC mh ON pcd.MonHocID = mh.MonHocID
                            LEFT JOIN LOP l ON pcd.LopID = l.LopID
                            GROUP BY gv.GiaoVienID, ho.HoTen, ho.NgaySinh, ho.GioiTinh, ho.Email, ho.DiaChi
                            ORDER BY gv.GiaoVienID;
        ";

        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(query, conn);
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    GiaoVien gv = new GiaoVien
                    {
                        MaGV = reader["MaGV"].ToString(),
                        HoTen = reader["HoTen"].ToString(),
                        NgaySinh = reader["NgaySinh"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["NgaySinh"]),
                        GioiTinh = reader["GioiTinh"].ToString(),
                        Email = reader["Email"].ToString(),
                        BoMon = reader["BoMon"] == DBNull.Value ? "Chưa phân công" : reader["BoMon"].ToString(),
                        DiaChi = reader["DiaChi"].ToString(),
                        LopDayID = reader["LopDayID"] == DBNull.Value ? "Chưa phân công" : reader["LopDayID"].ToString()
                    };
                    list.Add(gv);
                }
            }
        }
        return list;
    }

    public static List<string> GetLopDayCuaUser(string userID)
    {
        List<string> danhSachLop = new List<string>();
        string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

        string query = @"
        SELECT DISTINCT l.TenLop
        FROM USERS u
        JOIN GIAOVIEN gv ON u.UserID = gv.UserID
        JOIN PHANCONGDAY pcd ON gv.GiaoVienID = pcd.GiaoVienID
        JOIN LOP l ON pcd.LopID = l.LopID
        WHERE u.UserID = @UserID;

    ";

        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@UserID", userID);

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    danhSachLop.Add(reader["TenLop"].ToString());
                }
            }
        }

        return danhSachLop;
    }

    public static List<string> GetAllLop()
        {
            List<string> list = new List<string>();
            string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
            string query = "SELECT TenLop FROM LOP ORDER BY TenLop";
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(reader["TenLop"]?.ToString() ?? "");
                    }
                }
            }
            return list;
        }

    //Cập nhật thông tin giáo viên - chỉ cập nhật thông tin cá nhân, không cập nhật phân công dạy
    public static void UpdateGiaoVien(GiaoVien giaoVien)
    {
        string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
        string query = @"UPDATE HOSO h
                            JOIN HOSOGIAOVIEN hgv ON h.HoSoID = hgv.HoSoID
                            SET h.HoTen = @HoTen, h.GioiTinh = @GioiTinh, h.NgaySinh = @NgaySinh, 
                                h.Email = @Email, h.DiaChi = @DiaChi,
                                h.NgayCapNhatGanNhat = NOW()
                            WHERE hgv.GiaoVienID = @GiaoVienID";
        using (var conn = new MySql.Data.MySqlClient.MySqlConnection(connectionString))
        {
            conn.Open();
            using (var cmd = new MySql.Data.MySqlClient.MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@HoTen", giaoVien.HoTen);
                cmd.Parameters.AddWithValue("@GioiTinh", giaoVien.GioiTinh);
                cmd.Parameters.AddWithValue("@NgaySinh", giaoVien.NgaySinh);
                cmd.Parameters.AddWithValue("@Email", giaoVien.Email);
                cmd.Parameters.AddWithValue("@DiaChi", giaoVien.DiaChi);
                cmd.Parameters.AddWithValue("@GiaoVienID", giaoVien.MaGV);
                cmd.ExecuteNonQuery();
            }
        }
    }

    // Cập nhật phân công dạy của giáo viên
    public static void UpdatePhanCongDay(string giaoVienID, List<string> danhSachLop, List<string> danhSachMonHoc)
    {
        string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
        
        using (var conn = new MySqlConnection(connectionString))
        {
            conn.Open();
            using (var transaction = conn.BeginTransaction())
            {
                try
                {
                    // Xóa tất cả phân công cũ của giáo viên
                    string deleteQuery = "DELETE FROM PHANCONGDAY WHERE GiaoVienID = @GiaoVienID";
                    using (var deleteCmd = new MySqlCommand(deleteQuery, conn, transaction))
                    {
                        deleteCmd.Parameters.AddWithValue("@GiaoVienID", giaoVienID);
                        deleteCmd.ExecuteNonQuery();
                    }

                    // Thêm phân công mới
                    if (danhSachLop != null && danhSachMonHoc != null && danhSachLop.Count > 0 && danhSachMonHoc.Count > 0)
                    {
                        // Lấy NamHocID có sẵn (nếu NH2025 không tồn tại)
                        string namHocID = "NH2025";
                        using (var checkNamHocCmd = new MySqlCommand("SELECT NamHocID FROM NAMHOC WHERE NamHocID = @NamHocID", conn, transaction))
                        {
                            checkNamHocCmd.Parameters.AddWithValue("@NamHocID", namHocID);
                            var namHocResult = checkNamHocCmd.ExecuteScalar();
                            if (namHocResult == null)
                            {
                                // Nếu NH2025 không tồn tại, lấy năm học đầu tiên có sẵn
                                using (var getFirstNamHocCmd = new MySqlCommand("SELECT NamHocID FROM NAMHOC LIMIT 1", conn, transaction))
                                {
                                    var firstNamHocResult = getFirstNamHocCmd.ExecuteScalar();
                                    namHocID = firstNamHocResult?.ToString() ?? "NH2024"; // Fallback
                                }
                            }
                        }
                        
                        // Lấy số ID hiện tại lớn nhất để tạo ID unique
                        int maxIdNumber = 0;
                        using (var getMaxIdCmd = new MySqlCommand("SELECT COALESCE(MAX(CAST(SUBSTRING(PhanCongDayID, 4) AS UNSIGNED)), 0) FROM PHANCONGDAY WHERE PhanCongDayID LIKE 'PCD%'", conn, transaction))
                        {
                            var maxIdResult = getMaxIdCmd.ExecuteScalar();
                            if (maxIdResult != null && int.TryParse(maxIdResult.ToString(), out int maxId))
                            {
                                maxIdNumber = maxId;
                            }
                        }
                        
                        int phanCongIdCounter = maxIdNumber + 1;
                        
                        foreach (string lopName in danhSachLop)
                        {
                            foreach (string monHocName in danhSachMonHoc)
                            {
                                // Kiểm tra tồn tại LopID và MonHocID trước khi insert
                                string lopID = null;
                                string monHocID = null;
                                
                                // Lấy LopID
                                using (var checkLopCmd = new MySqlCommand("SELECT LopID FROM LOP WHERE TenLop = @TenLop", conn, transaction))
                                {
                                    checkLopCmd.Parameters.AddWithValue("@TenLop", lopName);
                                    var lopResult = checkLopCmd.ExecuteScalar();
                                    lopID = lopResult?.ToString();
                                }
                                
                                // Lấy MonHocID
                                using (var checkMonCmd = new MySqlCommand("SELECT MonHocID FROM MONHOC WHERE TenMonHoc = @TenMonHoc", conn, transaction))
                                {
                                    checkMonCmd.Parameters.AddWithValue("@TenMonHoc", monHocName);
                                    var monResult = checkMonCmd.ExecuteScalar();
                                    monHocID = monResult?.ToString();
                                }
                                
                                // Chỉ insert nếu cả LopID và MonHocID tồn tại
                                if (!string.IsNullOrEmpty(lopID) && !string.IsNullOrEmpty(monHocID))
                                {
                                    // Tạo PhanCongDayID unique với 12 ký tự
                                    string phanCongDayID;
                                    bool isUnique = false;
                                    int attempts = 0;
                                    
                                    do
                                    {
                                        phanCongDayID = $"PCD{phanCongIdCounter:D9}"; // 12 ký tự: PCD + 9 số
                                        
                                        // Kiểm tra unique
                                        using (var checkUniqueCmd = new MySqlCommand("SELECT COUNT(*) FROM PHANCONGDAY WHERE PhanCongDayID = @PhanCongDayID", conn, transaction))
                                        {
                                            checkUniqueCmd.Parameters.AddWithValue("@PhanCongDayID", phanCongDayID);
                                            int count = Convert.ToInt32(checkUniqueCmd.ExecuteScalar());
                                            isUnique = (count == 0);
                                        }
                                        
                                        if (!isUnique)
                                        {
                                            phanCongIdCounter++;
                                            attempts++;
                                            if (attempts > 1000) // Tránh vòng lặp vô hạn
                                            {
                                                throw new Exception("Không thể tạo PhanCongDayID unique");
                                            }
                                        }
                                    } while (!isUnique);
                                    
                                    string insertQuery = @"INSERT INTO PHANCONGDAY (PhanCongDayID, GiaoVienID, LopID, MonHocID, NamHocID, ChuanDauRa) 
                                                         VALUES (@PhanCongDayID, @GiaoVienID, @LopID, @MonHocID, @NamHocID, @ChuanDauRa)";
                                    
                                    using (var insertCmd = new MySqlCommand(insertQuery, conn, transaction))
                                    {
                                        insertCmd.Parameters.AddWithValue("@PhanCongDayID", phanCongDayID);
                                        insertCmd.Parameters.AddWithValue("@GiaoVienID", giaoVienID);
                                        insertCmd.Parameters.AddWithValue("@LopID", lopID);
                                        insertCmd.Parameters.AddWithValue("@MonHocID", monHocID);
                                        insertCmd.Parameters.AddWithValue("@NamHocID", namHocID); // Năm học được kiểm tra
                                        insertCmd.Parameters.AddWithValue("@ChuanDauRa", ""); // Có thể để trống
                                        insertCmd.ExecuteNonQuery();
                                    }
                                    phanCongIdCounter++;
                                }
                                else
                                {
                                    throw new Exception($"Không tìm thấy lớp '{lopName}' hoặc môn học '{monHocName}' trong cơ sở dữ liệu.");
                                }
                            }
                        }
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
