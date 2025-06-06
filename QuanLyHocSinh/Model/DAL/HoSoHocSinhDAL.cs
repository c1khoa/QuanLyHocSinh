using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using QuanLyHocSinh.Model.Entities;

namespace QuanLyHocSinh.Model.Relationships
{
        public class HoSoHocSinhDAL : BaseDAL
    {
        private static async Task<string> GenerateNewHoSoHocSinhIDAsync(MySqlConnection conn, MySqlTransaction transaction)
        {
            // Using the same logic as in LopDALHelper for consistency
            string prefix = "HSHS"; 
            string query = "SELECT HoSoHocSinhID FROM HOSOHOCSINH ORDER BY LENGTH(HoSoHocSinhID) DESC, HoSoHocSinhID DESC LIMIT 1";
            using (MySqlCommand cmd = new MySqlCommand(query, conn, transaction))
            {
                var result = await cmd.ExecuteScalarAsync();
                if (result != null && result != DBNull.Value)
                {
                    string lastId = result.ToString();
                    if (lastId.StartsWith(prefix) && lastId.Length > prefix.Length)
                    {
                        string numericPart = lastId.Substring(prefix.Length);
                        if (int.TryParse(numericPart, out int num)) { num++; return prefix + num.ToString("D4"); }
                    }
                }
                return prefix + "0001";
            }
        }

