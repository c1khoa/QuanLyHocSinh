using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using QuanLyHocSinh.Model.Entities;

namespace QuanLyHocSinh.Model.Relationships
{
        public class ChiTietQuyenDAL : BaseDAL
    {
        private static async Task<string> GenerateNewChiTietQuyenIDAsync(MySqlConnection conn, MySqlTransaction transaction)
        {
            string prefix = "CTQ"; // Chi Tiet Quyen
            // ChiTietQuyenID CHAR(8). "CTQ" + 5 digits
            string query = "SELECT ChiTietQuyenID FROM CHITIETQUYEN WHERE ChiTietQuyenID LIKE @PrefixPattern ORDER BY ChiTietQuyenID DESC LIMIT 1";
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
            return prefix + nextNumber.ToString("D5"); // CTQ00001
        }

        public async Task AddChiTietQuyenAsync(ChiTietQuyen chiTietQuyen)
        {
            using (MySqlConnection conn = GetConnection())
            {
                await conn.OpenAsync();
                using (MySqlTransaction transaction = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        chiTietQuyen.ChiTietQuyenID = await GenerateNewChiTietQuyenIDAsync(conn, transaction);
                        string query = @"INSERT INTO CHITIETQUYEN 
                                         (ChiTietQuyenID, QuyenID, VaiTroID, TuongTac) 
                                         VALUES (@ChiTietQuyenID, @QuyenID, @VaiTroID, @TuongTac)";
                        using (MySqlCommand cmd = new MySqlCommand(query, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@ChiTietQuyenID", chiTietQuyen.ChiTietQuyenID);
                            cmd.Parameters.AddWithValue("@QuyenID", chiTietQuyen.QuyenID);
                            cmd.Parameters.AddWithValue("@VaiTroID", chiTietQuyen.VaiTroID);
                            cmd.Parameters.AddWithValue("@TuongTac", chiTietQuyen.TuongTac);
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

        public async Task UpdateChiTietQuyenAsync(ChiTietQuyen chiTietQuyen)
        {
            using (MySqlConnection conn = GetConnection())
            {
                await conn.OpenAsync();
                using (MySqlTransaction transaction = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        string query = @"UPDATE CHITIETQUYEN SET 
                                         QuyenID = @QuyenID, VaiTroID = @VaiTroID, TuongTac = @TuongTac 
                                         WHERE ChiTietQuyenID = @ChiTietQuyenID";
                        using (MySqlCommand cmd = new MySqlCommand(query, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@QuyenID", chiTietQuyen.QuyenID);
                            cmd.Parameters.AddWithValue("@VaiTroID", chiTietQuyen.VaiTroID);
                            cmd.Parameters.AddWithValue("@TuongTac", chiTietQuyen.TuongTac);
                            cmd.Parameters.AddWithValue("@ChiTietQuyenID", chiTietQuyen.ChiTietQuyenID);
                            int rowsAffected = await cmd.ExecuteNonQueryAsync();
                            if (rowsAffected == 0) throw new Exception("Chi tiết quyền không tìm thấy hoặc không có gì thay đổi.");
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

        public async Task DeleteChiTietQuyenAsync(string chiTietQuyenID)
        {
            using (MySqlConnection conn = GetConnection())
            {
                await conn.OpenAsync();
                using (MySqlTransaction transaction = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        string query = "DELETE FROM CHITIETQUYEN WHERE ChiTietQuyenID = @ChiTietQuyenID";
                        using (MySqlCommand cmd = new MySqlCommand(query, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@ChiTietQuyenID", chiTietQuyenID);
                            int rowsAffected = await cmd.ExecuteNonQueryAsync();
                            if (rowsAffected == 0) throw new Exception("Chi tiết quyền không tìm thấy.");
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
        
        private ChiTietQuyen MapReaderToChiTietQuyen(MySqlDataReader reader, Dictionary<string, int> ordinals, bool includeNav = false)
        {
            var chiTietQuyen = new ChiTietQuyen
            {
                ChiTietQuyenID = reader.GetString(ordinals["ChiTietQuyenID"]),
                QuyenID = reader.GetString(ordinals["QuyenID"]),
                VaiTroID = reader.GetString(ordinals["VaiTroID"]),
                TuongTac = reader.GetString(ordinals["TuongTac"])
            };
            if (includeNav)
            {
                 if (ordinals.ContainsKey("TenQuyen") && !reader.IsDBNull(ordinals["TenQuyen"]))
                    chiTietQuyen.Quyen = new Quyen { QuyenID = chiTietQuyen.QuyenID, TenQuyen = reader.GetString(ordinals["TenQuyen"]) };
                
                if (ordinals.ContainsKey("TenVaiTro") && !reader.IsDBNull(ordinals["TenVaiTro"]))
                    chiTietQuyen.VaiTro = new VaiTro { VaiTroID = chiTietQuyen.VaiTroID, TenVaiTro = reader.GetString(ordinals["TenVaiTro"]) };
            }
            return chiTietQuyen;
        }
        
        private Dictionary<string, int> GetChiTietQuyenOrdinals(MySqlDataReader reader, bool includeNav = false)
        {
            var ordinals = new Dictionary<string, int> {
                {"ChiTietQuyenID", reader.GetOrdinal("ChiTietQuyenID")},
                {"QuyenID", reader.GetOrdinal("QuyenID")},
                {"VaiTroID", reader.GetOrdinal("VaiTroID")},
                {"TuongTac", reader.GetOrdinal("TuongTac")}
            };
            if(includeNav)
            {
                if(HasColumn(reader, "TenQuyen")) ordinals.Add("TenQuyen", reader.GetOrdinal("TenQuyen"));
                if(HasColumn(reader, "TenVaiTro")) ordinals.Add("TenVaiTro", reader.GetOrdinal("TenVaiTro"));
            }
            return ordinals;
        }
         private bool HasColumn(MySqlDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }


        public async Task<ChiTietQuyen> GetChiTietQuyenByIdAsync(string chiTietQuyenID)
        {
            ChiTietQuyen ctq = null;
            string query = @"SELECT ctq.*, q.TenQuyen, vt.TenVaiTro 
                             FROM CHITIETQUYEN ctq
                             LEFT JOIN QUYEN q ON ctq.QuyenID = q.QuyenID
                             LEFT JOIN VAITRO vt ON ctq.VaiTroID = vt.VaiTroID
                             WHERE ctq.ChiTietQuyenID = @ChiTietQuyenID";
            using (MySqlConnection conn = GetConnection())
            {
                await conn.OpenAsync();
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ChiTietQuyenID", chiTietQuyenID);
                    using (var dbReader = await cmd.ExecuteReaderAsync())
                    {
                        var mysqlReader = (MySqlDataReader)dbReader;
                        if (await mysqlReader.ReadAsync())
                        {
                            var ordinals = GetChiTietQuyenOrdinals(mysqlReader, true);
                            ctq = MapReaderToChiTietQuyen(mysqlReader, ordinals, true);
                        }
                    }
                }
            }
            return ctq;
        }

        public async Task<List<ChiTietQuyen>> GetChiTietQuyenByVaiTroIDAsync(string vaiTroID)
        {
            List<ChiTietQuyen> list = new List<ChiTietQuyen>();
            string query = @"SELECT ctq.*, q.TenQuyen, vt.TenVaiTro 
                             FROM CHITIETQUYEN ctq
                             LEFT JOIN QUYEN q ON ctq.QuyenID = q.QuyenID
                             LEFT JOIN VAITRO vt ON ctq.VaiTroID = vt.VaiTroID
                             WHERE ctq.VaiTroID = @VaiTroID";
            using (MySqlConnection conn = GetConnection())
            {
                await conn.OpenAsync();
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@VaiTroID", vaiTroID);
                    using (var dbReader = await cmd.ExecuteReaderAsync())
                    {
                        var mysqlReader = (MySqlDataReader)dbReader;
                        if(mysqlReader.HasRows)
                        {
                            var ordinals = GetChiTietQuyenOrdinals(mysqlReader, true);
                            while (await mysqlReader.ReadAsync())
                            {
                                list.Add(MapReaderToChiTietQuyen(mysqlReader, ordinals, true));
                            }
                        }
                    }
                }
            }
            return list;
        }
         public async Task<List<ChiTietQuyen>> GetChiTietQuyenByQuyenIDAsync(string quyenID)
        {
            List<ChiTietQuyen> list = new List<ChiTietQuyen>();
            string query = @"SELECT ctq.*, q.TenQuyen, vt.TenVaiTro 
                             FROM CHITIETQUYEN ctq
                             LEFT JOIN QUYEN q ON ctq.QuyenID = q.QuyenID
                             LEFT JOIN VAITRO vt ON ctq.VaiTroID = vt.VaiTroID
                             WHERE ctq.QuyenID = @QuyenID";
            using (MySqlConnection conn = GetConnection())
            {
                await conn.OpenAsync();
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@QuyenID", quyenID);
                    using (var dbReader = await cmd.ExecuteReaderAsync())
                    {
                        var mysqlReader = (MySqlDataReader)dbReader;
                         if(mysqlReader.HasRows)
                        {
                            var ordinals = GetChiTietQuyenOrdinals(mysqlReader, true);
                            while (await mysqlReader.ReadAsync())
                            {
                                list.Add(MapReaderToChiTietQuyen(mysqlReader, ordinals, true));
                            }
                        }
                    }
                }
            }
            return list;
        }
    }
}