using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using QuanLyHocSinh.Model.Entities;

namespace QuanLyHocSinh.Model.Relationships
{
    public class ChiTietMonHocDAL : BaseDAL
    {
        private static async Task<string> GenerateNewChiTietMonHocIDAsync(MySqlConnection conn, MySqlTransaction transaction)
        {
            string prefix = "CTMH"; 
            string query = "SELECT ChiTietMonHocID FROM CHITIETMONHOC WHERE ChiTietMonHocID LIKE @PrefixPattern ORDER BY ChiTietMonHocID DESC LIMIT 1";
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
            return prefix + nextNumber.ToString("D4"); 
        }

        public async Task AddChiTietMonHocAsync(ChiTietMonHoc chiTietMonHoc)
        {
            using (MySqlConnection conn = GetConnection())
            {
                await conn.OpenAsync();
                using (MySqlTransaction transaction = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        chiTietMonHoc.ChiTietMonHocID = await GenerateNewChiTietMonHocIDAsync(conn, transaction);
                        string query = @"INSERT INTO CHITIETMONHOC 
                                         (ChiTietMonHocID, GiaoVienID, MonHocID, LopDayID, NgayDay, NoiDungDay) 
                                         VALUES (@ChiTietMonHocID, @GiaoVienID, @MonHocID, @LopDayID, @NgayDay, @NoiDungDay)";
                        using (MySqlCommand cmd = new MySqlCommand(query, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@ChiTietMonHocID", chiTietMonHoc.ChiTietMonHocID);
                            cmd.Parameters.AddWithValue("@GiaoVienID", chiTietMonHoc.GiaoVienID);
                            cmd.Parameters.AddWithValue("@MonHocID", chiTietMonHoc.MonHocID);
                            cmd.Parameters.AddWithValue("@LopDayID", chiTietMonHoc.LopDayID);
                            cmd.Parameters.AddWithValue("@NgayDay", chiTietMonHoc.NgayDay);
                            cmd.Parameters.AddWithValue("@NoiDungDay", (object)chiTietMonHoc.NoiDungDay ?? DBNull.Value);
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

        public async Task UpdateChiTietMonHocAsync(ChiTietMonHoc chiTietMonHoc)
        {
            using (MySqlConnection conn = GetConnection())
            {
                await conn.OpenAsync();
                using (MySqlTransaction transaction = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        string query = @"UPDATE CHITIETMONHOC SET 
                                         GiaoVienID = @GiaoVienID, MonHocID = @MonHocID, LopDayID = @LopDayID, 
                                         NgayDay = @NgayDay, NoiDungDay = @NoiDungDay 
                                         WHERE ChiTietMonHocID = @ChiTietMonHocID";
                        using (MySqlCommand cmd = new MySqlCommand(query, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@GiaoVienID", chiTietMonHoc.GiaoVienID);
                            cmd.Parameters.AddWithValue("@MonHocID", chiTietMonHoc.MonHocID);
                            cmd.Parameters.AddWithValue("@LopDayID", chiTietMonHoc.LopDayID);
                            cmd.Parameters.AddWithValue("@NgayDay", chiTietMonHoc.NgayDay);
                            cmd.Parameters.AddWithValue("@NoiDungDay", (object)chiTietMonHoc.NoiDungDay ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@ChiTietMonHocID", chiTietMonHoc.ChiTietMonHocID);
                            int rowsAffected = await cmd.ExecuteNonQueryAsync();
                            if (rowsAffected == 0) throw new Exception("Chi tiết môn học không tìm thấy hoặc không có gì thay đổi.");
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

        public async Task DeleteChiTietMonHocAsync(string chiTietMonHocID)
        {
            using (MySqlConnection conn = GetConnection())
            {
                await conn.OpenAsync();
                using (MySqlTransaction transaction = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        string query = "DELETE FROM CHITIETMONHOC WHERE ChiTietMonHocID = @ChiTietMonHocID";
                        using (MySqlCommand cmd = new MySqlCommand(query, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@ChiTietMonHocID", chiTietMonHocID);
                            int rowsAffected = await cmd.ExecuteNonQueryAsync();
                            if (rowsAffected == 0) throw new Exception("Chi tiết môn học không tìm thấy.");
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
        
        // Phương thức helper này nên nhận MySqlDataReader
        private ChiTietMonHoc MapReaderToChiTietMonHoc(MySqlDataReader reader, Dictionary<string, int> ordinals)
        {
            return new ChiTietMonHoc
            {
                ChiTietMonHocID = reader.GetString(ordinals["ChiTietMonHocID"]),
                GiaoVienID = reader.IsDBNull(ordinals["GiaoVienID"]) ? null : reader.GetString(ordinals["GiaoVienID"]),
                MonHocID = reader.GetString(ordinals["MonHocID"]),
                LopDayID = reader.GetString(ordinals["LopDayID"]),
                NgayDay = reader.GetDateTime(ordinals["NgayDay"]),
                NoiDungDay = reader.IsDBNull(ordinals["NoiDungDay"]) ? null : reader.GetString(ordinals["NoiDungDay"]),
                GiaoVien = !reader.IsDBNull(ordinals["GiaoVienID"]) && !reader.IsDBNull(ordinals["TenGV"]) ? new GiaoVien { MaGV = reader.GetString(ordinals["GiaoVienID"]), HoTen = reader.GetString(ordinals["TenGV"]) } : null,
                MonHoc = !reader.IsDBNull(ordinals["MonHocID"]) && !reader.IsDBNull(ordinals["TenMonHoc"]) ? new MonHoc { MonHocID = reader.GetString(ordinals["MonHocID"]), TenMonHoc = reader.GetString(ordinals["TenMonHoc"]) } : null,
                Lop = !reader.IsDBNull(ordinals["LopDayID"]) && !reader.IsDBNull(ordinals["TenLop"]) ? new Lop { LopID = reader.GetString(ordinals["LopDayID"]), TenLop = reader.GetString(ordinals["TenLop"]) } : null
            };
        }
        
        // Phương thức helper này nên nhận MySqlDataReader
        private Dictionary<string, int> GetChiTietMonHocOrdinals(MySqlDataReader reader, bool includeNavProps = false)
        {
            var ordinals = new Dictionary<string, int>
            {
                { "ChiTietMonHocID", reader.GetOrdinal("ChiTietMonHocID") },
                { "GiaoVienID", reader.GetOrdinal("GiaoVienID") },
                { "MonHocID", reader.GetOrdinal("MonHocID") },
                { "LopDayID", reader.GetOrdinal("LopDayID") },
                { "NgayDay", reader.GetOrdinal("NgayDay") },
                { "NoiDungDay", reader.GetOrdinal("NoiDungDay") }
            };
            if (includeNavProps)
            {
                // Kiểm tra xem các cột này có tồn tại không trước khi GetOrdinal nếu query có thể không chứa chúng
                if (HasColumn(reader, "TenGV")) ordinals.Add("TenGV", reader.GetOrdinal("TenGV")); else ordinals.Add("TenGV", -1); // Gán -1 nếu không có để check sau
                if (HasColumn(reader, "TenMonHoc")) ordinals.Add("TenMonHoc", reader.GetOrdinal("TenMonHoc")); else ordinals.Add("TenMonHoc", -1);
                if (HasColumn(reader, "TenLop")) ordinals.Add("TenLop", reader.GetOrdinal("TenLop")); else ordinals.Add("TenLop", -1);

            }
            return ordinals;
        }
        // Helper để kiểm tra cột có tồn tại không
        private bool HasColumn(MySqlDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }


        public async Task<ChiTietMonHoc> GetChiTietMonHocByIdAsync(string chiTietMonHocID)
        {
            ChiTietMonHoc chiTiet = null;
            string query = @"
                SELECT ctmh.*, 
                       gv_hoso.HoTen AS TenGV, 
                       mh.TenMonHoc, 
                       l.TenLop
                FROM CHITIETMONHOC ctmh
                LEFT JOIN GIAOVIEN gv ON ctmh.GiaoVienID = gv.GiaoVienID
                LEFT JOIN HOSOGIAOVIEN hsgv ON gv.GiaoVienID = hsgv.GiaoVienID
                LEFT JOIN HOSO gv_hoso ON hsgv.HoSoID = gv_hoso.HoSoID
                LEFT JOIN MONHOC mh ON ctmh.MonHocID = mh.MonHocID
                LEFT JOIN LOP l ON ctmh.LopDayID = l.LopID
                WHERE ctmh.ChiTietMonHocID = @ChiTietMonHocID";
            
            using (MySqlConnection conn = GetConnection())
            {
                await conn.OpenAsync();
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ChiTietMonHocID", chiTietMonHocID);
                    using (var dbReader = await cmd.ExecuteReaderAsync()) // dbReader là DbDataReader
                    {
                        if (await dbReader.ReadAsync())
                        {
                            var mysqlReader = (MySqlDataReader)dbReader; // Ép kiểu ở đây
                            var ordinals = GetChiTietMonHocOrdinals(mysqlReader, true);
                            chiTiet = MapReaderToChiTietMonHoc(mysqlReader, ordinals);
                        }
                    }
                }
            }
            return chiTiet;
        }

        public async Task<List<ChiTietMonHoc>> GetChiTietMonHocByFiltersAsync(
            string lopID = null, string giaoVienID = null, string monHocID = null, 
            DateTime? fromDate = null, DateTime? toDate = null)
        {
            List<ChiTietMonHoc> chiTiets = new List<ChiTietMonHoc>();
             string query = @"
                SELECT ctmh.*, 
                       gv_hoso.HoTen AS TenGV, 
                       mh.TenMonHoc, 
                       l.TenLop
                FROM CHITIETMONHOC ctmh
                LEFT JOIN GIAOVIEN gv ON ctmh.GiaoVienID = gv.GiaoVienID
                LEFT JOIN HOSOGIAOVIEN hsgv ON gv.GiaoVienID = hsgv.GiaoVienID
                LEFT JOIN HOSO gv_hoso ON hsgv.HoSoID = gv_hoso.HoSoID
                LEFT JOIN MONHOC mh ON ctmh.MonHocID = mh.MonHocID
                LEFT JOIN LOP l ON ctmh.LopDayID = l.LopID
                WHERE 1=1"; 

            if (!string.IsNullOrEmpty(lopID)) query += " AND ctmh.LopDayID = @LopDayID";
            if (!string.IsNullOrEmpty(giaoVienID)) query += " AND ctmh.GiaoVienID = @GiaoVienID";
            if (!string.IsNullOrEmpty(monHocID)) query += " AND ctmh.MonHocID = @MonHocID";
            if (fromDate.HasValue) query += " AND ctmh.NgayDay >= @FromDate";
            if (toDate.HasValue) query += " AND ctmh.NgayDay <= @ToDate";
            query += " ORDER BY ctmh.NgayDay DESC";

            using (MySqlConnection conn = GetConnection())
            {
                await conn.OpenAsync();
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(lopID)) cmd.Parameters.AddWithValue("@LopDayID", lopID);
                    if (!string.IsNullOrEmpty(giaoVienID)) cmd.Parameters.AddWithValue("@GiaoVienID", giaoVienID);
                    if (!string.IsNullOrEmpty(monHocID)) cmd.Parameters.AddWithValue("@MonHocID", monHocID);
                    if (fromDate.HasValue) cmd.Parameters.AddWithValue("@FromDate", fromDate.Value);
                    if (toDate.HasValue) cmd.Parameters.AddWithValue("@ToDate", toDate.Value);
                    
                    using (var dbReader = await cmd.ExecuteReaderAsync()) // dbReader là DbDataReader
                    {
                        if (dbReader.HasRows) 
                        {
                            var mysqlReader = (MySqlDataReader)dbReader; // Ép kiểu ở đây
                            var ordinals = GetChiTietMonHocOrdinals(mysqlReader, true);
                            while (await dbReader.ReadAsync()) // Vẫn dùng dbReader cho ReadAsync
                            {
                                // Truyền mysqlReader đã ép kiểu vào MapReaderToChiTietMonHoc
                                chiTiets.Add(MapReaderToChiTietMonHoc(mysqlReader, ordinals));
                            }
                        }
                    }
                }
            }
            return chiTiets;
        }
    }
}