        public async Task AddHoSoHocSinhAsync(HoSoHocSinh hoSoHocSinh)
        {
            using (MySqlConnection conn = GetConnection())
            {
                await conn.OpenAsync();
                using (MySqlTransaction transaction = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        hoSoHocSinh.HoSoHocSinhID = await GenerateNewHoSoHocSinhIDAsync(conn, transaction);
                        string query = @"INSERT INTO HOSOHOCSINH 
                                         (HoSoHocSinhID, HocSinhID, HoSoID, LopHocID, NienKhoa) 
                                         VALUES (@HoSoHocSinhID, @HocSinhID, @HoSoID, @LopHocID, @NienKhoa)";
                        using (MySqlCommand cmd = new MySqlCommand(query, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@HoSoHocSinhID", hoSoHocSinh.HoSoHocSinhID);
                            cmd.Parameters.AddWithValue("@HocSinhID", hoSoHocSinh.HocSinhID);
                            cmd.Parameters.AddWithValue("@HoSoID", hoSoHocSinh.HoSoID);
                            cmd.Parameters.AddWithValue("@LopHocID", hoSoHocSinh.LopHocID);
                            cmd.Parameters.AddWithValue("@NienKhoa", hoSoHocSinh.NienKhoa);
                            await cmd.ExecuteNonQueryAsync();
                        }
                        // Potentially update Lop.SiSo here or in a service layer
                        await transaction.CommitAsync();
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
        }

        public async Task UpdateHoSoHocSinhAsync(HoSoHocSinh hoSoHocSinh)
        {
            using (MySqlConnection conn = GetConnection())
            {
                await conn.OpenAsync();
                using (MySqlTransaction transaction = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        string query = @"UPDATE HOSOHOCSINH SET 
                                         HocSinhID = @HocSinhID, HoSoID = @HoSoID, 
                                         LopHocID = @LopHocID, NienKhoa = @NienKhoa 
                                         WHERE HoSoHocSinhID = @HoSoHocSinhID";
                        using (MySqlCommand cmd = new MySqlCommand(query, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@HocSinhID", hoSoHocSinh.HocSinhID);
                            cmd.Parameters.AddWithValue("@HoSoID", hoSoHocSinh.HoSoID);
                            cmd.Parameters.AddWithValue("@LopHocID", hoSoHocSinh.LopHocID);
                            cmd.Parameters.AddWithValue("@NienKhoa", hoSoHocSinh.NienKhoa);
                            cmd.Parameters.AddWithValue("@HoSoHocSinhID", hoSoHocSinh.HoSoHocSinhID);
                            int rowsAffected = await cmd.ExecuteNonQueryAsync();
                            if (rowsAffected == 0) throw new Exception("Hồ sơ học sinh không tìm thấy hoặc không có gì thay đổi.");
                        }
                        await transaction.CommitAsync();
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
        }

        public async Task DeleteHoSoHocSinhAsync(string hoSoHocSinhID)
        {
            using (MySqlConnection conn = GetConnection())
            {
                await conn.OpenAsync();
                using (MySqlTransaction transaction = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        // Potentially update Lop.SiSo here or in a service layer
                        string query = "DELETE FROM HOSOHOCSINH WHERE HoSoHocSinhID = @HoSoHocSinhID";
                        using (MySqlCommand cmd = new MySqlCommand(query, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@HoSoHocSinhID", hoSoHocSinhID);
                             int rowsAffected = await cmd.ExecuteNonQueryAsync();
                            if (rowsAffected == 0) throw new Exception("Hồ sơ học sinh không tìm thấy.");
                        }
                        await transaction.CommitAsync();
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
        }
        
        private HoSoHocSinh MapReaderToHoSoHocSinh(MySqlDataReader reader, Dictionary<string, int> ordinals)
        {
             return new HoSoHocSinh
            {
                HoSoHocSinhID = reader.GetString(ordinals["HoSoHocSinhID"]),
                HocSinhID = reader.GetString(ordinals["HocSinhID"]),
                HoSoID = reader.GetString(ordinals["HoSoID"]),
                LopHocID = reader.GetString(ordinals["LopHocID"]),
                NienKhoa = reader.GetInt32(ordinals["NienKhoa"])
                // Navigation properties can be populated if joining
            };
        }
        private Dictionary<string, int> GetHoSoHocSinhOrdinals(MySqlDataReader reader)
        {
            return new Dictionary<string, int> {
                {"HoSoHocSinhID", reader.GetOrdinal("HoSoHocSinhID")},
                {"HocSinhID", reader.GetOrdinal("HocSinhID")},
                {"HoSoID", reader.GetOrdinal("HoSoID")},
                {"LopHocID", reader.GetOrdinal("LopHocID")},
                {"NienKhoa", reader.GetOrdinal("NienKhoa")}
            };
        }


        public async Task<HoSoHocSinh> GetHoSoHocSinhByIdAsync(string hoSoHocSinhID)
        {
            HoSoHocSinh hshs = null;
            string query = "SELECT * FROM HOSOHOCSINH WHERE HoSoHocSinhID = @HoSoHocSinhID";
            using (MySqlConnection conn = GetConnection())
            {
                await conn.OpenAsync();
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@HoSoHocSinhID", hoSoHocSinhID);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            var mysqlReader = (MySqlDataReader)reader;
                            var ordinals = GetHoSoHocSinhOrdinals(mysqlReader);
                            hshs = MapReaderToHoSoHocSinh(mysqlReader, ordinals);
                        }
                    }
                }
            }
            return hshs;
        }

        public async Task<List<HoSoHocSinh>> GetHoSoHocSinhByHocSinhIDAsync(string hocSinhID)
        {
            List<HoSoHocSinh> list = new List<HoSoHocSinh>();
            string query = "SELECT * FROM HOSOHOCSINH WHERE HocSinhID = @HocSinhID ORDER BY NienKhoa DESC";
            using (MySqlConnection conn = GetConnection())
            {
                await conn.OpenAsync();
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@HocSinhID", hocSinhID);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if(reader.HasRows)
                        {
                            var mysqlReader = (MySqlDataReader)reader; // Ép kiểu ở đây
                            var ordinals = GetHoSoHocSinhOrdinals(mysqlReader);
                            while (await reader.ReadAsync())
                            {
                                list.Add(MapReaderToHoSoHocSinh(mysqlReader, ordinals));
                            }
                        }
                    }
                }
            }
            return list;
        }
         public async Task<List<HoSoHocSinh>> GetHoSoHocSinhByLopAndNienKhoaAsync(string lopHocID, int nienKhoa)
        {
            List<HoSoHocSinh> list = new List<HoSoHocSinh>();
            // Query should also join with HOSO and HOCSINH to get student names etc. for practical use.
            // For now, just the HOSOHOCSINH record.
            string query = "SELECT * FROM HOSOHOCSINH WHERE LopHocID = @LopHocID AND NienKhoa = @NienKhoa";
            using (MySqlConnection conn = GetConnection())
            {
                await conn.OpenAsync();
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@LopHocID", lopHocID);
                    cmd.Parameters.AddWithValue("@NienKhoa", nienKhoa);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                         if(reader.HasRows)
                        {
                            var mysqlReader = (MySqlDataReader)reader; // Ép kiểu ở đây
                            var ordinals = GetHoSoHocSinhOrdinals(mysqlReader);
                            while (await reader.ReadAsync())
                            {
                                list.Add(MapReaderToHoSoHocSinh(mysqlReader, ordinals));
                            }
                        }
                    }
                }
            }
            return list;
        }
    }
}