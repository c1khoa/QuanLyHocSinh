using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using QuanLyHocSinh.Model.Entities;
using System.Threading.Tasks;

namespace QuanLyHocSinh.Model.Relationships
{
        public class ChiTietDiemDAL : BaseDAL
    {
        private static async Task<string> GenerateNewChiTietDiemIDAsync(MySqlConnection conn, MySqlTransaction transaction)
        {
            string prefix = "CTD"; // Chi Tiet Diem
            // ChiTietDiemID CHAR(8), sample CTD001. "CTD" + 5 digits
            string query = "SELECT ChiTietDiemID FROM CHITIETDIEM WHERE ChiTietDiemID LIKE @PrefixPattern ORDER BY ChiTietDiemID DESC LIMIT 1";
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

        public async Task AddChiTietDiemAsync(ChiTietDiem chiTietDiem)
        {
            using (MySqlConnection conn = GetConnection())
            {
                await conn.OpenAsync();
                using (MySqlTransaction transaction = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        chiTietDiem.ChiTietDiemID = await GenerateNewChiTietDiemIDAsync(conn, transaction);
                        string query = "INSERT INTO CHITIETDIEM (ChiTietDiemID, DiemID, LoaiDiemID, GiaTri) VALUES (@ChiTietDiemID, @DiemID, @LoaiDiemID, @GiaTri)";
                        using (MySqlCommand cmd = new MySqlCommand(query, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@ChiTietDiemID", chiTietDiem.ChiTietDiemID);
                            cmd.Parameters.AddWithValue("@DiemID", chiTietDiem.DiemID);
                            cmd.Parameters.AddWithValue("@LoaiDiemID", chiTietDiem.LoaiDiemID);
                            cmd.Parameters.AddWithValue("@GiaTri", chiTietDiem.GiaTri);
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

        public async Task UpdateChiTietDiemGiaTriAsync(string chiTietDiemID, float newGiaTri)
        {
            using (MySqlConnection conn = GetConnection())
            {
                await conn.OpenAsync();
                using (MySqlTransaction transaction = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        string query = "UPDATE CHITIETDIEM SET GiaTri = @GiaTri WHERE ChiTietDiemID = @ChiTietDiemID";
                        using (MySqlCommand cmd = new MySqlCommand(query, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@GiaTri", newGiaTri);
                            cmd.Parameters.AddWithValue("@ChiTietDiemID", chiTietDiemID);
                            int rowsAffected = await cmd.ExecuteNonQueryAsync();
                            if (rowsAffected == 0) throw new Exception("Chi tiết điểm không tìm thấy hoặc giá trị không đổi.");
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
        
        public async Task DeleteChiTietDiemAsync(string chiTietDiemID)
        {
            using (MySqlConnection conn = GetConnection())
            {
                await conn.OpenAsync();
                using (MySqlTransaction transaction = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        string query = "DELETE FROM CHITIETDIEM WHERE ChiTietDiemID = @ChiTietDiemID";
                        using (MySqlCommand cmd = new MySqlCommand(query, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@ChiTietDiemID", chiTietDiemID);
                            int rowsAffected = await cmd.ExecuteNonQueryAsync();
                            if (rowsAffected == 0) throw new Exception("Chi tiết điểm không tìm thấy.");
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

        public async Task<List<ChiTietDiem>> GetChiTietDiemByDiemIDAsync(string diemID)
        {
            List<ChiTietDiem> chiTietDiems = new List<ChiTietDiem>();
            string query = @"
                SELECT ctd.ChiTietDiemID, ctd.DiemID, ctd.LoaiDiemID, ctd.GiaTri,
                       ld.TenLoaiDiem, ld.HeSo, ld.MoTa AS LoaiDiemMoTa
                FROM CHITIETDIEM ctd
                JOIN LOAIDIEM ld ON ctd.LoaiDiemID = ld.LoaiDiemID
                WHERE ctd.DiemID = @DiemID";
            
            using (MySqlConnection conn = GetConnection())
            {
                await conn.OpenAsync();
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@DiemID", diemID);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        int ctdIDOrdinal = reader.GetOrdinal("ChiTietDiemID");
                        int dIDOrdinal = reader.GetOrdinal("DiemID");
                        int ldIDOrdinal = reader.GetOrdinal("LoaiDiemID");
                        int giaTriOrdinal = reader.GetOrdinal("GiaTri");
                        int tenLoaiDiemOrdinal = reader.GetOrdinal("TenLoaiDiem");
                        int heSoOrdinal = reader.GetOrdinal("HeSo");
                        int loaiDiemMoTaOrdinal = reader.GetOrdinal("LoaiDiemMoTa");

                        while (await reader.ReadAsync())
                        {
                            chiTietDiems.Add(new ChiTietDiem
                            {
                                ChiTietDiemID = reader.GetString(ctdIDOrdinal),
                                DiemID = reader.GetString(dIDOrdinal),
                                LoaiDiemID = reader.GetString(ldIDOrdinal),
                                GiaTri = reader.GetFloat(giaTriOrdinal),
                                LoaiDiem = new LoaiDiem
                                {
                                    LoaiDiemID = reader.GetString(ldIDOrdinal),
                                    TenLoaiDiem = reader.GetString(tenLoaiDiemOrdinal),
                                    HeSo = reader.GetFloat(heSoOrdinal),
                                    MoTa = reader.GetString(loaiDiemMoTaOrdinal)
                                }
                            });
                        }
                    }
                }
            }
            return chiTietDiems;
        }

        // "Xuất toàn bộ điểm của học sinh, môn học, học kỳ, năm học."
        // "Xuất theo Id của từng học sinh"
        // This usually means getting all Diem entries for a student, then for each Diem, getting ChiTietDiem.
        // The DiemDAL and LopDAL already handle parts of this. A more specific query for *all* ChiTietDiem of a student:
        public async Task<List<ChiTietDiem>> GetAllChiTietDiemByHocSinhIDAsync(string hocSinhID, string namHocID = null, int? hocKy = null)
        {
            List<ChiTietDiem> chiTietDiems = new List<ChiTietDiem>();
            string query = @"
                SELECT ctd.ChiTietDiemID, ctd.DiemID, ctd.LoaiDiemID, ctd.GiaTri,
                       ld.TenLoaiDiem, ld.HeSo, ld.MoTa AS LoaiDiemMoTa,
                       d.HocSinhID, d.MonHocID, d.NamHocID, d.HocKy AS DiemHocKy,
                       mh.TenMonHoc
                FROM CHITIETDIEM ctd
                JOIN DIEM d ON ctd.DiemID = d.DiemID
                JOIN LOAIDIEM ld ON ctd.LoaiDiemID = ld.LoaiDiemID
                JOIN MONHOC mh ON d.MonHocID = mh.MonHocID
                WHERE d.HocSinhID = @HocSinhID";

            if (!string.IsNullOrEmpty(namHocID))
                query += " AND d.NamHocID = @NamHocID";
            if (hocKy.HasValue)
                query += " AND d.HocKy = @HocKy";
            
            query += " ORDER BY d.NamHocID, d.HocKy, d.MonHocID, ctd.LoaiDiemID";

            using (MySqlConnection conn = GetConnection())
            {
                await conn.OpenAsync();
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@HocSinhID", hocSinhID);
                    if (!string.IsNullOrEmpty(namHocID))
                        cmd.Parameters.AddWithValue("@NamHocID", namHocID);
                    if (hocKy.HasValue)
                        cmd.Parameters.AddWithValue("@HocKy", hocKy.Value);
                    
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        // Get ordinals
                        int ctdIDOrdinal = reader.GetOrdinal("ChiTietDiemID");
                        int diemIDOrdinal = reader.GetOrdinal("DiemID"); // from ctd
                        int ldIDOrdinal = reader.GetOrdinal("LoaiDiemID");
                        int giaTriOrdinal = reader.GetOrdinal("GiaTri");
                        int tenLoaiDiemOrdinal = reader.GetOrdinal("TenLoaiDiem");
                        int heSoOrdinal = reader.GetOrdinal("HeSo");
                        int loaiDiemMoTaOrdinal = reader.GetOrdinal("LoaiDiemMoTa");
                        // Ordinals for Diem related fields to populate Diem navigation property
                        int dHocSinhIDOrdinal = reader.GetOrdinal("HocSinhID");
                        int dMonHocIDOrdinal = reader.GetOrdinal("MonHocID");
                        int dNamHocIDOrdinal = reader.GetOrdinal("NamHocID");
                        int dHocKyOrdinal = reader.GetOrdinal("DiemHocKy");
                        int mhTenMonHocOrdinal = reader.GetOrdinal("TenMonHoc");


                        while (await reader.ReadAsync())
                        {
                            var chiTietDiem = new ChiTietDiem
                            {
                                ChiTietDiemID = reader.GetString(ctdIDOrdinal),
                                DiemID = reader.GetString(diemIDOrdinal),
                                LoaiDiemID = reader.GetString(ldIDOrdinal),
                                GiaTri = reader.GetFloat(giaTriOrdinal),
                                LoaiDiem = new LoaiDiem
                                {
                                    LoaiDiemID = reader.GetString(ldIDOrdinal),
                                    TenLoaiDiem = reader.GetString(tenLoaiDiemOrdinal),
                                    HeSo = reader.GetFloat(heSoOrdinal),
                                    MoTa = reader.GetString(loaiDiemMoTaOrdinal)
                                },
                                Diem = new Diem // Populate navigation Diem property
                                {
                                     DiemID = reader.GetString(diemIDOrdinal),
                                     MaHS = reader.GetString(dHocSinhIDOrdinal), // Or HocSinhID
                                     MonHocID = reader.GetString(dMonHocIDOrdinal),
                                     NamHocID = reader.GetString(dNamHocIDOrdinal),
                                     HocKy = reader.GetInt32(dHocKyOrdinal),
                                     MonHoc = reader.GetString(mhTenMonHocOrdinal) 
                                     // Other Diem properties like DiemTB, XepLoai, HoTen, Lop are not directly in this query focused on ChiTietDiem
                                     // but can be fetched if needed by expanding the query or making Diem a more detailed object here.
                                }
                            };
                            chiTietDiems.Add(chiTietDiem);
                        }
                    }
                }
            }
            return chiTietDiems;
        }
    }
}