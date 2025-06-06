using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using QuanLyHocSinh.Model.Entities;

namespace QuanLyHocSinh.Model.Relationships
{
        public class HoSoGiaoVienDAL : BaseDAL
    {
        private static async Task<string> GenerateNewHoSoGiaoVienIDAsync(MySqlConnection conn, MySqlTransaction transaction)
        {
            string prefix = "HGV"; // HoSoGiaoVien
            // HoSoGiaoVienID CHAR(8), sample HGV001. "HGV" + 5 digits
            string query = "SELECT HoSoGiaoVienID FROM HOSOGIAOVIEN WHERE HoSoGiaoVienID LIKE @PrefixPattern ORDER BY HoSoGiaoVienID DESC LIMIT 1";
            int nextNumber = 1;

            using (MySqlCommand cmd = new MySqlCommand(query, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@PrefixPattern", prefix + "%");
                var result = await cmd.ExecuteScalarAsync();
                if (result != null && result != DBNull.Value)
                {
                    string lastId = result.ToString();
                    if (lastId.StartsWith(prefix) && lastId.Length > prefix.Length)
                    {
                        if (int.TryParse(lastId.Substring(prefix.Length), out int num))
                        {
                            nextNumber = num + 1;
                        }
                    }
                }
            }
            return prefix + nextNumber.ToString("D5");
        }

        public async Task AddHoSoGiaoVienAsync(HoSoGiaoVien hoSoGiaoVien)
        {
            using (MySqlConnection conn = GetConnection())
            {
                await conn.OpenAsync();
                using (MySqlTransaction transaction = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        hoSoGiaoVien.HoSoGiaoVienID = await GenerateNewHoSoGiaoVienIDAsync(conn, transaction);
                        string query = @"INSERT INTO HOSOGIAOVIEN 
                                         (HoSoGiaoVienID, GiaoVienID, HoSoID, LopDayID, NgayBatDauLamViec) 
                                         VALUES (@HoSoGiaoVienID, @GiaoVienID, @HoSoID, @LopDayID, @NgayBatDauLamViec)";
                        using (MySqlCommand cmd = new MySqlCommand(query, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@HoSoGiaoVienID", hoSoGiaoVien.HoSoGiaoVienID);
                            cmd.Parameters.AddWithValue("@GiaoVienID", hoSoGiaoVien.GiaoVienID);
                            cmd.Parameters.AddWithValue("@HoSoID", hoSoGiaoVien.HoSoID);
                            cmd.Parameters.AddWithValue("@LopDayID", (object)hoSoGiaoVien.LopDayID ?? DBNull.Value); // LopDayID can be null if not GVCN
                            cmd.Parameters.AddWithValue("@NgayBatDauLamViec", hoSoGiaoVien.NgayBatDauLamViec);
                            await cmd.ExecuteNonQueryAsync();
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
        
        public async Task UpdateHoSoGiaoVienAsync(HoSoGiaoVien hoSoGiaoVien)
        {
            using (MySqlConnection conn = GetConnection())
            {
                await conn.OpenAsync();
                using (MySqlTransaction transaction = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        string query = @"UPDATE HOSOGIAOVIEN SET 
                                         GiaoVienID = @GiaoVienID, HoSoID = @HoSoID, 
                                         LopDayID = @LopDayID, NgayBatDauLamViec = @NgayBatDauLamViec 
                                         WHERE HoSoGiaoVienID = @HoSoGiaoVienID";
                        using (MySqlCommand cmd = new MySqlCommand(query, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@GiaoVienID", hoSoGiaoVien.GiaoVienID);
                            cmd.Parameters.AddWithValue("@HoSoID", hoSoGiaoVien.HoSoID);
                            cmd.Parameters.AddWithValue("@LopDayID", (object)hoSoGiaoVien.LopDayID ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@NgayBatDauLamViec", hoSoGiaoVien.NgayBatDauLamViec);
                            cmd.Parameters.AddWithValue("@HoSoGiaoVienID", hoSoGiaoVien.HoSoGiaoVienID);
                            int rowsAffected = await cmd.ExecuteNonQueryAsync();
                            if (rowsAffected == 0) throw new Exception("Hồ sơ giáo viên không tìm thấy hoặc không có gì thay đổi.");
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

        public async Task DeleteHoSoGiaoVienAsync(string hoSoGiaoVienID)
        {
            using (MySqlConnection conn = GetConnection())
            {
                await conn.OpenAsync();
                using (MySqlTransaction transaction = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        string query = "DELETE FROM HOSOGIAOVIEN WHERE HoSoGiaoVienID = @HoSoGiaoVienID";
                        using (MySqlCommand cmd = new MySqlCommand(query, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@HoSoGiaoVienID", hoSoGiaoVienID);
                            int rowsAffected = await cmd.ExecuteNonQueryAsync();
                            if (rowsAffected == 0) throw new Exception("Hồ sơ giáo viên không tìm thấy.");
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
        
        private HoSoGiaoVien MapReaderToHoSoGiaoVien(MySqlDataReader reader, Dictionary<string, int> ordinals)
        {
            return new HoSoGiaoVien
            {
                HoSoGiaoVienID = reader.GetString(ordinals["HoSoGiaoVienID"]),
                GiaoVienID = reader.GetString(ordinals["GiaoVienID"]),
                HoSoID = reader.GetString(ordinals["HoSoID"]),
                LopDayID = reader.IsDBNull(ordinals["LopDayID"]) ? null : reader.GetString(ordinals["LopDayID"]),
                NgayBatDauLamViec = reader.GetDateTime(ordinals["NgayBatDauLamViec"])
            };
        }

        private Dictionary<string, int> GetHoSoGiaoVienOrdinals(MySqlDataReader reader)
        {
             return new Dictionary<string, int> {
                {"HoSoGiaoVienID", reader.GetOrdinal("HoSoGiaoVienID")},
                {"GiaoVienID", reader.GetOrdinal("GiaoVienID")},
                {"HoSoID", reader.GetOrdinal("HoSoID")},
                {"LopDayID", reader.GetOrdinal("LopDayID")},
                {"NgayBatDauLamViec", reader.GetOrdinal("NgayBatDauLamViec")}
            };
        }

        public async Task<HoSoGiaoVien> GetHoSoGiaoVienByIdAsync(string hoSoGiaoVienID)
        {
            HoSoGiaoVien hsgv = null;
            string query = "SELECT * FROM HOSOGIAOVIEN WHERE HoSoGiaoVienID = @HoSoGiaoVienID";
            // Consider joining with GIAOVIEN, HOSO, LOP to populate navigation properties
            using (MySqlConnection conn = GetConnection())
            {
                await conn.OpenAsync();
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@HoSoGiaoVienID", hoSoGiaoVienID);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            var mysqlReader = (MySqlDataReader)reader;
                            var ordinals = GetHoSoGiaoVienOrdinals(mysqlReader);
                            hsgv = MapReaderToHoSoGiaoVien(mysqlReader, ordinals);
                        }
                    }
                }
            }
            return hsgv;
        }
        
        public async Task<HoSoGiaoVien> GetHoSoGiaoVienByGiaoVienIDAsync(string giaoVienID)
        {
            // Assuming one GiaoVien has one primary HoSoGiaoVien record. If multiple, return List.
            HoSoGiaoVien hsgv = null;
            string query = "SELECT * FROM HOSOGIAOVIEN WHERE GiaoVienID = @GiaoVienID LIMIT 1";
            using (MySqlConnection conn = GetConnection())
            {
                await conn.OpenAsync();
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@GiaoVienID", giaoVienID);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                           var mysqlReader = (MySqlDataReader)reader;
                           var ordinals = GetHoSoGiaoVienOrdinals(mysqlReader);
                           hsgv = MapReaderToHoSoGiaoVien(mysqlReader, ordinals);
                        }
                    }
                }
            }
            return hsgv;
        }
    }
}