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
        string query = @"SELECT 
                        gv.GiaoVienID AS MaGV, 
                        ho.HoTen, 
                        ho.NgaySinh, 
                        ho.GioiTinh, 
                        ho.Email, 
                        ho.DiaChi, 
                        cv.TenChucVu,
                        cv.VaiTroID, -- 🆕 Thêm VaiTroID
                        GROUP_CONCAT(DISTINCT mh.TenMonHoc SEPARATOR ', ') AS BoMon,
                        lop_giangday.LopDay AS LopGiangDay,
                        lop_chunhiem.TenLop AS LopChuNhiem
                    FROM GIAOVIEN gv
                    JOIN HOSOGIAOVIEN hsgv ON gv.GiaoVienID = hsgv.GiaoVienID
                    JOIN HOSO ho ON hsgv.HoSoID = ho.HoSoID
                    JOIN CHUCVU cv ON ho.ChucVuID = cv.ChucVuID

                    LEFT JOIN PHANCONGDAY pcd ON pcd.GiaoVienID = gv.GiaoVienID
                    LEFT JOIN MONHOC mh ON pcd.MonHocID = mh.MonHocID

                    LEFT JOIN LOP lop_chunhiem ON gv.GiaoVienID = lop_chunhiem.GVCNID

                    LEFT JOIN (
                        SELECT 
                            gd.GiaoVienID,
                            GROUP_CONCAT(DISTINCT l.TenLop SEPARATOR ', ') AS LopDay
                        FROM (
                            SELECT GiaoVienID, LopID FROM PHANCONGDAY
                            UNION
                            SELECT GVCNID AS GiaoVienID, LopID FROM LOP
                        ) gd
                        JOIN LOP l ON gd.LopID = l.LopID
                        GROUP BY gd.GiaoVienID
                    ) lop_giangday ON gv.GiaoVienID = lop_giangday.GiaoVienID

                    GROUP BY 
                        gv.GiaoVienID, ho.HoTen, ho.NgaySinh, ho.GioiTinh, ho.Email, ho.DiaChi, 
                        cv.TenChucVu, cv.VaiTroID, 
                        lop_chunhiem.TenLop, lop_giangday.LopDay

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
                        LopDayID = reader["LopGiangDay"] == DBNull.Value ? "Chưa phân công" : reader["LopGiangDay"].ToString(),
                        LopChuNhiemID = reader["LopChuNhiem"] == DBNull.Value ? "Không" : reader["LopChuNhiem"].ToString(),
                        ChucVu = reader["TenChucVu"].ToString(),
                        VaiTroID = reader["VaiTroID"].ToString()
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
    public static void ClearLopChuNhiem(string giaoVienID)
    {
        string query = "UPDATE LOP SET GVCNID = NULL WHERE GVCNID = @GiaoVienID";
        using (var conn = new MySqlConnection(ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString))
        {
            conn.Open();
            using (var cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@GiaoVienID", giaoVienID);
                cmd.ExecuteNonQuery();
            }
        }
    }
    public static bool CapNhatLopChuNhiem(string giaoVienID, string tenLop, out string? loi)
    {
        loi = null;

        using (var conn = new MySqlConnection(ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString))
        {
            conn.Open();

            // B1: Lấy LopID từ TenLop
            string? lopID = null;
            string layLopIDQuery = "SELECT LopID FROM LOP WHERE TenLop = @TenLop";
            using (var cmd = new MySqlCommand(layLopIDQuery, conn))
            {
                cmd.Parameters.AddWithValue("@TenLop", tenLop);
                var result = cmd.ExecuteScalar();
                if (result == null)
                {
                    loi = "Không tìm thấy lớp có tên '" + tenLop + "'.";
                    return false;
                }
                lopID = result.ToString();
            }

            // B2: Kiểm tra lớp đã có GVCN khác chưa
            string kiemTraQuery = "SELECT GVCNID FROM LOP WHERE LopID = @LopID";
            using (var cmd = new MySqlCommand(kiemTraQuery, conn))
            {
                cmd.Parameters.AddWithValue("@LopID", lopID);
                var gvcnHienTai = cmd.ExecuteScalar() as string;

                if (!string.IsNullOrEmpty(gvcnHienTai) && gvcnHienTai != giaoVienID)
                {
                    loi = "Lớp đã có giáo viên chủ nhiệm khác!";
                    return false;
                }
            }

            // B3: Cập nhật GVCN nếu hợp lệ
            string capNhatQuery = "UPDATE LOP SET GVCNID = @GiaoVienID WHERE LopID = @LopID";
            using (var cmd = new MySqlCommand(capNhatQuery, conn))
            {
                cmd.Parameters.AddWithValue("@GiaoVienID", giaoVienID);
                cmd.Parameters.AddWithValue("@LopID", lopID);
                cmd.ExecuteNonQuery();
            }
        }

        return true;
    }





    //Cập nhật thông tin giáo viên - chỉ cập nhật thông tin cá nhân, không cập nhật phân công dạy
    public static void UpdateGiaoVien(GiaoVien giaoVien)
    {
        string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

        using (var conn = new MySql.Data.MySqlClient.MySqlConnection(connectionString))
        {
            conn.Open();

            // Lấy ChucVuID từ TenChucVu
            string getChucVuIdQuery = "SELECT ChucVuID FROM CHUCVU WHERE TenChucVu = @TenChucVu";
            string? chucVuID = null;

            using (var cmdGet = new MySql.Data.MySqlClient.MySqlCommand(getChucVuIdQuery, conn))
            {
                cmdGet.Parameters.AddWithValue("@TenChucVu", giaoVien.ChucVu);
                var result = cmdGet.ExecuteScalar();
                if (result != null)
                    chucVuID = result.ToString();
                else
                    throw new Exception("Không tìm thấy ChucVuID tương ứng với tên chức vụ đã truyền.");
            }

            // Cập nhật thông tin giáo viên
            string updateQuery = @"
            UPDATE HOSO h
            JOIN HOSOGIAOVIEN hgv ON h.HoSoID = hgv.HoSoID
            SET h.HoTen = @HoTen,
                h.GioiTinh = @GioiTinh,
                h.NgaySinh = @NgaySinh,
                h.Email = @Email,
                h.DiaChi = @DiaChi,
                h.ChucVuID = @ChucVuID,
                h.NgayCapNhatGanNhat = NOW()
            WHERE hgv.GiaoVienID = @GiaoVienID";

            using (var cmdUpdate = new MySql.Data.MySqlClient.MySqlCommand(updateQuery, conn))
            {
                cmdUpdate.Parameters.AddWithValue("@HoTen", giaoVien.HoTen);
                cmdUpdate.Parameters.AddWithValue("@GioiTinh", giaoVien.GioiTinh);
                cmdUpdate.Parameters.AddWithValue("@NgaySinh", giaoVien.NgaySinh);
                cmdUpdate.Parameters.AddWithValue("@Email", giaoVien.Email);
                cmdUpdate.Parameters.AddWithValue("@DiaChi", giaoVien.DiaChi);
                cmdUpdate.Parameters.AddWithValue("@ChucVuID", chucVuID);
                cmdUpdate.Parameters.AddWithValue("@GiaoVienID", giaoVien.MaGV);
                cmdUpdate.ExecuteNonQuery();
            }
        }
    }


    // Cập nhật phân công dạy của giáo viên
    public static bool UpdatePhanCongDay(string giaoVienID, List<string> danhSachLop, List<string> danhSachMonHoc, out string message)
    {
        message = "";
        string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

        try
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Xóa tất cả phân công cũ
                        string deleteQuery = "DELETE FROM PHANCONGDAY WHERE GiaoVienID = @GiaoVienID";
                        using (var deleteCmd = new MySqlCommand(deleteQuery, conn, transaction))
                        {
                            deleteCmd.Parameters.AddWithValue("@GiaoVienID", giaoVienID);
                            deleteCmd.ExecuteNonQuery();
                        }

                        if (danhSachLop != null && danhSachMonHoc != null && danhSachLop.Count > 0 && danhSachMonHoc.Count > 0)
                        {
                            // Lấy NamHocID
                            string namHocID = "NH2025";
                            using (var checkNamHocCmd = new MySqlCommand("SELECT NamHocID FROM NAMHOC WHERE NamHocID = @NamHocID", conn, transaction))
                            {
                                checkNamHocCmd.Parameters.AddWithValue("@NamHocID", namHocID);
                                var namHocResult = checkNamHocCmd.ExecuteScalar();
                                if (namHocResult == null)
                                {
                                    using (var getFirstNamHocCmd = new MySqlCommand("SELECT NamHocID FROM NAMHOC LIMIT 1", conn, transaction))
                                    {
                                        var firstNamHocResult = getFirstNamHocCmd.ExecuteScalar();
                                        namHocID = firstNamHocResult?.ToString() ?? "NH2024";
                                    }
                                }
                            }

                            // Lấy ID cao nhất hiện tại
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

                                    if (!string.IsNullOrEmpty(lopID) && !string.IsNullOrEmpty(monHocID))
                                    {
                                        // Kiểm tra trùng môn - lớp
                                        string checkConflictQuery = @"
                                        SELECT GiaoVienID FROM PHANCONGDAY 
                                        WHERE LopID = @LopID AND MonHocID = @MonHocID 
                                        AND NamHocID = @NamHocID AND GiaoVienID <> @GiaoVienID";

                                        using (var checkConflictCmd = new MySqlCommand(checkConflictQuery, conn, transaction))
                                        {
                                            checkConflictCmd.Parameters.AddWithValue("@LopID", lopID);
                                            checkConflictCmd.Parameters.AddWithValue("@MonHocID", monHocID);
                                            checkConflictCmd.Parameters.AddWithValue("@NamHocID", namHocID);
                                            checkConflictCmd.Parameters.AddWithValue("@GiaoVienID", giaoVienID);

                                            var conflictResult = checkConflictCmd.ExecuteScalar();
                                            if (conflictResult != null)
                                            {
                                                message = $"⚠️ Lớp {lopName} đã có giáo viên khác dạy môn {monHocName}.";
                                                transaction.Rollback();
                                                return false;
                                            }
                                        }

                                        // Tạo ID
                                        string phanCongDayID;
                                        bool isUnique = false;
                                        int attempts = 0;

                                        do
                                        {
                                            phanCongDayID = $"PCD{phanCongIdCounter:D9}";
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
                                                if (attempts > 1000)
                                                {
                                                    message = "Không thể tạo mã phân công duy nhất sau nhiều lần thử.";
                                                    transaction.Rollback();
                                                    return false;
                                                }
                                            }
                                        } while (!isUnique);

                                        // Thêm bản ghi
                                        string insertQuery = @"
                                        INSERT INTO PHANCONGDAY 
                                        (PhanCongDayID, GiaoVienID, LopID, MonHocID, NamHocID, ChuanDauRa)
                                        VALUES 
                                        (@PhanCongDayID, @GiaoVienID, @LopID, @MonHocID, @NamHocID, @ChuanDauRa)";

                                        using (var insertCmd = new MySqlCommand(insertQuery, conn, transaction))
                                        {
                                            insertCmd.Parameters.AddWithValue("@PhanCongDayID", phanCongDayID);
                                            insertCmd.Parameters.AddWithValue("@GiaoVienID", giaoVienID);
                                            insertCmd.Parameters.AddWithValue("@LopID", lopID);
                                            insertCmd.Parameters.AddWithValue("@MonHocID", monHocID);
                                            insertCmd.Parameters.AddWithValue("@NamHocID", namHocID);
                                            insertCmd.Parameters.AddWithValue("@ChuanDauRa", "");
                                            insertCmd.ExecuteNonQuery();
                                        }

                                        phanCongIdCounter++;
                                    }
                                    else
                                    {
                                        message = $"Không tìm thấy lớp '{lopName}' hoặc môn học '{monHocName}' trong CSDL.";
                                        transaction.Rollback();
                                        return false;
                                    }
                                }
                            }
                        }

                        transaction.Commit();
                        message = "✅ Cập nhật phân công giảng dạy thành công.";
                        return true;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        message = $"Lỗi khi cập nhật phân công: {ex.Message}";
                        return false;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            message = $"Không thể kết nối cơ sở dữ liệu: {ex.Message}";
            return false;
        }
    }

}
