using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using QuanLyHocSinh.Model.Entities;

public class DiemDAL
{
    public static List<Diem> GetAllDiemHocSinh()
    {
        List<Diem> list = new List<Diem>();
        string connectionString = "Server=localhost;Database=quanlyhocsinh;Uid=khanghy1102;Pwd=khanghy1102;SslMode=none;";
        string query = @"
            SELECT hs.HocSinhID AS MaHS, ho.HoTen, l.TenLop AS Lop, mh.TenMonHoc AS MonHoc,
                   IFNULL(diem_mieng.GiaTri, 0) AS DiemMieng,
                   IFNULL(diem_15p.GiaTri, 0) AS Diem15p,
                   IFNULL(diem_1tiet.GiaTri, 0) AS Diem1Tiet,
                   IFNULL(diem_thi.GiaTri, 0) AS DiemThi,
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
                        DiemMieng = float.Parse(reader["DiemMieng"].ToString()),
                        Diem15p = float.Parse(reader["Diem15p"].ToString()),
                        Diem1Tiet = float.Parse(reader["Diem1Tiet"].ToString()),
                        DiemThi = float.Parse(reader["DiemThi"].ToString()),
                        DiemTB = float.Parse(reader["DiemTB"].ToString())
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
        string connectionString = "Server=localhost;Database=quanlyhocsinh;Uid=khanghy1102;Pwd=khanghy1102;SslMode=none;";
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
        string connectionString = "Server=localhost;Database=quanlyhocsinh;Uid=khanghy1102;Pwd=khanghy1102;SslMode=none;";
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

    // Sửa điểm
    public static void UpdateDiem(Diem diem)
    {
        string connectionString = "Server=localhost;Database=quanlyhocsinh;Uid=khanghy1102;Pwd=khanghy1102;SslMode=none;";
        string query = @"
            UPDATE DIEM d
            JOIN HOCSINH hs ON d.HocSinhID = hs.HocSinhID
            JOIN MONHOC mh ON d.MonHocID = mh.MonHocID
            JOIN CHITIETDIEM ctd_mieng ON ctd_mieng.DiemID = d.DiemID AND ctd_mieng.LoaiDiemID = 'LD001'
            JOIN CHITIETDIEM ctd_15p ON ctd_15p.DiemID = d.DiemID AND ctd_15p.LoaiDiemID = 'LD002'
            JOIN CHITIETDIEM ctd_1tiet ON ctd_1tiet.DiemID = d.DiemID AND ctd_1tiet.LoaiDiemID = 'LD003'
            JOIN CHITIETDIEM ctd_thi ON ctd_thi.DiemID = d.DiemID AND ctd_thi.LoaiDiemID = 'LD004'
            SET 
                ctd_mieng.GiaTri = @DiemMieng,
                ctd_15p.GiaTri = @Diem15p,
                ctd_1tiet.GiaTri = @Diem1Tiet,
                ctd_thi.GiaTri = @DiemThi,
                d.DiemTrungBinh = @DiemTB
            WHERE hs.HocSinhID = @MaHS AND mh.TenMonHoc = @MonHoc;";

        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@DiemMieng", diem.DiemMieng);
            cmd.Parameters.AddWithValue("@Diem15p", diem.Diem15p);
            cmd.Parameters.AddWithValue("@Diem1Tiet", diem.Diem1Tiet);
            cmd.Parameters.AddWithValue("@DiemThi", diem.DiemThi);
            cmd.Parameters.AddWithValue("@DiemTB", diem.DiemTB);
            cmd.Parameters.AddWithValue("@MaHS", diem.MaHS);
            cmd.Parameters.AddWithValue("@MonHoc", diem.MonHoc);

            cmd.ExecuteNonQuery();
        }
    }
}
