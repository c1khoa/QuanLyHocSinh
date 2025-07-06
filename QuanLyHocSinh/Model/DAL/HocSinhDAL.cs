using System;
using System.Collections.Generic;
using System.Configuration;
using MySql.Data.MySqlClient;
using QuanLyHocSinh.Model.Entities;

namespace QuanLyHocSinh.Model.Entities
{
    public class HocSinhDAL
    {
        public static List<HocSinh> GetAllHocSinh()
        {
            List<HocSinh> list = new List<HocSinh>();
            // Chuỗi kết nối cơ sở dữ liệu
            string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
            // Truy vấn lấy tất cả thông tin học sinh
            string query = @"
                SELECT hs.HocSinhID, ho.HoTen, ho.GioiTinh, ho.NgaySinh, ho.Email, ho.DiaChi, 
                       l.TenLop, hhs.NienKhoa, ho.TrangThaiHoSo
                FROM HOCSINH hs
                JOIN HOSOHOCSINH hhs ON hs.HocSinhID = hhs.HocSinhID
                JOIN HOSO ho ON hhs.HoSoID = ho.HoSoID
                JOIN LOP l ON hhs.LopHocID = l.LopID
            ";

            // Kết nối đến cơ sở dữ liệu
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                // Tạo lệnh SQL
                MySqlCommand cmd = new MySqlCommand(query, conn);
                // Thực thi lệnh và đọc kết quả
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {                        // Tạo đối tượng HocSinh từ kết quả truy vấn
                        HocSinh hs = new HocSinh
                        {
                            HocSinhID = reader["HocSinhID"]?.ToString() ?? "",
                            HoTen = reader["HoTen"]?.ToString() ?? "",
                            GioiTinh = reader["GioiTinh"]?.ToString() ?? "",
                            NgaySinh = Convert.ToDateTime(reader["NgaySinh"]),
                            Email = reader["Email"]?.ToString() ?? "",
                            DiaChi = reader["DiaChi"]?.ToString() ?? "",
                            TenLop = reader["TenLop"]?.ToString() ?? "",
                            NienKhoa = Convert.ToInt32(reader["NienKhoa"]),
                            TrangThaiHoSo = reader["TrangThaiHoSo"]?.ToString() ?? ""
                        };
                        list.Add(hs);
                    }
                }
            }
            return list;
        }
        public static (int soNam, int soNu) GetThongKeGioiTinh()
        {
            int nam = 0, nu = 0;
            string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                string query = @"SELECT gioitinh, COUNT(*) AS SoLuong 
                                        from hoso hs 
                                        inner join hosohocsinh hshs on hs.HoSoID = hshs.HoSoID
                                        group by gioitinh";

                using (var cmd = new MySqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string gioiTinh = reader.GetString("gioitinh");
                        int soLuong = reader.GetInt32("SoLuong");

                        if (gioiTinh.ToLower() == "nam") nam = soLuong;
                        else if (gioiTinh.ToLower() == "nữ" || gioiTinh.ToLower() == "nu") nu = soLuong;
                    }
                }
            }

            return (nam, nu);
        }
        public static List<string> GetLopHocCuaUser(string userID)
        {
            List<string> danhSachLop = new List<string>();
            string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

            string query = @"
                    SELECT DISTINCT l.TenLop
                    FROM USERS u
                    JOIN HOCSINH hs ON u.UserID = hs.UserID
                    JOIN HOSOHOCSINH hshs ON hs.HocSinhID = hshs.HocSinhID
                    JOIN LOP l ON hshs.LopHocID = l.LopID
                    WHERE u.UserID = @UserID
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

        public static List<string> GetAllNienKhoa()
        {
            List<string> list = new List<string>();
            string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
            string query = "SELECT DISTINCT NienKhoa FROM HOSOHOCSINH";
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(reader["NienKhoa"]?.ToString() ?? "");
                    }
                }
            }
            return list;        }

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

        public static bool UpdateHocSinh(HocSinh hocSinh, out string message)
        {
            message = "";
            string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                // Nếu chọn là Lớp trưởng → kiểm tra trùng
                if (hocSinh.ChucVu == "Lớp trưởng")
                {
                    string checkQuery = @"SELECT COUNT(*) 
                                  FROM HOSO h
                                  JOIN HOSOHOCSINH hhs ON h.HoSoID = hhs.HoSoID
                                  JOIN CHUCVU cv ON h.ChucVuID = cv.ChucVuID
                                  WHERE cv.TenChucVu = 'Lớp trưởng'
                                        AND hhs.LopHocID = (SELECT LopID FROM LOP WHERE TenLop = @TenLop)
                                        AND hhs.HocSinhID <> @HocSinhID";

                    using (var checkCmd = new MySqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@TenLop", hocSinh.TenLop);
                        checkCmd.Parameters.AddWithValue("@HocSinhID", hocSinh.HocSinhID);

                        int count = Convert.ToInt32(checkCmd.ExecuteScalar());
                        if (count > 0)
                        {
                            message = "Lớp này đã có lớp trưởng rồi.";
                            return false;
                        }
                    }
                }

                // Nếu không vi phạm → Cập nhật
                string updateQuery = @"UPDATE HOSO h
                               JOIN HOSOHOCSINH hhs ON h.HoSoID = hhs.HoSoID
                               SET h.HoTen = @HoTen,
                                   h.GioiTinh = @GioiTinh,
                                   h.NgaySinh = @NgaySinh,
                                   h.Email = @Email,
                                   h.DiaChi = @DiaChi,
                                   h.ChucVuID = (SELECT ChucVuID FROM CHUCVU WHERE TenChucVu = @TenChucVu),
                                   h.NgayCapNhatGanNhat = NOW(),
                                   hhs.LopHocID = (SELECT LopID FROM LOP WHERE TenLop = @TenLop),
                                   hhs.NienKhoa = @NienKhoa
                               WHERE hhs.HocSinhID = @HocSinhID";

                using (var updateCmd = new MySqlCommand(updateQuery, conn))
                {
                    updateCmd.Parameters.AddWithValue("@HoTen", hocSinh.HoTen);
                    updateCmd.Parameters.AddWithValue("@GioiTinh", hocSinh.GioiTinh);
                    updateCmd.Parameters.AddWithValue("@NgaySinh", hocSinh.NgaySinh);
                    updateCmd.Parameters.AddWithValue("@Email", hocSinh.Email);
                    updateCmd.Parameters.AddWithValue("@DiaChi", hocSinh.DiaChi);
                    updateCmd.Parameters.AddWithValue("@TenChucVu", hocSinh.ChucVu);
                    updateCmd.Parameters.AddWithValue("@TenLop", hocSinh.TenLop);
                    updateCmd.Parameters.AddWithValue("@NienKhoa", hocSinh.NienKhoa);
                    updateCmd.Parameters.AddWithValue("@HocSinhID", hocSinh.HocSinhID);
                    updateCmd.ExecuteNonQuery();
                }
            }

            return true;
        }



        public static List<string> GetAllNamHoc()
        {
            List<string> list = new List<string>();
            string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
            string query = "SELECT DISTINCT NamHocID FROM DIEM";
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(reader["NamHocID"]?.ToString() ?? "");
                    }
                }
            }
            return list;
        }
        
        public static List<int> GetAllHocKy()
        {
            List<int> list = new List<int>();
            string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
            string query = "SELECT DISTINCT HocKy FROM DIEM";
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(Convert.ToInt32(reader["HocKy"]));
                    }
                }
            }
            return list;
        }

        public static List<TongKetMonItem> GetBangDiemHocSinh_GiaoVien(string hocSinhID, string giaoVienID, string namHoc = null, int? hocKy = null)
        {
            List<TongKetMonItem> list = new List<TongKetMonItem>();
            string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

            string query = @"
                        SELECT 
                            hs.HocSinhID,
                            ho.HoTen,
                            l.TenLop,
                            mh.TenMonHoc as MonHoc,
                            d.NamHocID as NamHoc,
                            d.HocKy,
                            d.DiemTrungBinh,
                            CASE 
                                WHEN d.DiemTrungBinh >= 8.5 THEN 'Giỏi'
                                WHEN d.DiemTrungBinh >= 6.5 THEN 'Khá'
                                WHEN d.DiemTrungBinh >= 5.0 THEN 'Trung bình'
                                WHEN d.DiemTrungBinh >= 3.5 THEN 'Yếu'
                                ELSE 'Kém'
                            END as XepLoai,
                            CASE 
                                WHEN d.DiemTrungBinh >= 5.0 THEN 'Đạt'
                                ELSE 'Không đạt'
                            END as GhiChu,
                            IFNULL(diem_mieng.GiaTri, NULL) AS DiemMieng,
                            IFNULL(diem_15p.GiaTri, NULL) AS Diem15Phut,
                            IFNULL(diem_1tiet.GiaTri, NULL) AS Diem1Tiet,
                            IFNULL(diem_thi.GiaTri, NULL) AS DiemThi
                        FROM HOCSINH hs
                        JOIN HOSOHOCSINH hhs ON hs.HocSinhID = hhs.HocSinhID
                        JOIN HOSO ho ON hhs.HoSoID = ho.HoSoID
                        JOIN LOP l ON hhs.LopHocID = l.LopID
                        JOIN DIEM d ON d.HocSinhID = hs.HocSinhID
                        JOIN MONHOC mh ON d.MonHocID = mh.MonHocID
                        JOIN PHANCONGDAY pcd ON d.MonHocID = pcd.MonHocID 
                                            AND d.NamHocID = pcd.NamHocID 
                                            AND LEFT(hhs.LopHocID, 4) = pcd.LopID
                        LEFT JOIN (
                            SELECT d.DiemID, ctd.GiaTri FROM DIEM d
                            JOIN CHITIETDIEM ctd ON d.DiemID = ctd.DiemID
                            WHERE ctd.LoaiDiemID = 'LD01'
                        ) diem_mieng ON d.DiemID = diem_mieng.DiemID
                        LEFT JOIN (
                            SELECT d.DiemID, ctd.GiaTri FROM DIEM d
                            JOIN CHITIETDIEM ctd ON d.DiemID = ctd.DiemID
                            WHERE ctd.LoaiDiemID = 'LD02'
                        ) diem_15p ON d.DiemID = diem_15p.DiemID
                        LEFT JOIN (
                            SELECT d.DiemID, ctd.GiaTri FROM DIEM d
                            JOIN CHITIETDIEM ctd ON d.DiemID = ctd.DiemID
                            WHERE ctd.LoaiDiemID = 'LD03'
                        ) diem_1tiet ON d.DiemID = diem_1tiet.DiemID
                        LEFT JOIN (
                            SELECT d.DiemID, ctd.GiaTri FROM DIEM d
                            JOIN CHITIETDIEM ctd ON d.DiemID = ctd.DiemID
                            WHERE ctd.LoaiDiemID = 'LD04'
                        ) diem_thi ON d.DiemID = diem_thi.DiemID
                        WHERE hs.HocSinhID = @HocSinhID
                          AND pcd.GiaoVienID = @GiaoVienID
                          " + (namHoc != null ? " AND d.NamHocID = @NamHoc " : "") +
                                    (hocKy != null ? " AND d.HocKy = @HocKy " : "") + @"
                        ORDER BY mh.TenMonHoc";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@HocSinhID", hocSinhID);
                    cmd.Parameters.AddWithValue("@GiaoVienID", giaoVienID);
                    if (namHoc != null) cmd.Parameters.AddWithValue("@NamHoc", namHoc);
                    if (hocKy != null) cmd.Parameters.AddWithValue("@HocKy", hocKy);

                    using (var reader = cmd.ExecuteReader())
                    {
                        int stt = 1;
                        while (reader.Read())
                        {
                            TongKetMonItem item = new TongKetMonItem
                            {
                                STT = stt++,
                                HocSinhID = reader["HocSinhID"]?.ToString() ?? "",
                                HoTen = reader["HoTen"]?.ToString() ?? "",
                                TenLop = reader["TenLop"]?.ToString() ?? "",
                                MonHoc = reader["MonHoc"]?.ToString() ?? "",
                                NamHoc = reader["NamHoc"]?.ToString() ?? "",
                                HocKy = Convert.ToInt32(reader["HocKy"]),
                                DiemTrungBinh = reader["DiemTrungBinh"] == DBNull.Value ? double.NaN : Convert.ToDouble(reader["DiemTrungBinh"]),
                                XepLoai = reader["XepLoai"]?.ToString() ?? "",
                                GhiChu = reader["GhiChu"]?.ToString() ?? "",
                                DiemMieng = reader["DiemMieng"] == DBNull.Value ? double.NaN : Convert.ToDouble(reader["DiemMieng"]),
                                Diem15Phut = reader["Diem15Phut"] == DBNull.Value ? double.NaN : Convert.ToDouble(reader["Diem15Phut"]),
                                Diem1Tiet = reader["Diem1Tiet"] == DBNull.Value ? double.NaN : Convert.ToDouble(reader["Diem1Tiet"]),
                                DiemThi = reader["DiemThi"] == DBNull.Value ? double.NaN : Convert.ToDouble(reader["DiemThi"])
                            };
                            list.Add(item);
                        }
                    }
                }
            }

            return list;
        }


        public static List<TongKetMonItem> GetBangDiemHocSinh(string hocSinhID, string namHoc = null, int? hocKy = null)
        {
            List<TongKetMonItem> list = new List<TongKetMonItem>();
            string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
            string query = @"
                SELECT 
                    hs.HocSinhID,
                    ho.HoTen,
                    l.TenLop,
                    mh.TenMonHoc as MonHoc,
                    d.NamHocID as NamHoc,
                    d.HocKy,
                    d.DiemTrungBinh,
                    CASE 
                        WHEN d.DiemTrungBinh >= 8.5 THEN 'Giỏi'
                        WHEN d.DiemTrungBinh >= 6.5 THEN 'Khá'
                        WHEN d.DiemTrungBinh >= 5.0 THEN 'Trung bình'
                        WHEN d.DiemTrungBinh >= 3.5 THEN 'Yếu'
                        ELSE 'Kém'
                    END as XepLoai,
                    CASE 
                        WHEN d.DiemTrungBinh >= 5.0 THEN 'Đạt'
                        ELSE 'Không đạt'
                    END as GhiChu,
                    IFNULL(diem_mieng.GiaTri, NULL) AS DiemMieng,
                    IFNULL(diem_15p.GiaTri, NULL) AS Diem15Phut,
                    IFNULL(diem_1tiet.GiaTri, NULL) AS Diem1Tiet,
                    IFNULL(diem_thi.GiaTri, NULL) AS DiemThi
                FROM HOCSINH hs
                JOIN HOSOHOCSINH hhs ON hs.HocSinhID = hhs.HocSinhID
                JOIN HOSO ho ON hhs.HoSoID = ho.HoSoID
                JOIN LOP l ON hhs.LopHocID = l.LopID
                JOIN DIEM d ON d.HocSinhID = hs.HocSinhID
                JOIN MONHOC mh ON d.MonHocID = mh.MonHocID
                LEFT JOIN (
                    SELECT d.DiemID, ctd.GiaTri FROM DIEM d
                    JOIN CHITIETDIEM ctd ON d.DiemID = ctd.DiemID
                    WHERE ctd.LoaiDiemID = 'LD01'
                ) diem_mieng ON d.DiemID = diem_mieng.DiemID
                LEFT JOIN (
                    SELECT d.DiemID, ctd.GiaTri FROM DIEM d
                    JOIN CHITIETDIEM ctd ON d.DiemID = ctd.DiemID
                    WHERE ctd.LoaiDiemID = 'LD02'
                ) diem_15p ON d.DiemID = diem_15p.DiemID
                LEFT JOIN (
                    SELECT d.DiemID, ctd.GiaTri FROM DIEM d
                    JOIN CHITIETDIEM ctd ON d.DiemID = ctd.DiemID
                    WHERE ctd.LoaiDiemID = 'LD03'
                ) diem_1tiet ON d.DiemID = diem_1tiet.DiemID
                LEFT JOIN (
                    SELECT d.DiemID, ctd.GiaTri FROM DIEM d
                    JOIN CHITIETDIEM ctd ON d.DiemID = ctd.DiemID
                    WHERE ctd.LoaiDiemID = 'LD04'
                ) diem_thi ON d.DiemID = diem_thi.DiemID
                WHERE hs.HocSinhID = @HocSinhID
                " + (namHoc != null ? " AND d.NamHocID = @NamHoc " : "") + (hocKy != null ? " AND d.HocKy = @HocKy " : "") +
                "ORDER BY mh.TenMonHoc";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@HocSinhID", hocSinhID);
                    if (namHoc != null) cmd.Parameters.AddWithValue("@NamHoc", namHoc);
                    if (hocKy != null) cmd.Parameters.AddWithValue("@HocKy", hocKy);
                    using (var reader = cmd.ExecuteReader())
                    {
                        int stt = 1;
                        while (reader.Read())
                        {
                            TongKetMonItem item = new TongKetMonItem
                            {
                                STT = stt++,
                                HocSinhID = reader["HocSinhID"]?.ToString() ?? "",
                                HoTen = reader["HoTen"]?.ToString() ?? "",
                                TenLop = reader["TenLop"]?.ToString() ?? "",
                                MonHoc = reader["MonHoc"]?.ToString() ?? "",
                                NamHoc = reader["NamHoc"]?.ToString() ?? "",
                                HocKy = Convert.ToInt32(reader["HocKy"]),
                                DiemTrungBinh = reader["DiemTrungBinh"] == DBNull.Value ? double.NaN : Convert.ToDouble(reader["DiemTrungBinh"]),
                                XepLoai = reader["XepLoai"]?.ToString() ?? "",
                                GhiChu = reader["GhiChu"]?.ToString() ?? "",
                                DiemMieng = reader["DiemMieng"] == DBNull.Value ? double.NaN : Convert.ToDouble(reader["DiemMieng"]),
                                Diem15Phut = reader["Diem15Phut"] == DBNull.Value ? double.NaN : Convert.ToDouble(reader["Diem15Phut"]),
                                Diem1Tiet = reader["Diem1Tiet"] == DBNull.Value ? double.NaN : Convert.ToDouble(reader["Diem1Tiet"]),
                                DiemThi = reader["DiemThi"] == DBNull.Value ? double.NaN : Convert.ToDouble(reader["DiemThi"])
                            };
                            list.Add(item);
                        }
                    }
                }
            }
            return list;
        }

        public static List<HocSinhDanhSachItem> GetDanhSachHocSinhWithDiemTB(string? lop = null, string? gioiTinh = null, int? nienKhoa = null, string? namHoc = null)
        {
            List<HocSinhDanhSachItem> list = new List<HocSinhDanhSachItem>();
            string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
            
            string whereClause = "WHERE 1=1";
            if (!string.IsNullOrEmpty(lop) && lop != "Tất cả")
                whereClause += " AND l.TenLop = @Lop";
            if (!string.IsNullOrEmpty(gioiTinh) && gioiTinh != "Tất cả")
                whereClause += " AND ho.GioiTinh = @GioiTinh";
            if (nienKhoa.HasValue)
                whereClause += " AND hhs.NienKhoa = @NienKhoa";
            if (!string.IsNullOrEmpty(namHoc) && namHoc != "Tất cả")
                whereClause += " AND d.NamHocID = @NamHoc";

            string query = $@"
                SELECT DISTINCT
                    hs.HocSinhID,
                    ho.HoTen,
                    l.TenLop,
                    ho.GioiTinh,
                    ho.NgaySinh,
                    ho.Email,
                    ho.DiaChi,
                    hhs.NienKhoa,
                    COALESCE(d.NamHocID, (SELECT MAX(NamHocID) FROM DIEM)) as NamHoc,
                    -- Điểm TB HK1 (trung bình của tất cả môn trong HK1)
                    (SELECT AVG(d1.DiemTrungBinh) 
                     FROM DIEM d1 
                     WHERE d1.HocSinhID = hs.HocSinhID 
                       AND d1.HocKy = 1 
                       AND d1.NamHocID = COALESCE(d.NamHocID, (SELECT MAX(NamHocID) FROM DIEM))
                       AND d1.DiemTrungBinh IS NOT NULL
                       AND d1.DiemTrungBinh > 0) as DiemTBHK1,
                    -- Điểm TB HK2 (trung bình của tất cả môn trong HK2)
                    (SELECT AVG(d2.DiemTrungBinh) 
                     FROM DIEM d2 
                     WHERE d2.HocSinhID = hs.HocSinhID 
                       AND d2.HocKy = 2 
                       AND d2.NamHocID = COALESCE(d.NamHocID, (SELECT MAX(NamHocID) FROM DIEM))
                       AND d2.DiemTrungBinh IS NOT NULL
                       AND d2.DiemTrungBinh > 0) as DiemTBHK2
                FROM HOCSINH hs
                JOIN HOSOHOCSINH hhs ON hs.HocSinhID = hhs.HocSinhID
                JOIN HOSO ho ON hhs.HoSoID = ho.HoSoID
                JOIN LOP l ON hhs.LopHocID = l.LopID
                LEFT JOIN DIEM d ON d.HocSinhID = hs.HocSinhID
                {whereClause}
                ORDER BY l.TenLop, ho.HoTen";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(lop) && lop != "Tất cả")
                        cmd.Parameters.AddWithValue("@Lop", lop);
                    if (!string.IsNullOrEmpty(gioiTinh) && gioiTinh != "Tất cả")
                        cmd.Parameters.AddWithValue("@GioiTinh", gioiTinh);
                    if (nienKhoa.HasValue)
                        cmd.Parameters.AddWithValue("@NienKhoa", nienKhoa);
                    if (!string.IsNullOrEmpty(namHoc) && namHoc != "Tất cả")
                        cmd.Parameters.AddWithValue("@NamHoc", namHoc);

                    using (var reader = cmd.ExecuteReader())
                    {
                        int stt = 1;
                        while (reader.Read())
                        {
                            HocSinhDanhSachItem item = new HocSinhDanhSachItem
                            {
                                STT = stt++,
                                HocSinhID = reader["HocSinhID"]?.ToString() ?? "",
                                HoTen = reader["HoTen"]?.ToString() ?? "",
                                TenLop = reader["TenLop"]?.ToString() ?? "",
                                GioiTinh = reader["GioiTinh"]?.ToString() ?? "",
                                NgaySinh = Convert.ToDateTime(reader["NgaySinh"]),
                                Email = reader["Email"]?.ToString() ?? "",
                                DiaChi = reader["DiaChi"]?.ToString() ?? "",
                                NienKhoa = Convert.ToInt32(reader["NienKhoa"]),
                                NamHoc = reader["NamHoc"]?.ToString() ?? "",
                                DiemTBHK1 = reader["DiemTBHK1"] == DBNull.Value ? null : Math.Round(Convert.ToDouble(reader["DiemTBHK1"]), 2),
                                DiemTBHK2 = reader["DiemTBHK2"] == DBNull.Value ? null : Math.Round(Convert.ToDouble(reader["DiemTBHK2"]), 2)
                            };
                            list.Add(item);
                        }
                    }
                }
            }
            return list;
        }

        public static List<HocSinhLopItem> GetDanhSachHocSinhTheoLop(string tenLop)
        {
            List<HocSinhLopItem> list = new List<HocSinhLopItem>();
            string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
            
            string query = @"
                                    SELECT 
                        hs.HocSinhID,
                        ho.HoTen,
                        ho.GioiTinh,
                        ho.NgaySinh,
                        YEAR(ho.NgaySinh) as NamSinh,
                        ho.Email,
                        ho.DiaChi,
                        l.TenLop,
                        hhs.NienKhoa,
                        cv.TenChucVu
                    FROM HOCSINH hs
                    JOIN HOSOHOCSINH hhs ON hs.HocSinhID = hhs.HocSinhID
                    JOIN HOSO ho ON hhs.HoSoID = ho.HoSoID
                    JOIN LOP l ON hhs.LopHocID = l.LopID
                    LEFT JOIN CHUCVU cv ON ho.ChucVuID = cv.ChucVuID
                    WHERE l.TenLop = @TenLop
                    ORDER BY ho.HoTen;"
                    ;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@TenLop", tenLop);
                    using (var reader = cmd.ExecuteReader())
                    {
                        int stt = 1;
                        while (reader.Read())
                        {
                            HocSinhLopItem item = new HocSinhLopItem
                            {
                                STT = stt++,
                                HocSinhID = reader["HocSinhID"]?.ToString() ?? "",
                                HoTen = reader["HoTen"]?.ToString() ?? "",
                                GioiTinh = reader["GioiTinh"]?.ToString() ?? "",
                                NgaySinh = Convert.ToDateTime(reader["NgaySinh"]),
                                NamSinh = Convert.ToInt32(reader["NamSinh"]),
                                Email = reader["Email"]?.ToString() ?? "",
                                DiaChi = reader["DiaChi"]?.ToString() ?? "",
                                TenLop = reader["TenLop"]?.ToString() ?? "",
                                NienKhoa = Convert.ToInt32(reader["NienKhoa"]),
                                ChucVu = reader["TenChucVu"]?.ToString() ?? ""
                            };
                            list.Add(item);
                        }
                    }
                }
            }
            return list;
        }

        public static int GetSiSoLop(string tenLop)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
            
            string query = @"
                SELECT COUNT(*) as SiSo
                FROM HOCSINH hs
                JOIN HOSOHOCSINH hhs ON hs.HocSinhID = hhs.HocSinhID
                JOIN LOP l ON hhs.LopHocID = l.LopID
                WHERE l.TenLop = @TenLop";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@TenLop", tenLop);
                    var result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : 0;
                }
            }
        }
    }
}