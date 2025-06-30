using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using QuanLyHocSinh.Model.Entities;
using QuanLyHocSinh.Model.Relationships;

namespace QuanLyHocSinh.Model.DAL
{
    public class HoSoGiaoVuDAL : BaseDAL
    {
        private static async Task<string> GenerateNewHoSoGiaoVuIDAsync(MySqlConnection conn, MySqlTransaction transaction)
        {
            string prefix = "HGU"; // HoSoGiaoVu
            string query = "SELECT HoSoGiaoVuID FROM HOSOGIAOVU WHERE HoSoGiaoVuID LIKE @PrefixPattern ORDER BY HoSoGiaoVuID DESC LIMIT 1";
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
            return prefix + nextNumber.ToString("D7"); // CHAR(10) total
        }

        public async Task AddHoSoGiaoVuAsync(HoSoGiaoVu hoSoGiaoVu)
        {
            using (MySqlConnection conn = GetConnection())
            {
                await conn.OpenAsync();
                using (MySqlTransaction transaction = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        hoSoGiaoVu.HoSoGiaoVuID = await GenerateNewHoSoGiaoVuIDAsync(conn, transaction);
                        string query = @"INSERT INTO HOSOGIAOVU 
                                         (HoSoGiaoVuID, GiaoVuID, HoSoID) 
                                         VALUES (@HoSoGiaoVuID, @GiaoVuID, @HoSoID)";
                        using (MySqlCommand cmd = new MySqlCommand(query, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@HoSoGiaoVuID", hoSoGiaoVu.HoSoGiaoVuID);
                            cmd.Parameters.AddWithValue("@GiaoVuID", hoSoGiaoVu.GiaoVuID);
                            cmd.Parameters.AddWithValue("@HoSoID", hoSoGiaoVu.HoSoID);
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
        
        public async Task<HoSoGiaoVu> GetHoSoGiaoVuByIdAsync(string hoSoGiaoVuID)
        {
            HoSoGiaoVu hsgvu = null;
            string query = "SELECT * FROM HOSOGIAOVU WHERE HoSoGiaoVuID = @HoSoGiaoVuID";
            
            using (MySqlConnection conn = GetConnection())
            {
                await conn.OpenAsync();
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@HoSoGiaoVuID", hoSoGiaoVuID);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            hsgvu = new HoSoGiaoVu
                            {
                                HoSoGiaoVuID = reader["HoSoGiaoVuID"].ToString(),
                                GiaoVuID = reader["GiaoVuID"].ToString(),
                                HoSoID = reader["HoSoID"].ToString()
                            };
                        }
                    }
                }
            }
            return hsgvu;
        }

        public async Task<HoSoGiaoVu> GetHoSoGiaoVuByGiaoVuIDAsync(string giaoVuID)
        {
            HoSoGiaoVu hsgvu = null;
            string query = "SELECT * FROM HOSOGIAOVU WHERE GiaoVuID = @GiaoVuID LIMIT 1";
            
            using (MySqlConnection conn = GetConnection())
            {
                await conn.OpenAsync();
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@GiaoVuID", giaoVuID);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            hsgvu = new HoSoGiaoVu
                            {
                                HoSoGiaoVuID = reader["HoSoGiaoVuID"].ToString(),
                                GiaoVuID = reader["GiaoVuID"].ToString(),
                                HoSoID = reader["HoSoID"].ToString()
                            };
                        }
                    }
                }
            }
            return hsgvu;
        }
    }
} 