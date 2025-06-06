using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using QuanLyHocSinh.Model.Entities;

namespace QuanLyHocSinh.Model.Relationships
{
        public class PhanQuyenDAL : BaseDAL
    {
        private static async Task<string> GenerateNewPhanQuyenIDAsync(MySqlConnection conn, MySqlTransaction transaction)
        {
            string prefix = "PQ"; // Phan Quyen
            // PhanQuyenID CHAR(8). "PQ" + 6 digits = 8 chars
            string query = "SELECT PhanQuyenID FROM PHANQUYEN WHERE PhanQuyenID LIKE @PrefixPattern ORDER BY PhanQuyenID DESC LIMIT 1";
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
            return prefix + nextNumber.ToString("D6"); // PQ000001
        }

        public async Task AddPhanQuyenAsync(PhanQuyen phanQuyen)
        {
            using (MySqlConnection conn = GetConnection())
            {
                await conn.OpenAsync();
                using (MySqlTransaction transaction = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        phanQuyen.PhanQuyenID = await GenerateNewPhanQuyenIDAsync(conn, transaction);
                        string query = @"INSERT INTO PHANQUYEN 
                                         (PhanQuyenID, QuyenID, GiaoVuPhanQuyenID, UserDuocPhanQuyenID, NgayPhanQuyen) 
                                         VALUES (@PhanQuyenID, @QuyenID, @GiaoVuPhanQuyenID, @UserDuocPhanQuyenID, @NgayPhanQuyen)";
                        using (MySqlCommand cmd = new MySqlCommand(query, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@PhanQuyenID", phanQuyen.PhanQuyenID);
                            cmd.Parameters.AddWithValue("@QuyenID", phanQuyen.QuyenID);
                            cmd.Parameters.AddWithValue("@GiaoVuPhanQuyenID", phanQuyen.GiaoVuPhanQuyenID);
                            cmd.Parameters.AddWithValue("@UserDuocPhanQuyenID", phanQuyen.UserDuocPhanQuyenID);
                            cmd.Parameters.AddWithValue("@NgayPhanQuyen", phanQuyen.NgayPhanQuyen);
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

        public async Task UpdatePhanQuyenAsync(PhanQuyen phanQuyen)
        {
            using (MySqlConnection conn = GetConnection())
            {
                await conn.OpenAsync();
                using (MySqlTransaction transaction = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        string query = @"UPDATE PHANQUYEN SET 
                                         QuyenID = @QuyenID, GiaoVuPhanQuyenID = @GiaoVuPhanQuyenID, 
                                         UserDuocPhanQuyenID = @UserDuocPhanQuyenID, NgayPhanQuyen = @NgayPhanQuyen 
                                         WHERE PhanQuyenID = @PhanQuyenID";
                        using (MySqlCommand cmd = new MySqlCommand(query, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@QuyenID", phanQuyen.QuyenID);
                            cmd.Parameters.AddWithValue("@GiaoVuPhanQuyenID", phanQuyen.GiaoVuPhanQuyenID);
                            cmd.Parameters.AddWithValue("@UserDuocPhanQuyenID", phanQuyen.UserDuocPhanQuyenID);
                            cmd.Parameters.AddWithValue("@NgayPhanQuyen", phanQuyen.NgayPhanQuyen);
                            cmd.Parameters.AddWithValue("@PhanQuyenID", phanQuyen.PhanQuyenID);
                            int rowsAffected = await cmd.ExecuteNonQueryAsync();
                            if (rowsAffected == 0) throw new Exception("Phân quyền không tìm thấy hoặc không có gì thay đổi.");
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

        public async Task DeletePhanQuyenAsync(string phanQuyenID)
        {
            using (MySqlConnection conn = GetConnection())
            {
                await conn.OpenAsync();
                using (MySqlTransaction transaction = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        string query = "DELETE FROM PHANQUYEN WHERE PhanQuyenID = @PhanQuyenID";
                        using (MySqlCommand cmd = new MySqlCommand(query, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@PhanQuyenID", phanQuyenID);
                            int rowsAffected = await cmd.ExecuteNonQueryAsync();
                            if (rowsAffected == 0) throw new Exception("Phân quyền không tìm thấy.");
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
        
        private PhanQuyen MapReaderToPhanQuyen(MySqlDataReader reader, Dictionary<string, int> ordinals, bool includeNav = false)
        {
            var phanQuyen = new PhanQuyen
            {
                PhanQuyenID = reader.GetString(ordinals["PhanQuyenID"]),
                QuyenID = reader.GetString(ordinals["QuyenID"]),
                GiaoVuPhanQuyenID = reader.GetString(ordinals["GiaoVuPhanQuyenID"]),
                UserDuocPhanQuyenID = reader.GetString(ordinals["UserDuocPhanQuyenID"]),
                NgayPhanQuyen = reader.GetDateTime(ordinals["NgayPhanQuyen"])
            };

            if (includeNav)
            {
                if (ordinals.ContainsKey("TenQuyen") && !reader.IsDBNull(ordinals["TenQuyen"]))
                    phanQuyen.Quyen = new Quyen { QuyenID = phanQuyen.QuyenID, TenQuyen = reader.GetString(ordinals["TenQuyen"]) };
                
                // GiaoVu navigation (TenDangNhap as a simple representation)
                if (ordinals.ContainsKey("TenGiaoVu") && !reader.IsDBNull(ordinals["TenGiaoVu"]))
                     phanQuyen.GiaoVuPhanQuyen = new GiaoVu { GiaoVuID = phanQuyen.GiaoVuPhanQuyenID, TenDangNhap = reader.GetString(ordinals["TenGiaoVu"]) };


                // User navigation (TenDangNhap as a simple representation)
                if (ordinals.ContainsKey("TenUserDuocPhanQuyen") && !reader.IsDBNull(ordinals["TenUserDuocPhanQuyen"]))
                    phanQuyen.UserDuocPhanQuyen = new User { UserID = phanQuyen.UserDuocPhanQuyenID, TenDangNhap = reader.GetString(ordinals["TenUserDuocPhanQuyen"]) };
            }
            return phanQuyen;
        }

        private Dictionary<string, int> GetPhanQuyenOrdinals(MySqlDataReader reader, bool includeNav = false)
        {
            var ordinals = new Dictionary<string, int> {
                {"PhanQuyenID", reader.GetOrdinal("PhanQuyenID")},
                {"QuyenID", reader.GetOrdinal("QuyenID")},
                {"GiaoVuPhanQuyenID", reader.GetOrdinal("GiaoVuPhanQuyenID")},
                {"UserDuocPhanQuyenID", reader.GetOrdinal("UserDuocPhanQuyenID")},
                {"NgayPhanQuyen", reader.GetOrdinal("NgayPhanQuyen")}
            };
            if (includeNav)
            {
                if (HasColumn(reader, "TenQuyen")) ordinals.Add("TenQuyen", reader.GetOrdinal("TenQuyen"));
                if (HasColumn(reader, "TenGiaoVu")) ordinals.Add("TenGiaoVu", reader.GetOrdinal("TenGiaoVu"));
                if (HasColumn(reader, "TenUserDuocPhanQuyen")) ordinals.Add("TenUserDuocPhanQuyen", reader.GetOrdinal("TenUserDuocPhanQuyen"));
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

        public async Task<PhanQuyen> GetPhanQuyenByIdAsync(string phanQuyenID)
        {
            PhanQuyen pq = null;
            string query = @"SELECT pq.*, q.TenQuyen, gv_user.TenDangNhap AS TenGiaoVu, u_pq.TenDangNhap AS TenUserDuocPhanQuyen
                             FROM PHANQUYEN pq
                             LEFT JOIN QUYEN q ON pq.QuyenID = q.QuyenID
                             LEFT JOIN GIAOVU gv ON pq.GiaoVuPhanQuyenID = gv.GiaoVuID
                             LEFT JOIN USERS gv_user ON gv.UserID = gv_user.UserID
                             LEFT JOIN USERS u_pq ON pq.UserDuocPhanQuyenID = u_pq.UserID
                             WHERE pq.PhanQuyenID = @PhanQuyenID";
            using (MySqlConnection conn = GetConnection())
            {
                await conn.OpenAsync();
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@PhanQuyenID", phanQuyenID);
                    using (var dbReader = await cmd.ExecuteReaderAsync())
                    {
                        var mysqlReader = (MySqlDataReader)dbReader;
                        if (await mysqlReader.ReadAsync())
                        {
                            var ordinals = GetPhanQuyenOrdinals(mysqlReader, true);
                            pq = MapReaderToPhanQuyen(mysqlReader, ordinals, true);
                        }
                    }
                }
            }
            return pq;
        }

        public async Task<List<PhanQuyen>> GetPhanQuyenByUserDuocPhanQuyenIDAsync(string userDuocPhanQuyenID)
        {
            List<PhanQuyen> list = new List<PhanQuyen>();
            string query = @"SELECT pq.*, q.TenQuyen, gv_user.TenDangNhap AS TenGiaoVu, u_pq.TenDangNhap AS TenUserDuocPhanQuyen
                             FROM PHANQUYEN pq
                             LEFT JOIN QUYEN q ON pq.QuyenID = q.QuyenID
                             LEFT JOIN GIAOVU gv ON pq.GiaoVuPhanQuyenID = gv.GiaoVuID
                             LEFT JOIN USERS gv_user ON gv.UserID = gv_user.UserID
                             LEFT JOIN USERS u_pq ON pq.UserDuocPhanQuyenID = u_pq.UserID
                             WHERE pq.UserDuocPhanQuyenID = @UserDuocPhanQuyenID";
            using (MySqlConnection conn = GetConnection())
            {
                await conn.OpenAsync();
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserDuocPhanQuyenID", userDuocPhanQuyenID);
                    using (var dbReader = await cmd.ExecuteReaderAsync())
                    {
                         var mysqlReader = (MySqlDataReader)dbReader;
                        if(mysqlReader.HasRows)
                        {
                            var ordinals = GetPhanQuyenOrdinals(mysqlReader, true);
                            while (await mysqlReader.ReadAsync())
                            {
                                list.Add(MapReaderToPhanQuyen(mysqlReader, ordinals, true));
                            }
                        }
                    }
                }
            }
            return list;
        }
    }
}