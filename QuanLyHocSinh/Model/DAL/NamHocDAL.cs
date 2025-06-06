using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using QuanLyHocSinh.Model.Entities;
using System.Threading.Tasks;

namespace QuanLyHocSinh.Model.Entities
{
        public class NamHocDAL : BaseDAL
    {
        // ID cho NamHoc (NH2023) thường không phải là số thứ tự đơn giản.
        // Nó có thể được nhập thủ công hoặc sinh theo quy tắc (ví dụ: "NH" + năm bắt đầu).
        // Vì vậy, hàm Generate ID có thể không cần thiết nếu ID được cung cấp từ bên ngoài.
        // Nếu cần Generate, ví dụ: "NH" + 6 chữ số tăng dần:
        private static async Task<string> GenerateNewNamHocIDAsync(MySqlConnection conn, MySqlTransaction transaction)
        {
            string prefix = "NH";
            string query = "SELECT NamHocID FROM NAMHOC WHERE NamHocID LIKE @PrefixPattern ORDER BY NamHocID DESC LIMIT 1";
            int nextNumber = 1;

            using (MySqlCommand cmd = new MySqlCommand(query, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@PrefixPattern", prefix + "%");
                var result = await cmd.ExecuteScalarAsync();
                if (result != null && result != DBNull.Value)
                {
                    string lastId = result.ToString();
                    if (lastId.StartsWith(prefix) && lastId.Length > prefix.Length) // NHxxxxxx (total 8)
                    {
                         // Giả sử phần số là 6 chữ số sau 'NH'
                        if (lastId.Length == 8 && int.TryParse(lastId.Substring(prefix.Length), out int num))
                        {
                            nextNumber = num + 1;
                        }
                    }
                }
            }
            return prefix + nextNumber.ToString("D6"); // NH000001
        }


        public async Task<List<NamHoc>> GetAllNamHocAsync()
        {
            List<NamHoc> namHocs = new List<NamHoc>();
            string query = "SELECT NamHocID, MoTa, BatDau, KetThuc FROM NAMHOC";
            using (MySqlConnection conn = GetConnection())
            {
                await conn.OpenAsync();
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        int idOrdinal = reader.GetOrdinal("NamHocID");
                        int moTaOrdinal = reader.GetOrdinal("MoTa");
                        int batDauOrdinal = reader.GetOrdinal("BatDau");
                        int ketThucOrdinal = reader.GetOrdinal("KetThuc");
                        while (await reader.ReadAsync())
                        {
                            namHocs.Add(new NamHoc
                            {
                                NamHocID = reader.GetString(idOrdinal),
                                MoTa = reader.GetString(moTaOrdinal),
                                BatDau = reader.GetDateTime(batDauOrdinal),
                                KetThuc = reader.GetDateTime(ketThucOrdinal)
                            });
                        }
                    }
                }
            }
            return namHocs;
        }

        public async Task<NamHoc> GetNamHocByIdAsync(string namHocID)
        {
            NamHoc namHoc = null;
            string query = "SELECT NamHocID, MoTa, BatDau, KetThuc FROM NAMHOC WHERE NamHocID = @NamHocID";
            using (MySqlConnection conn = GetConnection())
            {
                await conn.OpenAsync();
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@NamHocID", namHocID);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            int idOrdinal = reader.GetOrdinal("NamHocID");
                            int moTaOrdinal = reader.GetOrdinal("MoTa");
                            int batDauOrdinal = reader.GetOrdinal("BatDau");
                            int ketThucOrdinal = reader.GetOrdinal("KetThuc");
                            namHoc = new NamHoc
                            {
                                NamHocID = reader.GetString(idOrdinal),
                                MoTa = reader.GetString(moTaOrdinal),
                                BatDau = reader.GetDateTime(batDauOrdinal),
                                KetThuc = reader.GetDateTime(ketThucOrdinal)
                            };
                        }
                    }
                }
            }
            return namHoc;
        }

        public async Task AddNamHocAsync(NamHoc namHoc, bool autoGenerateID = false)
        {
            using (MySqlConnection conn = GetConnection())
            {
                await conn.OpenAsync();
                using (MySqlTransaction transaction = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        if (autoGenerateID || string.IsNullOrEmpty(namHoc.NamHocID))
                        {
                            namHoc.NamHocID = await GenerateNewNamHocIDAsync(conn, transaction);
                        }
                        string query = "INSERT INTO NAMHOC (NamHocID, MoTa, BatDau, KetThuc) VALUES (@NamHocID, @MoTa, @BatDau, @KetThuc)";
                        using (MySqlCommand cmd = new MySqlCommand(query, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@NamHocID", namHoc.NamHocID);
                            cmd.Parameters.AddWithValue("@MoTa", namHoc.MoTa);
                            cmd.Parameters.AddWithValue("@BatDau", namHoc.BatDau);
                            cmd.Parameters.AddWithValue("@KetThuc", namHoc.KetThuc);
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

        public async Task UpdateNamHocAsync(NamHoc namHoc)
        {
            using (MySqlConnection conn = GetConnection())
            {
                await conn.OpenAsync();
                using (MySqlTransaction transaction = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        string query = "UPDATE NAMHOC SET MoTa = @MoTa, BatDau = @BatDau, KetThuc = @KetThuc WHERE NamHocID = @NamHocID";
                        using (MySqlCommand cmd = new MySqlCommand(query, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@MoTa", namHoc.MoTa);
                            cmd.Parameters.AddWithValue("@BatDau", namHoc.BatDau);
                            cmd.Parameters.AddWithValue("@KetThuc", namHoc.KetThuc);
                            cmd.Parameters.AddWithValue("@NamHocID", namHoc.NamHocID);
                            int rowsAffected = await cmd.ExecuteNonQueryAsync();
                            if (rowsAffected == 0) throw new Exception("Năm học không tìm thấy hoặc không có gì thay đổi.");
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

        public async Task DeleteNamHocAsync(string namHocID)
        {
            using (MySqlConnection conn = GetConnection())
            {
                await conn.OpenAsync();
                using (MySqlTransaction transaction = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        // Cân nhắc: Kiểm tra DIEM bảng tham chiếu trước khi xóa.
                        string query = "DELETE FROM NAMHOC WHERE NamHocID = @NamHocID";
                        using (MySqlCommand cmd = new MySqlCommand(query, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@NamHocID", namHocID);
                            int rowsAffected = await cmd.ExecuteNonQueryAsync();
                             if (rowsAffected == 0) throw new Exception("Năm học không tìm thấy.");
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