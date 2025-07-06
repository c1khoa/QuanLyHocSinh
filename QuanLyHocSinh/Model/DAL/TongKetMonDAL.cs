using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using QuanLyHocSinh.Model.Entities;
using System.Configuration;
using System.Linq;

public class TongKetMonDAL
{
    public static List<TongKetMonItem> GetAllTongKetMon()
    {
        List<TongKetMonItem> list = new List<TongKetMonItem>();
        string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
        QuyDinhEntities quyDinh = QuyDinhDAL.GetQuyDinh();
        float diemdat = quyDinh.DiemDat;

        // Truy vấn tính điểm trung bình và xếp loại cho từng học sinh theo môn học
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
                    WHEN d.DiemTrungBinh >= {diemdat} THEN 'Đạt'
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
        QuyDinhEntities quyDinh = QuyDinhDAL.GetQuyDinh();
        float diemdat = quyDinh.DiemDat;

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
                    WHEN d.DiemTrungBinh >= {diemdat} THEN 'Đạt'
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


    public static List<TongKetLopItem> GetTongKetTheoLop(string? monHoc = null, int? hocKy = null)
    {
        List<TongKetLopItem> list = new List<TongKetLopItem>();
        string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
        QuyDinhEntities quyDinh = QuyDinhDAL.GetQuyDinh();
        float diemdat = quyDinh.DiemDat;

        string whereClause = "WHERE d.DiemTrungBinh IS NOT NULL AND d.DiemTrungBinh > 0";

        if (!string.IsNullOrEmpty(monHoc) && monHoc != "Tất cả")
            whereClause += " AND mh.TenMonHoc = @MonHoc";
        if (hocKy.HasValue)
            whereClause += " AND d.HocKy = @HocKy";

        string groupByClause;
        string selectClause;

        if (!hocKy.HasValue)
        {
            groupByClause = "GROUP BY l.TenLop, l.SiSo, d.NamHocID";
            selectClause = $@"
            SELECT 
                ROW_NUMBER() OVER (ORDER BY l.TenLop) as STT,
                l.TenLop,
                COALESCE(MAX(mh.TenMonHoc), 'Tất cả môn') as MonHoc,
                d.NamHocID as NamHoc,
                -1 as HocKy,
                l.SiSo,
                COUNT(DISTINCT CASE WHEN d.DiemTrungBinh >= {diemdat} THEN d.HocSinhID END) as SoLuongDat,
                ROUND((COUNT(DISTINCT CASE WHEN d.DiemTrungBinh >= {diemdat} THEN d.HocSinhID END) * 100.0 / l.SiSo), 2) as TiLeDat";
        }
        else
        {
            groupByClause = "GROUP BY l.TenLop, l.SiSo, d.NamHocID, d.HocKy";
            selectClause = $@"
            SELECT 
                ROW_NUMBER() OVER (ORDER BY l.TenLop) as STT,
                l.TenLop,
                COALESCE(MAX(mh.TenMonHoc), 'Tất cả môn') as MonHoc,
                d.NamHocID as NamHoc,
                d.HocKy,
                l.SiSo,
                COUNT(DISTINCT CASE WHEN d.DiemTrungBinh >= {diemdat} THEN d.HocSinhID END) as SoLuongDat,
                ROUND((COUNT(DISTINCT CASE WHEN d.DiemTrungBinh >= {diemdat} THEN d.HocSinhID END) * 100.0 / l.SiSo), 2) as TiLeDat";
        }

        string query = $@"
        {selectClause}
        FROM DIEM d
        LEFT JOIN HOCSINH hs ON d.HocSinhID = hs.HocSinhID
        LEFT JOIN HOSOHOCSINH hhs ON hs.HocSinhID = hhs.HocSinhID
        LEFT JOIN LOP l ON LEFT(hhs.LopHocID, 4) = l.LopID
        LEFT JOIN MONHOC mh ON d.MonHocID = mh.MonHocID
        {whereClause}
        {groupByClause}
        HAVING l.SiSo > 0
        ORDER BY l.TenLop
    ";

        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(query, conn);

            if (!string.IsNullOrEmpty(monHoc) && monHoc != "Tất cả")
                cmd.Parameters.AddWithValue("@MonHoc", monHoc);
            if (hocKy.HasValue)
                cmd.Parameters.AddWithValue("@HocKy", hocKy.Value);

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    TongKetLopItem item = new TongKetLopItem
                    {
                        STT = Convert.ToInt32(reader["STT"]),
                        TenLop = reader["TenLop"]?.ToString() ?? "",
                        MonHoc = reader["MonHoc"]?.ToString() ?? "",
                        NamHoc = reader["NamHoc"]?.ToString() ?? "",
                        HocKy = Convert.ToInt32(reader["HocKy"]),
                        SiSo = Convert.ToInt32(reader["SiSo"]),
                        SoLuongDat = Convert.ToInt32(reader["SoLuongDat"]),
                        TiLeDat = Convert.ToDouble(reader["TiLeDat"])
                    };
                    list.Add(item);
                }
            }
        }
        return list;
    }
    public static List<TongKetLopItem> GetTongKetTheoLop_GiaoVien(string giaoVienID, string? monHoc = null, int? hocKy = null)
    {
        List<TongKetLopItem> list = new List<TongKetLopItem>();
        string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
        QuyDinhEntities quyDinh = QuyDinhDAL.GetQuyDinh();
        float diemdat = quyDinh.DiemDat;

        string whereClause = @"
        JOIN PHANCONGDAY pcd ON d.MonHocID = pcd.MonHocID AND LEFT(hhs.LopHocID, 4) = pcd.LopID AND d.NamHocID = pcd.NamHocID
        WHERE d.DiemTrungBinh IS NOT NULL AND d.DiemTrungBinh > 0
          AND pcd.GiaoVienID = @GiaoVienID
    ";

        if (!string.IsNullOrEmpty(monHoc) && monHoc != "Tất cả")
            whereClause += " AND mh.TenMonHoc = @MonHoc";
        if (hocKy.HasValue)
            whereClause += " AND d.HocKy = @HocKy";

        string groupByClause;
        string selectClause;

        if (!hocKy.HasValue)
        {
            groupByClause = "GROUP BY l.TenLop, l.SiSo, d.NamHocID";
            selectClause = $@"
        SELECT 
            ROW_NUMBER() OVER (ORDER BY l.TenLop) as STT,
            l.TenLop,
            COALESCE(MAX(mh.TenMonHoc), 'Tất cả môn') as MonHoc,
            d.NamHocID as NamHoc,
            -1 as HocKy,
            l.SiSo,
            COUNT(DISTINCT CASE WHEN d.DiemTrungBinh >= {diemdat} THEN d.HocSinhID END) as SoLuongDat,
            ROUND((COUNT(DISTINCT CASE WHEN d.DiemTrungBinh >= {diemdat} THEN d.HocSinhID END) * 100.0 / l.SiSo), 2) as TiLeDat";
        }
        else
        {
            groupByClause = "GROUP BY l.TenLop, l.SiSo, d.NamHocID, d.HocKy";
            selectClause = $@"
        SELECT 
            ROW_NUMBER() OVER (ORDER BY l.TenLop) as STT,
            l.TenLop,
            COALESCE(MAX(mh.TenMonHoc), 'Tất cả môn') as MonHoc,
            d.NamHocID as NamHoc,
            d.HocKy,
            l.SiSo,
            COUNT(DISTINCT CASE WHEN d.DiemTrungBinh >= {diemdat} THEN d.HocSinhID END) as SoLuongDat,
            ROUND((COUNT(DISTINCT CASE WHEN d.DiemTrungBinh >= {diemdat} THEN d.HocSinhID END) * 100.0 / l.SiSo), 2) as TiLeDat";
        }

        string query = $@"
        {selectClause}
        FROM DIEM d
        JOIN HOCSINH hs ON d.HocSinhID = hs.HocSinhID
        JOIN HOSOHOCSINH hhs ON hs.HocSinhID = hhs.HocSinhID
        JOIN LOP l ON LEFT(hhs.LopHocID, 4) = l.LopID
        JOIN MONHOC mh ON d.MonHocID = mh.MonHocID
        {whereClause}
        {groupByClause}
        HAVING l.SiSo > 0
        ORDER BY l.TenLop
    ";

        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@GiaoVienID", giaoVienID);
            if (!string.IsNullOrEmpty(monHoc) && monHoc != "Tất cả")
                cmd.Parameters.AddWithValue("@MonHoc", monHoc);
            if (hocKy.HasValue)
                cmd.Parameters.AddWithValue("@HocKy", hocKy.Value);

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    TongKetLopItem item = new TongKetLopItem
                    {
                        STT = Convert.ToInt32(reader["STT"]),
                        TenLop = reader["TenLop"]?.ToString() ?? "",
                        MonHoc = reader["MonHoc"]?.ToString() ?? "",
                        NamHoc = reader["NamHoc"]?.ToString() ?? "",
                        HocKy = Convert.ToInt32(reader["HocKy"]),
                        SiSo = Convert.ToInt32(reader["SiSo"]),
                        SoLuongDat = Convert.ToInt32(reader["SoLuongDat"]),
                        TiLeDat = Convert.ToDouble(reader["TiLeDat"])
                    };
                    list.Add(item);
                }
            }
        }

        return list;
    }

    public static List<string> GetMonHocTheoGiaoVien(string giaoVienId)
    {
        List<string> dsMon = new List<string>();
        string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            conn.Open();
            string query = @"
            SELECT DISTINCT mh.TenMonHoc
            FROM PHANCONGDAY pcd
            JOIN MONHOC mh ON pcd.MonHocID = mh.MonHocID
            WHERE pcd.GiaoVienID = @GiaoVienID
        ";

            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@GiaoVienID", giaoVienId);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        dsMon.Add(reader.GetString("TenMonHoc"));
                    }
                }
            }
        }

        return dsMon.OrderBy(m => m).ToList();
    }


    public static List<HocSinhChiTietItem> GetHocSinhTrongLop(string tenLop, string monHoc, int? hocKy, string? namHoc = null)
    {
        List<HocSinhChiTietItem> list = new List<HocSinhChiTietItem>();
        string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;


        string whereClause = @"WHERE l.TenLop = @TenLop 
                              AND mh.TenMonHoc = @MonHoc 
                              AND d.DiemTrungBinh IS NOT NULL 
                              AND d.DiemTrungBinh > 0";
        
        if (hocKy.HasValue)
            whereClause += " AND d.HocKy = @HocKy";
        
        if (!string.IsNullOrEmpty(namHoc))
            whereClause += " AND d.NamHocID = @NamHoc";

        string query = $@"
            SELECT 
                ROW_NUMBER() OVER (ORDER BY ho.HoTen, d.HocKy) as STT,
                hs.HocSinhID,
                ho.HoTen,
                d.HocKy,
                IFNULL(diem_15p.GiaTri, 0) AS Diem15Phut,
                IFNULL(diem_1tiet.GiaTri, 0) AS Diem1Tiet,
                d.DiemTrungBinh,
                CASE 
                    WHEN d.DiemTrungBinh >= 8.5 THEN 'Giỏi'
                    WHEN d.DiemTrungBinh >= 6.5 THEN 'Khá'
                    WHEN d.DiemTrungBinh >= 5.0 THEN 'Trung bình'
                    WHEN d.DiemTrungBinh >= 3.5 THEN 'Yếu'
                    ELSE 'Kém'
                END as XepLoai
            FROM HOCSINH hs
            JOIN DIEM d ON d.HocSinhID = hs.HocSinhID
            JOIN MONHOC mh ON d.MonHocID = mh.MonHocID
            JOIN HOSOHOCSINH hhs ON hs.HocSinhID = hhs.HocSinhID
            JOIN HOSO ho ON hhs.HoSoID = ho.HoSoID
            JOIN LOP l ON LEFT(hhs.LopHocID, 4) = l.LopID
            LEFT JOIN CHITIETDIEM diem_15p ON diem_15p.DiemID = d.DiemID AND diem_15p.LoaiDiemID = 'LD02'
            LEFT JOIN CHITIETDIEM diem_1tiet ON diem_1tiet.DiemID = d.DiemID AND diem_1tiet.LoaiDiemID = 'LD03'
            {whereClause}
            ORDER BY ho.HoTen, d.HocKy
        ";

        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(query, conn);
            
            cmd.Parameters.AddWithValue("@TenLop", tenLop);
            cmd.Parameters.AddWithValue("@MonHoc", monHoc);
            if (hocKy.HasValue)
                cmd.Parameters.AddWithValue("@HocKy", hocKy.Value);
            if (!string.IsNullOrEmpty(namHoc))
                cmd.Parameters.AddWithValue("@NamHoc", namHoc);

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    HocSinhChiTietItem item = new HocSinhChiTietItem
                    {
                        STT = Convert.ToInt32(reader["STT"]),
                        HocSinhID = reader["HocSinhID"]?.ToString() ?? "",
                        HoTen = reader["HoTen"]?.ToString() ?? "",
                        HocKy = Convert.ToInt32(reader["HocKy"]),
                        Diem15Phut = reader["Diem15Phut"] == DBNull.Value ? 0 : Convert.ToDouble(reader["Diem15Phut"]),
                        Diem1Tiet = reader["Diem1Tiet"] == DBNull.Value ? 0 : Convert.ToDouble(reader["Diem1Tiet"]),
                        DiemTrungBinh = Convert.ToDouble(reader["DiemTrungBinh"]),
                        XepLoai = reader["XepLoai"]?.ToString() ?? ""
                    };
                    list.Add(item);
                }
            }
        }
        return list;
    }

    public static (int TongSoLop, int TongSoHocSinh, int TongSoDat, double TiLeDatChung) GetThongKeTongHopTheoLop(string? monHoc = null, int? hocKy = null)
    {
        string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
        QuyDinhEntities quyDinh = QuyDinhDAL.GetQuyDinh();
        float diemdat = quyDinh.DiemDat;
        string whereClause = "WHERE d.DiemTrungBinh IS NOT NULL AND d.DiemTrungBinh > 0";
        
        if (!string.IsNullOrEmpty(monHoc) && monHoc != "Tất cả")
            whereClause += " AND mh.TenMonHoc = @MonHoc";
        if (hocKy.HasValue)
            whereClause += " AND d.HocKy = @HocKy";

        string query = $@"
            SELECT 
                COUNT(DISTINCT l.LopID) as TongSoLop,
                (SELECT SUM(lop_distinct.SiSo) 
                 FROM (SELECT DISTINCT l2.LopID, l2.SiSo 
                       FROM DIEM d2
                       LEFT JOIN HOCSINH hs2 ON d2.HocSinhID = hs2.HocSinhID
                       LEFT JOIN HOSOHOCSINH hhs2 ON hs2.HocSinhID = hhs2.HocSinhID
                       LEFT JOIN LOP l2 ON LEFT(hhs2.LopHocID, 4) = l2.LopID
                       LEFT JOIN MONHOC mh2 ON d2.MonHocID = mh2.MonHocID
                       {whereClause.Replace("d.", "d2.").Replace("mh.", "mh2.").Replace("l.", "l2.")}
                       AND l2.LopID IS NOT NULL) as lop_distinct
                ) as TongSoHocSinh,
                COUNT(DISTINCT CASE WHEN d.DiemTrungBinh >= {diemdat} THEN d.HocSinhID END) as TongSoDat
            FROM DIEM d
            LEFT JOIN HOCSINH hs ON d.HocSinhID = hs.HocSinhID
            LEFT JOIN HOSOHOCSINH hhs ON hs.HocSinhID = hhs.HocSinhID
            LEFT JOIN LOP l ON LEFT(hhs.LopHocID, 4) = l.LopID
            LEFT JOIN MONHOC mh ON d.MonHocID = mh.MonHocID
            {whereClause}
            AND l.LopID IS NOT NULL
        ";

        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(query, conn);

            
            if (!string.IsNullOrEmpty(monHoc) && monHoc != "Tất cả")
            {
                cmd.Parameters.AddWithValue("@MonHoc", monHoc);
            }
            if (hocKy.HasValue)
            {
                cmd.Parameters.AddWithValue("@HocKy", hocKy.Value);
            }

            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    int tongSoLop = Convert.ToInt32(reader["TongSoLop"]);
                    int tongSoHocSinh = reader["TongSoHocSinh"] == DBNull.Value ? 0 : Convert.ToInt32(reader["TongSoHocSinh"]);
                    int tongSoDat = Convert.ToInt32(reader["TongSoDat"]);
                    double tiLeDatChung = tongSoHocSinh > 0 ? Math.Round((tongSoDat * 100.0) / tongSoHocSinh, 2) : 0;
                    
                    return (tongSoLop, tongSoHocSinh, tongSoDat, tiLeDatChung);
                }
            }
        }
        
        return (0, 0, 0, 0.0);
    }
    public static List<TongKetLopItem> GetTongKetHocKyTheoLop(string? namHoc = null, int? hocKy = null)
    {
        List<TongKetLopItem> list = new List<TongKetLopItem>();
        string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
        QuyDinhEntities quyDinh = QuyDinhDAL.GetQuyDinh();
        float diemdat = quyDinh.DiemDat;

        string whereClause = "";
        if (!string.IsNullOrEmpty(namHoc) && namHoc != "Tất cả")
            whereClause += " AND d.NamHocID = @NamHoc";
        if (hocKy.HasValue)
            whereClause += " AND d.HocKy = @HocKy";

        string query = $@"
        SELECT 
            l.TenLop,
            COALESCE(d.NamHocID, '') as NamHoc,
            COALESCE(d.HocKy, 0) as HocKy,
            l.SiSo,
            COUNT(DISTINCT hs.HocSinhID) as TongSoHocSinh,
            COUNT(DISTINCT CASE 
                WHEN NOT EXISTS (
                    SELECT 1 FROM DIEM d2
                    WHERE d2.HocSinhID = hs.HocSinhID
                      AND d2.HocKy = d.HocKy
                      AND d2.NamHocID = d.NamHocID
                      AND (d2.DiemTrungBinh IS NULL OR d2.DiemTrungBinh < {diemdat})
                )
                THEN hs.HocSinhID
            END) as SoLuongDat,
            ROUND((
                COUNT(DISTINCT CASE 
                    WHEN NOT EXISTS (
                        SELECT 1 FROM DIEM d2
                        WHERE d2.HocSinhID = hs.HocSinhID
                          AND d2.HocKy = d.HocKy
                          AND d2.NamHocID = d.NamHocID
                          AND (d2.DiemTrungBinh IS NULL OR d2.DiemTrungBinh < {diemdat})
                    )
                    THEN hs.HocSinhID
                END) * 100.0 / l.SiSo), 2
            ) as TiLeDat
        FROM DIEM d
        INNER JOIN HOCSINH hs ON d.HocSinhID = hs.HocSinhID
        INNER JOIN HOSOHOCSINH hhs ON hs.HocSinhID = hhs.HocSinhID
        INNER JOIN LOP l ON LEFT(hhs.LopHocID, 4) = l.LopID
        WHERE d.DiemTrungBinh IS NOT NULL AND d.DiemTrungBinh > 0 {whereClause}
        GROUP BY l.TenLop, l.SiSo, d.NamHocID, d.HocKy
        HAVING l.SiSo > 0
        ORDER BY l.TenLop;
    ";

        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(query, conn);

            if (!string.IsNullOrEmpty(namHoc) && namHoc != "Tất cả")
                cmd.Parameters.AddWithValue("@NamHoc", namHoc);
            if (hocKy.HasValue)
                cmd.Parameters.AddWithValue("@HocKy", hocKy.Value);

            using (var reader = cmd.ExecuteReader())
            {
                int stt = 1;
                while (reader.Read())
                {
                    TongKetLopItem item = new TongKetLopItem
                    {
                        STT = stt++,
                        TenLop = reader["TenLop"]?.ToString() ?? "",
                        MonHoc = "", // Không phân biệt môn học cho tổng kết học kỳ
                        NamHoc = reader["NamHoc"]?.ToString() ?? "",
                        HocKy = Convert.ToInt32(reader["HocKy"]),
                        SiSo = Convert.ToInt32(reader["SiSo"]),
                        SoLuongDat = Convert.ToInt32(reader["SoLuongDat"]),
                        TiLeDat = Convert.ToDouble(reader["TiLeDat"])
                    };
                    list.Add(item);
                }
            }
        }

        return list;
    }

}