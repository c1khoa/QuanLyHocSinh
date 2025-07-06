using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using QuanLyHocSinh.Model.Entities;
using System.Threading.Tasks;
using System.Configuration;
namespace QuanLyHocSinh.Model.Entities
{
    public class MonHocDAL : BaseDAL
    {
        private static async Task<string> GenerateNewMonHocIDAsync(MySqlConnection conn, MySqlTransaction transaction)
        {
            string prefix = "MH"; // Mon Hoc
            string query = "SELECT MonHocID FROM MONHOC WHERE MonHocID LIKE @PrefixPattern ORDER BY MonHocID DESC LIMIT 1";
            int nextNumber = 1;

            using (MySqlCommand cmd = new MySqlCommand(query, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@PrefixPattern", prefix + "%");
                var result = await cmd.ExecuteScalarAsync();
                if (result != null && result != DBNull.Value)
                {
                    string lastId = result.ToString();
                    if (lastId.StartsWith(prefix) && lastId.Length == 4) // MHxx
                    {
                        if (int.TryParse(lastId.Substring(prefix.Length), out int num))
                        {
                            nextNumber = num + 1;
                        }
                    }
                }
            }
            return prefix + nextNumber.ToString("D2"); // Ensures MH01, MH10 etc.
        }
        // MonHocDAL.cs
        public static string? LayTenMonHocTheoID(string monHocID)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT TenMonHoc FROM MONHOC WHERE MonHocID = @MonHocID";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@MonHocID", monHocID);
                    var result = cmd.ExecuteScalar();
                    return result?.ToString();
                }
            }
        }


        public async Task<List<MonHoc>> GetAllMonHocAsync()
        {
            List<MonHoc> monHocs = new List<MonHoc>();
            string query = "SELECT MonHocID, TenMonHoc FROM MONHOC";
            using (MySqlConnection conn = GetConnection())
            {
                await conn.OpenAsync();
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        int monHocIDOrdinal = reader.GetOrdinal("MonHocID");
                        int tenMonHocOrdinal = reader.GetOrdinal("TenMonHoc");
                        while (await reader.ReadAsync())
                        {
                            monHocs.Add(new MonHoc
                            {
                                MonHocID = reader.GetString(monHocIDOrdinal),
                                TenMonHoc = reader.GetString(tenMonHocOrdinal)
                            });
                        }
                    }
                }
            }
            return monHocs;
        }

        public async Task<MonHoc> GetMonHocByIdAsync(string monHocID)
        {
            MonHoc monHoc = null;
            string query = "SELECT MonHocID, TenMonHoc FROM MONHOC WHERE MonHocID = @MonHocID";
            using (MySqlConnection conn = GetConnection())
            {
                await conn.OpenAsync();
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@MonHocID", monHocID);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            int monHocIDOrdinal = reader.GetOrdinal("MonHocID");
                            int tenMonHocOrdinal = reader.GetOrdinal("TenMonHoc");
                            monHoc = new MonHoc
                            {
                                MonHocID = reader.GetString(monHocIDOrdinal),
                                TenMonHoc = reader.GetString(tenMonHocOrdinal)
                            };
                        }
                    }
                }
            }
            return monHoc;
        }

        public async Task AddMonHocAsync(MonHoc monHoc)
        {
            using (MySqlConnection conn = GetConnection())
            {
                await conn.OpenAsync();
                using (MySqlTransaction transaction = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        monHoc.MonHocID = await GenerateNewMonHocIDAsync(conn, transaction);
                        string query = "INSERT INTO MONHOC (MonHocID, TenMonHoc) VALUES (@MonHocID, @TenMonHoc)";
                        using (MySqlCommand cmd = new MySqlCommand(query, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@MonHocID", monHoc.MonHocID);
                            cmd.Parameters.AddWithValue("@TenMonHoc", monHoc.TenMonHoc);
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

        public async Task UpdateMonHocAsync(MonHoc monHoc)
        {
            using (MySqlConnection conn = GetConnection())
            {
                await conn.OpenAsync();
                using (MySqlTransaction transaction = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        string query = "UPDATE MONHOC SET TenMonHoc = @TenMonHoc WHERE MonHocID = @MonHocID";
                        using (MySqlCommand cmd = new MySqlCommand(query, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@TenMonHoc", monHoc.TenMonHoc);
                            cmd.Parameters.AddWithValue("@MonHocID", monHoc.MonHocID);
                            int rowsAffected = await cmd.ExecuteNonQueryAsync();
                            if (rowsAffected == 0) throw new Exception("Môn học không tìm thấy hoặc không có gì thay đổi.");
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

        public async Task DeleteMonHocAsync(string monHocID)
        {
            using (MySqlConnection conn = GetConnection())
            {
                await conn.OpenAsync();
                using (MySqlTransaction transaction = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        // Cân nhắc: Kiểm tra các bảng tham chiếu (DIEM, CHITIETMONHOC) trước khi xóa
                        // hoặc đảm bảo ON DELETE được cấu hình phù hợp trong DB.
                        string query = "DELETE FROM MONHOC WHERE MonHocID = @MonHocID";
                        using (MySqlCommand cmd = new MySqlCommand(query, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@MonHocID", monHocID);
                            int rowsAffected = await cmd.ExecuteNonQueryAsync();
                            if (rowsAffected == 0) throw new Exception("Môn học không tìm thấy.");
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

        public async Task<string> GetMonHocIdByNameAsync(string tenMonHoc)
        {
            string monHocID = null;
            string query = "SELECT MonHocID FROM MONHOC WHERE TenMonHoc = @TenMonHoc";
            using (MySqlConnection conn = GetConnection())
            {
                await conn.OpenAsync();
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@TenMonHoc", tenMonHoc);
                    var result = await cmd.ExecuteScalarAsync();
                    if (result != null && result != DBNull.Value)
                    {
                        monHocID = result.ToString();
                    }
                }
            }
            return monHocID;
        }
        public static void UpdateDanhSachMonHoc(List<MonHoc> danhSachMonHoc)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                // Bắt đầu một transaction để đảm bảo tất cả cập nhật thành công hoặc không gì cả
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string query = "UPDATE MONHOC SET TenMonHoc = @TenMonHoc WHERE MonHocID = @MonHocID";
                        
                        foreach (var monHoc in danhSachMonHoc)
                        {
                            using (var cmd = new MySqlCommand(query, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@TenMonHoc", monHoc.TenMonHoc);
                                cmd.Parameters.AddWithValue("@MonHocID", monHoc.MonHocID);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        
                        // Nếu không có lỗi, commit transaction để lưu tất cả thay đổi
                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        // Nếu có lỗi, rollback để hủy bỏ tất cả thay đổi đã thực hiện
                        transaction.Rollback();
                        throw; // Ném lại exception để lớp ViewModel có thể bắt và thông báo
                    }
                }
            }
        }

    }
}