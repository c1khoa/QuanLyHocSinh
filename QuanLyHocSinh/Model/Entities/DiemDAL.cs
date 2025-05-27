using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using QuanLyHocSinh.Model.Entities;
using System.Configuration;

public class DiemDAL
{
    public static List<Diem> GetAllDiemHocSinh()
    {
        List<Diem> list = new List<Diem>();
        string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
        string query = @"
            SELECT hs.HocSinhID AS MaHS, ho.HoTen, l.TenLop AS Lop, mh.TenMonHoc AS MonHoc, d.NamHocID, d.HocKy,
                   IFNULL(diem_mieng.GiaTri, -1) AS DiemMieng,
                   IFNULL(diem_15p.GiaTri, -1) AS Diem15p,
                   IFNULL(diem_1tiet.GiaTri, -1) AS Diem1Tiet,
                   IFNULL(diem_thi.GiaTri, -1) AS DiemThi,
                   IFNULL(d.DiemTrungBinh, 0) AS DiemTB
            FROM HOCSINH hs
            JOIN HOSOHOCSINH hhs ON hs.HocSinhID = hhs.HocSinhID
            JOIN HOSO ho ON hhs.HoSoID = ho.HoSoID
            JOIN LOP l ON hhs.LopHocID = l.LopID
            JOIN DIEM d ON d.HocSinhID = hs.HocSinhID
            JOIN MONHOC mh ON d.MonHocID = mh.MonHocID
            LEFT JOIN CHITIETDIEM diem_mieng ON diem_mieng.DiemID = d.DiemID AND diem_mieng.LoaiDiemID = 'LD001'
            LEFT JOIN CHITIETDIEM diem_15p ON diem_15p.DiemID = d.DiemID AND diem_15p.LoaiDiemID = 'LD002'
            LEFT JOIN CHITIETDIEM diem_1tiet ON diem_1tiet.DiemID = d.DiemID AND diem_1tiet.LoaiDiemID = 'LD003'
            LEFT JOIN CHITIETDIEM diem_thi ON diem_thi.DiemID = d.DiemID AND diem_thi.LoaiDiemID = 'LD004'
        ";

        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(query, conn);
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    Diem dhs = new Diem
                    {
                        MaHS = reader["MaHS"].ToString(),
                        HoTen = reader["HoTen"].ToString(),
                        Lop = reader["Lop"].ToString(),
                        MonHoc = reader["MonHoc"].ToString(),
                        NamHocID = reader["NamHocID"].ToString(),
                        HocKy = int.Parse(reader["HocKy"].ToString()),
                        DiemMieng = float.TryParse(reader["DiemMieng"].ToString(), out float dm) ? dm : (float?)-1,
                        Diem15p = float.TryParse(reader["Diem15p"].ToString(), out float d15) ? d15 : (float?)-1,
                        Diem1Tiet = float.TryParse(reader["Diem1Tiet"].ToString(), out float d1t) ? d1t : (float?)-1,
                        DiemThi = float.TryParse(reader["DiemThi"].ToString(), out float dt) ? dt : (float?)-1,
                        DiemTB = float.TryParse(reader["DiemTB"].ToString(), out float dtb) ? dtb : (float?)-1
                    };
                    list.Add(dhs);
                }
            }
        }
        return list;
    }

    public static List<string> GetAllMonHoc()
    {
        List<string> list = new List<string>();
        string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
        string query = "SELECT TenMonHoc FROM MONHOC";

        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(query, conn);
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    list.Add(reader["TenMonHoc"].ToString());
                }
            }
        }
        return list;
    }

    public static List<string> GetAllLop()
    {
        List<string> list = new List<string>();
        string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
        string query = "SELECT TenLop FROM LOP";

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
                    list.Add(reader["NamHocID"].ToString());
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
                    list.Add(int.Parse(reader["HocKy"].ToString()));
                }
            }
        }
        return list;
    }

    // Sửa điểm
    public static void UpdateDiem(Diem diem)
    {
        string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            conn.Open();

            // Lấy DiemID và MonHocID
            string getIdQuery = @"
                SELECT d.DiemID, mh.MonHocID
                FROM DIEM d
                JOIN MONHOC mh ON d.MonHocID = mh.MonHocID
                WHERE d.HocSinhID = @MaHS AND mh.TenMonHoc = @MonHoc AND d.NamHocID = @NamHocID AND d.HocKy = @HocKy";
            string diemID = "", monHocID = "";
            using (var cmd = new MySqlCommand(getIdQuery, conn))
            {
                cmd.Parameters.AddWithValue("@MaHS", diem.MaHS);
                cmd.Parameters.AddWithValue("@MonHoc", diem.MonHoc);
                cmd.Parameters.AddWithValue("@NamHocID", diem.NamHocID);
                cmd.Parameters.AddWithValue("@HocKy", diem.HocKy);

                // Lấy thông tin điểm
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        diemID = reader["DiemID"].ToString();
                        monHocID = reader["MonHocID"].ToString();
                    }
                }
            }
            if (string.IsNullOrEmpty(diemID) || string.IsNullOrEmpty(monHocID)) return; // Không tìm thấy

            // Danh sách loại điểm và giá trị
            var diemTypes = new Dictionary<string, float>
            {
                { "LD001", diem.DiemMieng ?? -1 },
                { "LD002", diem.Diem15p ?? -1 },
                { "LD003", diem.Diem1Tiet ?? -1 },
                { "LD004", diem.DiemThi ?? -1 }
            };

            foreach (var item in diemTypes)
            {
                if (item.Value < 0) continue; // Bỏ qua nếu không có điểm

                // Kiểm tra tồn tại
                string checkQuery = "SELECT COUNT(*) FROM CHITIETDIEM WHERE DiemID = @DiemID AND LoaiDiemID = @LoaiDiemID";
                int count = 0;
                using (var cmd = new MySqlCommand(checkQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@DiemID", diemID);
                    cmd.Parameters.AddWithValue("@LoaiDiemID", item.Key);
                    count = Convert.ToInt32(cmd.ExecuteScalar());
                }

                if (count == 0)
                {
                    // Sinh mã mới cho ChiTietDiemID
                    string newChiTietDiemID = GenerateNewChiTietDiemID(conn);
                    string insertQuery = "INSERT INTO CHITIETDIEM (ChiTietDiemID, DiemID, LoaiDiemID, GiaTri) VALUES (@ChiTietDiemID, @DiemID, @LoaiDiemID, @GiaTri)";
                    using (var cmd = new MySqlCommand(insertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@ChiTietDiemID", newChiTietDiemID);
                        cmd.Parameters.AddWithValue("@DiemID", diemID);
                        cmd.Parameters.AddWithValue("@LoaiDiemID", item.Key);
                        cmd.Parameters.AddWithValue("@GiaTri", item.Value);
                        cmd.ExecuteNonQuery();
                    }
                }
                else
                {
                    // Nếu đã có, update
                    string updateQuery = "UPDATE CHITIETDIEM SET GiaTri = @GiaTri WHERE DiemID = @DiemID AND LoaiDiemID = @LoaiDiemID";
                    using (var cmd = new MySqlCommand(updateQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@GiaTri", item.Value);
                        cmd.Parameters.AddWithValue("@DiemID", diemID);
                        cmd.Parameters.AddWithValue("@LoaiDiemID", item.Key);
                        cmd.ExecuteNonQuery();
                    }
                }
            }

            // Sau khi cập nhật các điểm, cập nhật điểm trung bình
            float diemTB = TinhDiemTrungBinh(diemID, conn);
            string updateDiemTB = "UPDATE DIEM SET DiemTrungBinh = @DiemTB WHERE DiemID = @DiemID";
            using (var cmd = new MySqlCommand(updateDiemTB, conn))
            {
                cmd.Parameters.AddWithValue("@DiemTB", diemTB);
                cmd.Parameters.AddWithValue("@DiemID", diemID);
                cmd.ExecuteNonQuery();
            }
        }
    }

    private static float TinhDiemTrungBinh(string diemID, MySqlConnection conn)
    {
        // Lấy điểm và loại điểm
        string query = @"
            SELECT ctd.GiaTri, ctd.LoaiDiemID, ld.HeSo
            FROM CHITIETDIEM ctd
            JOIN LOAIDIEM ld ON ctd.LoaiDiemID = ld.LoaiDiemID
            WHERE ctd.DiemID = @DiemID";
        float tongDiem = 0;
        float tongHeSo = 0;
        using (var cmd = new MySqlCommand(query, conn))
        {
            cmd.Parameters.AddWithValue("@DiemID", diemID);
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    float giaTri = float.Parse(reader["GiaTri"].ToString());
                    float heSo = float.Parse(reader["HeSo"].ToString());
                    if (giaTri >= 0)
                    {
                        tongDiem += giaTri * heSo;
                        tongHeSo += heSo;
                    }
                }
            }
        }
        if (tongHeSo == 0) return -1;
        return (float)Math.Round(tongDiem / tongHeSo, 2);
    }

    // Hàm sinh mã mới cho ChiTietDiemID
    private static string GenerateNewChiTietDiemID(MySqlConnection conn)
    {
        string prefix = "CTD";
        string query = "SELECT ChiTietDiemID FROM CHITIETDIEM ORDER BY ChiTietDiemID DESC LIMIT 1";
        using (var cmd = new MySqlCommand(query, conn))
        {
            var result = cmd.ExecuteScalar();
            if (result != null && result.ToString().StartsWith(prefix))
            {
                string lastId = result.ToString();
                int num = int.Parse(lastId.Substring(prefix.Length));
                return prefix + (num + 1).ToString("D3");
            }
            else
            {
                return prefix + "001";
            }
        }
    }
}
