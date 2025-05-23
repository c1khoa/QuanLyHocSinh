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
                    {
                        // Tạo đối tượng HocSinh từ kết quả truy vấn
                        HocSinh hs = new HocSinh
                        {
                            HocSinhID = reader["HocSinhID"].ToString(),
                            HoTen = reader["HoTen"].ToString(),
                            GioiTinh = reader["GioiTinh"].ToString(),
                            NgaySinh = Convert.ToDateTime(reader["NgaySinh"]),
                            Email = reader["Email"].ToString(),
                            DiaChi = reader["DiaChi"].ToString(),
                            TenLop = reader["TenLop"].ToString(),
                            NienKhoa = Convert.ToInt32(reader["NienKhoa"]),
                            TrangThaiHoSo = reader["TrangThaiHoSo"].ToString()
                        };
                        list.Add(hs);
                    }
                }
            }
            return list;
        }

        public static void UpdateHocSinh(HocSinh hocSinh)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
            string query = @"UPDATE HOSO h
                             JOIN HOSOHOCSINH hhs ON h.HoSoID = hhs.HoSoID
                             SET h.HoTen = @HoTen, h.GioiTinh = @GioiTinh, h.NgaySinh = @NgaySinh, h.Email = @Email, h.DiaChi = @DiaChi,
                                 hhs.LopHocID = (SELECT LopID FROM LOP WHERE TenLop = @TenLop), hhs.NienKhoa = @NienKhoa
                             WHERE hhs.HocSinhID = @HocSinhID";
            using (var conn = new MySql.Data.MySqlClient.MySqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new MySql.Data.MySqlClient.MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@HoTen", hocSinh.HoTen);
                    cmd.Parameters.AddWithValue("@GioiTinh", hocSinh.GioiTinh);
                    cmd.Parameters.AddWithValue("@NgaySinh", hocSinh.NgaySinh);
                    cmd.Parameters.AddWithValue("@Email", hocSinh.Email);
                    cmd.Parameters.AddWithValue("@DiaChi", hocSinh.DiaChi);
                    cmd.Parameters.AddWithValue("@TenLop", hocSinh.TenLop);
                    cmd.Parameters.AddWithValue("@NienKhoa", hocSinh.NienKhoa);
                    cmd.Parameters.AddWithValue("@HocSinhID", hocSinh.HocSinhID);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}