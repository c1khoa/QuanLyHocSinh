using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using QuanLyHocSinh.Model.Entities;
using System.Configuration;

public class TongKetMonDAL
{
    public static List<TongKetMonItem> GetAllTongKetMon()
    {
        List<TongKetMonItem> list = new List<TongKetMonItem>();
        string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
        
        // Truy vấn tính điểm trung bình và xếp loại cho từng học sinh theo môn học
        string query = @"
            SELECT 
                ROW_NUMBER() OVER (ORDER BY hs.HocSinhID, mh.TenMonHoc, d.NamHocID, d.HocKy) as STT,
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
            LEFT JOIN CHITIETDIEM diem_mieng ON diem_mieng.DiemID = d.DiemID AND diem_mieng.LoaiDiemID = 'LD01'
            LEFT JOIN CHITIETDIEM diem_15p ON diem_15p.DiemID = d.DiemID AND diem_15p.LoaiDiemID = 'LD02'
            LEFT JOIN CHITIETDIEM diem_1tiet ON diem_1tiet.DiemID = d.DiemID AND diem_1tiet.LoaiDiemID = 'LD03'
            LEFT JOIN CHITIETDIEM diem_thi ON diem_thi.DiemID = d.DiemID AND diem_thi.LoaiDiemID = 'LD04'
            WHERE d.DiemTrungBinh IS NOT NULL AND d.DiemTrungBinh > 0
            ORDER BY hs.HocSinhID, mh.TenMonHoc, d.NamHocID, d.HocKy
        ";

        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            conn.Open();            MySqlCommand cmd = new MySqlCommand(query, conn);
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    TongKetMonItem item = new TongKetMonItem
                    {
                        STT = Convert.ToInt32(reader["STT"]),
                        HocSinhID = reader["HocSinhID"]?.ToString() ?? "",
                        HoTen = reader["HoTen"]?.ToString() ?? "",
                        TenLop = reader["TenLop"]?.ToString() ?? "",
                        MonHoc = reader["MonHoc"]?.ToString() ?? "",
                        NamHoc = reader["NamHoc"]?.ToString() ?? "",
                        HocKy = Convert.ToInt32(reader["HocKy"]),
                        DiemTrungBinh = Convert.ToDouble(reader["DiemTrungBinh"]),
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
        return list;
    }// Lấy tổng kết môn theo các bộ lọc cụ thể
    public static List<TongKetMonItem> GetTongKetMonByFilter(string? namHoc = null, string? lop = null, string? monHoc = null, int? hocKy = null)
    {
        List<TongKetMonItem> list = new List<TongKetMonItem>();
        string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
        
        string whereClause = "WHERE d.DiemTrungBinh IS NOT NULL AND d.DiemTrungBinh > 0";
        
        if (!string.IsNullOrEmpty(namHoc) && namHoc != "Tất cả")
            whereClause += " AND d.NamHocID = @NamHoc";
        if (!string.IsNullOrEmpty(lop) && lop != "Tất cả")
            whereClause += " AND l.TenLop = @Lop";
        if (!string.IsNullOrEmpty(monHoc) && monHoc != "Tất cả")
            whereClause += " AND mh.TenMonHoc = @MonHoc";
        if (hocKy.HasValue)
            whereClause += " AND d.HocKy = @HocKy";

        string query = $@"
            SELECT 
                ROW_NUMBER() OVER (ORDER BY hs.HocSinhID, mh.TenMonHoc, d.NamHocID, d.HocKy) as STT,
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
            LEFT JOIN CHITIETDIEM diem_mieng ON diem_mieng.DiemID = d.DiemID AND diem_mieng.LoaiDiemID = 'LD01'
            LEFT JOIN CHITIETDIEM diem_15p ON diem_15p.DiemID = d.DiemID AND diem_15p.LoaiDiemID = 'LD02'
            LEFT JOIN CHITIETDIEM diem_1tiet ON diem_1tiet.DiemID = d.DiemID AND diem_1tiet.LoaiDiemID = 'LD03'
            LEFT JOIN CHITIETDIEM diem_thi ON diem_thi.DiemID = d.DiemID AND diem_thi.LoaiDiemID = 'LD04'
            {whereClause}
            ORDER BY hs.HocSinhID, mh.TenMonHoc, d.NamHocID, d.HocKy
        ";

        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(query, conn);
            
            if (!string.IsNullOrEmpty(namHoc) && namHoc != "Tất cả")
                cmd.Parameters.AddWithValue("@NamHoc", namHoc);
            if (!string.IsNullOrEmpty(lop) && lop != "Tất cả")
                cmd.Parameters.AddWithValue("@Lop", lop);
            if (!string.IsNullOrEmpty(monHoc) && monHoc != "Tất cả")
                cmd.Parameters.AddWithValue("@MonHoc", monHoc);            if (hocKy.HasValue)
                cmd.Parameters.AddWithValue("@HocKy", hocKy.Value);

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    TongKetMonItem item = new TongKetMonItem
                    {
                        STT = Convert.ToInt32(reader["STT"]),
                        HocSinhID = reader["HocSinhID"]?.ToString() ?? "",
                        HoTen = reader["HoTen"]?.ToString() ?? "",
                        TenLop = reader["TenLop"]?.ToString() ?? "",
                        MonHoc = reader["MonHoc"]?.ToString() ?? "",
                        NamHoc = reader["NamHoc"]?.ToString() ?? "",
                        HocKy = Convert.ToInt32(reader["HocKy"]),
                        DiemTrungBinh = Convert.ToDouble(reader["DiemTrungBinh"]),
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
        return list;
    }    // Thống kê tổng kết theo lớp và môn học
    public static Dictionary<string, int> GetThongKeTongKet(string? namHoc = null, string? lop = null, string? monHoc = null, int? hocKy = null)
    {
        Dictionary<string, int> thongKe = new Dictionary<string, int>
        {
            ["Giỏi"] = 0,
            ["Khá"] = 0,
            ["Trung bình"] = 0,
            ["Yếu"] = 0,
            ["Kém"] = 0,
            ["Đạt"] = 0,
            ["Không đạt"] = 0
        };

        var danhSach = GetTongKetMonByFilter(namHoc, lop, monHoc, hocKy);
        
        foreach (var item in danhSach)
        {
            if (thongKe.ContainsKey(item.XepLoai))
                thongKe[item.XepLoai]++;
            
            if (thongKe.ContainsKey(item.GhiChu))
                thongKe[item.GhiChu]++;
        }        return thongKe;
    }

    // Lấy danh sách năm học
    public static List<string> GetAllNamHoc()
    {
        List<string> list = new List<string>();
        string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
        string query = "SELECT DISTINCT NamHocID FROM DIEM ORDER BY NamHocID";

        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(query, conn);            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    list.Add(reader["NamHocID"]?.ToString() ?? "");
                }
            }
        }
        return list;
    }

    // Lấy danh sách lớp
    public static List<string> GetAllLop()
    {
        List<string> list = new List<string>();
        string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
        string query = "SELECT DISTINCT TenLop FROM LOP ORDER BY TenLop";

        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(query, conn);            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    list.Add(reader["TenLop"]?.ToString() ?? "");
                }
            }
        }
        return list;
    }

    // Lấy danh sách môn học
    public static List<string> GetAllMonHoc()
    {
        List<string> list = new List<string>();
        string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
        string query = "SELECT DISTINCT TenMonHoc FROM MONHOC ORDER BY TenMonHoc";

        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(query, conn);            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    list.Add(reader["TenMonHoc"]?.ToString() ?? "");
                }
            }
        }
        return list;
    }

    // Lấy danh sách học kỳ
    public static List<int> GetAllHocKy()
    {
        List<int> list = new List<int>();
        string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
        string query = "SELECT DISTINCT HocKy FROM DIEM ORDER BY HocKy";

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
}