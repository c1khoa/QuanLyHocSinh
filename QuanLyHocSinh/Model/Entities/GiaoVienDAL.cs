using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using QuanLyHocSinh.Model.Entities;
using System.Configuration;

public class GiaoVienDAL
{
    public static List<GiaoVien> GetAllGiaoVien()
    {
        List<GiaoVien> list = new List<GiaoVien>();
        string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
        string query = @"
            SELECT gv.GiaoVienID AS MaGV, ho.HoTen, ho.NgaySinh, ho.GioiTinh, ho.Email, ho.DiaChi, mh.TenMonHoc AS BoMon, l.TenLop AS LopDayID
            FROM GIAOVIEN gv
            JOIN HOSOGIAOVIEN hsgv ON gv.GiaoVienID = hsgv.GiaoVienID
            JOIN HOSO ho ON hsgv.HoSoID = ho.HoSoID
            LEFT JOIN CHITIETMONHOC ctmh ON ctmh.GiaoVienID = gv.GiaoVienID
            LEFT JOIN MONHOC mh ON ctmh.MonHocID = mh.MonHocID
            LEFT JOIN LOP l ON l.LopID = hsgv.LopDayID
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
                        BoMon = reader["BoMon"].ToString(),
                        LopDayID = reader["LopDayID"].ToString(),
                        DiaChi = reader["DiaChi"].ToString()
                    };
                    list.Add(gv);
                }
            }
        }
        return list;
    }

    //Cập nhật thông tin giáo viên
    public static void UpdateGiaoVien(GiaoVien giaoVien)
    {
        string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
        string query = @"UPDATE HOSO h
                            JOIN HOSOGIAOVIEN hgv ON h.HoSoID = hgv.HoSoID
                            LEFT JOIN CHITIETMONHOC ctmh ON ctmh.GiaoVienID = hgv.GiaoVienID
                            LEFT JOIN MONHOC mh ON ctmh.MonHocID = mh.MonHocID
                            SET h.HoTen = @HoTen, h.GioiTinh = @GioiTinh, h.NgaySinh = @NgaySinh, h.Email = @Email, h.DiaChi = @DiaChi,
                                hgv.LopDayID = (SELECT LopID FROM LOP WHERE TenLop = @TenLop),
                                mh.TenMonHoc = @BoMon
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
                cmd.Parameters.AddWithValue("@TenLop", giaoVien.LopDayID);
                cmd.Parameters.AddWithValue("@BoMon", giaoVien.BoMon);
                cmd.Parameters.AddWithValue("@GiaoVienID", giaoVien.MaGV);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
