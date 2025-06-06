using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using QuanLyHocSinh.Model.Entities;
using System.Threading.Tasks;

namespace QuanLyHocSinh.Model.Entities
{
        public class QuyenDAL : BaseDAL
    {
        private static async Task<string> GenerateNewQuyenIDAsync(MySqlConnection conn, MySqlTransaction transaction)
        {
            string prefix = "Q"; // Quyen
            // QuyenID là CHAR(8), sample data Q001. Giả sử Q + 7 chữ số.
            string query = "SELECT QuyenID FROM QUYEN WHERE QuyenID LIKE @PrefixPattern ORDER BY QuyenID DESC LIMIT 1";
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
            // QuyenID CHAR(8), "Q" + 7 digits
            return prefix + nextNumber.ToString("D7"); 
        }

        public async Task<List<Quyen>> GetAllQuyenAsync()
        {
            List<Quyen> quyens = new List<Quyen>();
            string query = "SELECT QuyenID, TenQuyen, MoTa FROM QUYEN";
            using (MySqlConnection conn = GetConnection())
            {
                await conn.OpenAsync();
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        int idOrdinal = reader.GetOrdinal("QuyenID");
                        int tenOrdinal = reader.GetOrdinal("TenQuyen");
                        int moTaOrdinal = reader.GetOrdinal("MoTa");
                        while (await reader.ReadAsync())
                        {
                            quyens.Add(new Quyen
                            {
                                QuyenID = reader.GetString(idOrdinal),
                                TenQuyen = reader.GetString(tenOrdinal),
                                MoTa = reader.GetString(moTaOrdinal)
                            });
                        }
                    }
                }
            }
            return quyens;
        }

        public async Task<Quyen> GetQuyenByIdAsync(string quyenID)
        {
            Quyen quyen = null;
            string query = "SELECT QuyenID, TenQuyen, MoTa FROM QUYEN WHERE QuyenID = @QuyenID";
            using (MySqlConnection conn = GetConnection())
            {
                await conn.OpenAsync();
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@QuyenID", quyenID);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            int idOrdinal = reader.GetOrdinal("QuyenID");
                            int tenOrdinal = reader.GetOrdinal("TenQuyen");
                            int moTaOrdinal = reader.GetOrdinal("MoTa");
                            quyen = new Quyen
                            {
                                QuyenID = reader.GetString(idOrdinal),
                                TenQuyen = reader.GetString(tenOrdinal),
                                MoTa = reader.GetString(moTaOrdinal)
                            };
                        }
                    }
                }
            }
            return quyen;
        }

        public async Task AddQuyenAsync(Quyen quyen)
        {
            using (MySqlConnection conn = GetConnection())
            {
                await conn.OpenAsync();
                using (MySqlTransaction transaction = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        quyen.QuyenID = await GenerateNewQuyenIDAsync(conn, transaction);
                        string query = "INSERT INTO QUYEN (QuyenID, TenQuyen, MoTa) VALUES (@QuyenID, @TenQuyen, @MoTa)";
                        using (MySqlCommand cmd = new MySqlCommand(query, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@QuyenID", quyen.QuyenID);
                            cmd.Parameters.AddWithValue("@TenQuyen", quyen.TenQuyen);
                            cmd.Parameters.AddWithValue("@MoTa", quyen.MoTa);
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

        public async Task UpdateQuyenAsync(Quyen quyen)
        {
            using (MySqlConnection conn = GetConnection())
            {
                await conn.OpenAsync();
                using (MySqlTransaction transaction = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        string query = "UPDATE QUYEN SET TenQuyen = @TenQuyen, MoTa = @MoTa WHERE QuyenID = @QuyenID";
                        using (MySqlCommand cmd = new MySqlCommand(query, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@TenQuyen", quyen.TenQuyen);
                            cmd.Parameters.AddWithValue("@MoTa", quyen.MoTa);
                            cmd.Parameters.AddWithValue("@QuyenID", quyen.QuyenID);
                             int rowsAffected = await cmd.ExecuteNonQueryAsync();
                            if (rowsAffected == 0) throw new Exception("Quyền không tìm thấy hoặc không có gì thay đổi.");
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

        public async Task DeleteQuyenAsync(string quyenID)
        {
            using (MySqlConnection conn = GetConnection())
            {
                await conn.OpenAsync();
                using (MySqlTransaction transaction = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        // Cân nhắc: Kiểm tra CHITIETQUYEN, PHANQUYEN
                        string query = "DELETE FROM QUYEN WHERE QuyenID = @QuyenID";
                        using (MySqlCommand cmd = new MySqlCommand(query, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@QuyenID", quyenID);
                            int rowsAffected = await cmd.ExecuteNonQueryAsync();
                            if (rowsAffected == 0) throw new Exception("Quyền không tìm thấy.");
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
    }
}