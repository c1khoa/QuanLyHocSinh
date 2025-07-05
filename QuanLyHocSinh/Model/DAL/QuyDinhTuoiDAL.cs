using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using QuanLyHocSinh.Model.Relationships;
using System.Configuration;

namespace QuanLyHocSinh.Model.Entities
{
    public class QuyDinhTuoiDAL
    {
        public static QuyDinhTuoiEntities GetQuyDinhTuoi(string quyDinhTuoiID)
        {
            QuyDinhTuoiEntities qdt = null;
            string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT TuoiToiThieu, TuoiToiDa FROM QUYDINHTUOI WHERE QuyDinhTuoiID = @ID";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ID", quyDinhTuoiID);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            qdt = new QuyDinhTuoiEntities
                            {
                                QuyDinhTuoiID = quyDinhTuoiID,
                                TuoiToiThieu = reader.GetInt32("TuoiToiThieu"),
                                TuoiToiDa = reader.GetInt32("TuoiToiDa")
                            };
                        }
                    }
                }
            }
            return qdt;
        }

        public static void UpdateQuyDinhTuoi(QuyDinhTuoiEntities qdt)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "UPDATE QUYDINHTUOI SET TuoiToiThieu = @Min, TuoiToiDa = @Max WHERE QuyDinhTuoiID = @ID";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Min", qdt.TuoiToiThieu);
                    cmd.Parameters.AddWithValue("@Max", qdt.TuoiToiDa);
                    cmd.Parameters.AddWithValue("@ID", qdt.QuyDinhTuoiID);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}