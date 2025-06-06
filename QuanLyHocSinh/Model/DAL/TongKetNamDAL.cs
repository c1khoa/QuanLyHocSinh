using System;
using System.Collections.Generic;
using System.Configuration;
using MySql.Data.MySqlClient;
using QuanLyHocSinh.Model.Entities;

namespace QuanLyHocSinh.Model.Entities
{
    public static class TongKetNamDAL
    {
        public static List<TongKetNamHocItem> GetTongKetNamHoc(string? namHoc, string? lop, int? hocKy)
        {
            var list = new List<TongKetNamHocItem>();
            string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
            string where = "WHERE 1=1 ";
            if (!string.IsNullOrEmpty(namHoc) && namHoc != "Tất cả")
                where += " AND d.NamHocID = @NamHoc ";
            if (!string.IsNullOrEmpty(lop) && lop != "Tất cả")
                where += " AND l.TenLop = @Lop ";
            if (hocKy.HasValue && hocKy.Value != 0)
                where += " AND d.HocKy = @HocKy ";
            string query = $@"
                SELECT 
                    ROW_NUMBER() OVER (ORDER BY hs.HocSinhID) as STT,
                    hs.HocSinhID,
                    ho.HoTen,
                    l.TenLop,
                    d.NamHocID as NamHoc,
                    d.HocKy,
                    AVG(d.DiemTrungBinh) as DiemTrungBinh,
                    CASE 
                        WHEN AVG(d.DiemTrungBinh) >= 8.5 THEN 'Giỏi'
                        WHEN AVG(d.DiemTrungBinh) >= 6.5 THEN 'Khá'
                        WHEN AVG(d.DiemTrungBinh) >= 5.0 THEN 'Trung bình'
                        WHEN AVG(d.DiemTrungBinh) >= 3.5 THEN 'Yếu'
                        ELSE 'Kém'
                    END as XepLoai,
                    CASE 
                        WHEN AVG(d.DiemTrungBinh) >= 5.0 THEN 'Đạt'
                        ELSE 'Không đạt'
                    END as GhiChu
                FROM HOCSINH hs
                JOIN HOSOHOCSINH hhs ON hs.HocSinhID = hhs.HocSinhID
                JOIN HOSO ho ON hhs.HoSoID = ho.HoSoID
                JOIN LOP l ON hhs.LopHocID = l.LopID
                JOIN DIEM d ON d.HocSinhID = hs.HocSinhID
                {where}
                GROUP BY hs.HocSinhID, ho.HoTen, l.TenLop, d.NamHocID, d.HocKy
                ORDER BY hs.HocSinhID
            ";
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                if (!string.IsNullOrEmpty(namHoc) && namHoc != "Tất cả")
                    cmd.Parameters.AddWithValue("@NamHoc", namHoc);
                if (!string.IsNullOrEmpty(lop) && lop != "Tất cả")
                    cmd.Parameters.AddWithValue("@Lop", lop);
                if (hocKy.HasValue && hocKy.Value != 0)
                    cmd.Parameters.AddWithValue("@HocKy", hocKy.Value);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var item = new TongKetNamHocItem
                        {
                            STT = Convert.ToInt32(reader["STT"]),
                            HocSinhID = reader["HocSinhID"].ToString(),
                            HoTen = reader["HoTen"].ToString(),
                            TenLop = reader["TenLop"].ToString(),
                            NamHoc = reader["NamHoc"].ToString(),
                            HocKy = Convert.ToInt32(reader["HocKy"]),
                            DiemTrungBinh = reader["DiemTrungBinh"] == DBNull.Value ? 0 : Convert.ToDouble(reader["DiemTrungBinh"]),
                            XepLoai = reader["XepLoai"].ToString(),
                            GhiChu = reader["GhiChu"].ToString()
                        };
                        list.Add(item);
                    }
                }
            }
            return list;
        }
        public static List<string> GetAllLop()
        {
            var list = new List<string>();
            string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
            string query = "SELECT DISTINCT TenLop FROM LOP ORDER BY TenLop";
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(reader["TenLop"].ToString());
                    }
                }
            }
            return list;
        }
        public static List<string> GetAllNamHoc()
        {
            var list = new List<string>();
            string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
            string query = "SELECT DISTINCT NamHocID FROM DIEM ORDER BY NamHocID";
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(reader["NamHocID"].ToString());
                    }
                }
            }
            return list;
        }
        public static List<int> GetAllHocKy()
        {
            var list = new List<int>();
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
        public static List<BangDiemLopItem> GetBangDiemLop(string namHoc, string lop, int hocKy)
        {
            var list = new List<BangDiemLopItem>();
            string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
            // Lấy danh sách môn học
            List<string> dsMon = new List<string>();
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                var cmdMon = new MySqlCommand("SELECT TenMonHoc FROM MONHOC", conn);
                using (var reader = cmdMon.ExecuteReader())
                {
                    while (reader.Read())
                        dsMon.Add(reader["TenMonHoc"].ToString());
                }
            }
            // Lấy bảng điểm từng học sinh
            string query = @"
                SELECT hs.HocSinhID, ho.HoTen, mh.TenMonHoc, d.DiemTrungBinh
                FROM HOCSINH hs
                JOIN HOSOHOCSINH hhs ON hs.HocSinhID = hhs.HocSinhID
                JOIN HOSO ho ON hhs.HoSoID = ho.HoSoID
                JOIN LOP l ON hhs.LopHocID = l.LopID
                JOIN DIEM d ON d.HocSinhID = hs.HocSinhID
                JOIN MONHOC mh ON d.MonHocID = mh.MonHocID
                WHERE d.NamHocID = @NamHoc AND l.TenLop = @Lop AND d.HocKy = @HocKy
                ORDER BY hs.HocSinhID, mh.TenMonHoc
            ";
            var dict = new Dictionary<string, BangDiemLopItem>();
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@NamHoc", namHoc);
                cmd.Parameters.AddWithValue("@Lop", lop);
                cmd.Parameters.AddWithValue("@HocKy", hocKy);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string hsID = reader["HocSinhID"].ToString();
                        if (!dict.ContainsKey(hsID))
                        {
                            dict[hsID] = new BangDiemLopItem
                            {
                                HocSinhID = hsID,
                                HoTen = reader["HoTen"].ToString(),
                                DiemTungMon = new Dictionary<string, double>(),
                                STT = dict.Count + 1
                            };
                        }
                        string mon = reader["TenMonHoc"].ToString();
                        double diem = reader["DiemTrungBinh"] == DBNull.Value ? 0 : Convert.ToDouble(reader["DiemTrungBinh"]);
                        dict[hsID].DiemTungMon[mon] = diem;
                    }
                }
            }
            // Tính điểm TB và xếp loại
            foreach (var item in dict.Values)
            {
                double tong = 0; int dem = 0;
                foreach (var mon in dsMon)
                {
                    if (item.DiemTungMon.ContainsKey(mon))
                    {
                        tong += item.DiemTungMon[mon];
                        dem++;
                    }
                }
                item.DiemTrungBinh = dem > 0 ? Math.Round(tong / dem, 2) : 0;
                if (item.DiemTrungBinh >= 8.5) item.XepLoai = "Giỏi";
                else if (item.DiemTrungBinh >= 6.5) item.XepLoai = "Khá";
                else if (item.DiemTrungBinh >= 5.0) item.XepLoai = "Trung bình";
                else if (item.DiemTrungBinh >= 3.5) item.XepLoai = "Yếu";
                else item.XepLoai = "Kém";
                list.Add(item);
            }
            return list;
        }
        public static ThongKeNamHoc GetThongKeNamHoc(string namHoc, string lop, int hocKy)
        {
            var thongKe = new ThongKeNamHoc();
            var bangDiem = GetBangDiemLop(namHoc, lop, hocKy);
            if (bangDiem.Count == 0) return thongKe;
            // Điểm TB lớp
            thongKe.DiemTrungBinhLop = Math.Round(bangDiem.Average(x => x.DiemTrungBinh), 2);
            // Điểm TB từng môn
            var dsMon = new List<string>();
            string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                var cmdMon = new MySqlCommand("SELECT TenMonHoc FROM MONHOC", conn);
                using (var reader = cmdMon.ExecuteReader())
                {
                    while (reader.Read())
                        dsMon.Add(reader["TenMonHoc"].ToString());
                }
            }
            foreach (var mon in dsMon)
            {
                var diemMon = bangDiem.Select(x => x.DiemTungMon.ContainsKey(mon) ? x.DiemTungMon[mon] : 0).ToList();
                double tb = diemMon.Count > 0 ? Math.Round(diemMon.Average(), 2) : 0;
                thongKe.DiemTrungBinhTungMon.Add((mon, tb));
            }
            // Tỉ lệ đạt
            int soDat = bangDiem.Count(x => x.DiemTrungBinh >= 5.0);
            thongKe.TiLeDat = Math.Round(100.0 * soDat / bangDiem.Count, 2);
            // Số lượng xếp loại
            thongKe.SoLuongGioi = bangDiem.Count(x => x.XepLoai == "Giỏi");
            thongKe.SoLuongKha = bangDiem.Count(x => x.XepLoai == "Khá");
            thongKe.SoLuongTrungBinh = bangDiem.Count(x => x.XepLoai == "Trung bình");
            thongKe.SoLuongYeu = bangDiem.Count(x => x.XepLoai == "Yếu");
            thongKe.SoLuongKem = bangDiem.Count(x => x.XepLoai == "Kém");
            return thongKe;
        }
        public static (List<BangDiemLopItem> BangDiem, double DiemTrungBinhLop, double TiLeDat, int SoLuongGioi, int SoLuongKha, int SoLuongTrungBinh, int SoLuongYeu, int SoLuongKem, List<(string MonHoc, double DiemTB)> DiemTrungBinhTungMon)
            GetBangDiemLopVaThongKe(string namHoc, string lop, int hocKy)
        {
            var bangDiem = GetBangDiemLop(namHoc, lop, hocKy);
            var thongKe = GetThongKeNamHoc(namHoc, lop, hocKy);
            return (bangDiem, thongKe.DiemTrungBinhLop, thongKe.TiLeDat, thongKe.SoLuongGioi, thongKe.SoLuongKha, thongKe.SoLuongTrungBinh, thongKe.SoLuongYeu, thongKe.SoLuongKem, thongKe.DiemTrungBinhTungMon);
        }

        public static HocSinhThongTin GetThongTinHocSinh(string hocSinhID, string namHoc, string lop)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
            string query = @"
                SELECT ho.HoTen, hs.HocSinhID, l.TenLop, d.NamHocID, ho.NgaySinh
                FROM HOCSINH hs
                JOIN HOSOHOCSINH hhs ON hs.HocSinhID = hhs.HocSinhID
                JOIN HOSO ho ON hhs.HoSoID = ho.HoSoID
                JOIN LOP l ON hhs.LopHocID = l.LopID
                JOIN DIEM d ON d.HocSinhID = hs.HocSinhID
                WHERE hs.HocSinhID = @HocSinhID AND d.NamHocID = @NamHoc AND l.TenLop = @Lop
                LIMIT 1
            ";
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@HocSinhID", hocSinhID);
                    cmd.Parameters.AddWithValue("@NamHoc", namHoc);
                    cmd.Parameters.AddWithValue("@Lop", lop);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new HocSinhThongTin
                            {
                                HoTen = reader["HoTen"].ToString(),
                                HocSinhID = reader["HocSinhID"].ToString(),
                                TenLop = reader["TenLop"].ToString(),
                                NamHoc = reader["NamHocID"].ToString(),
                                NgaySinh = Convert.ToDateTime(reader["NgaySinh"])
                            };
                        }
                    }
                }
            }
            return null;
        }

        public static List<BangDiemMonHocItem> GetBangDiemMonHoc(string hocSinhID, string namHoc)
        {
            var list = new List<BangDiemMonHocItem>();
            string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
            string query = @"
                SELECT mh.TenMonHoc,
                    MAX(CASE WHEN d.HocKy = 1 THEN d.DiemTrungBinh END) AS DiemTBHK1,
                    MAX(CASE WHEN d.HocKy = 2 THEN d.DiemTrungBinh END) AS DiemTBHK2
                FROM DIEM d
                JOIN MONHOC mh ON d.MonHocID = mh.MonHocID
                WHERE d.HocSinhID = @HocSinhID AND d.NamHocID = @NamHoc
                GROUP BY mh.TenMonHoc
                ORDER BY mh.TenMonHoc
            ";
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@HocSinhID", hocSinhID);
                    cmd.Parameters.AddWithValue("@NamHoc", namHoc);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new BangDiemMonHocItem
                            {
                                MonHoc = reader["TenMonHoc"].ToString(),
                                DiemTBHK1 = reader["DiemTBHK1"] == DBNull.Value ? (double?)null : Convert.ToDouble(reader["DiemTBHK1"]),
                                DiemTBHK2 = reader["DiemTBHK2"] == DBNull.Value ? (double?)null : Convert.ToDouble(reader["DiemTBHK2"])
                            });
                        }
                    }
                }
            }
            return list;
        }
    }
}
