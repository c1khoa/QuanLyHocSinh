using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using QuanLyHocSinh.Model.Relationships;
using System.Configuration;
namespace QuanLyHocSinh.Model.Entities
{
    public class QuyDinhDAL
    {
        public static QuyDinhEntities GetQuyDinh()
        {
            QuyDinhEntities quyDinh = null;
            string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT SiSoLop, SoLuongMonHoc, DiemDat, QuyDinhKhac FROM QUYDINH LIMIT 1";
                using (var cmd = new MySqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        quyDinh = new QuyDinhEntities
                        {
                            SiSoLop_ToiDa = reader.GetInt32("SiSoLop"),
                            SoLuongMonHoc = reader.GetInt32("SoLuongMonHoc"),
                            DiemDat = reader.GetFloat("DiemDat"),
                            QuyDinhKhac = reader.GetString("QuyDinhKhac")
                        };
                    }
                }
            }
            return quyDinh;
        }

        public static void UpdateQuyDinh(QuyDinhEntities quyDinh)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = @"UPDATE QUYDINH SET 
                                SiSoLop = @SiSoLop, 
                                SoLuongMonHoc = @SoLuongMonHoc, 
                                DiemDat = @DiemDat, 
                                QuyDinhKhac = @QuyDinhKhac"; // Giả sử chỉ có 1 dòng, không cần WHERE

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@SiSoLop", quyDinh.SiSoLop_ToiDa);
                    cmd.Parameters.AddWithValue("@SoLuongMonHoc", quyDinh.SoLuongMonHoc);
                    cmd.Parameters.AddWithValue("@DiemDat", quyDinh.DiemDat);
                    cmd.Parameters.AddWithValue("@QuyDinhKhac", quyDinh.QuyDinhKhac);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}