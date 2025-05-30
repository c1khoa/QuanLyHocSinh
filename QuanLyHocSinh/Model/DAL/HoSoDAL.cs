using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System.Threading.Tasks;

namespace QuanLyHocSinh.Model.Entities
{
    public class HoSoDAL : BaseDAL
    {
        public async Task<List<HoSo>> GetAllHoSoAsync()
        {
            List<HoSo> hoSos = new List<HoSo>();
            string query = @"
                SELECT hs.HoSoID, hs.HoTen, hs.GioiTinh, hs.NgaySinh, hs.Email, hs.DiaChi, hs.ChucVuID,
                       hs.TrangThaiHoSo, hs.NgayTao, hs.NgayCapNhatGanNhat,
                       cv.TenChucVu, cv.MoTa AS ChucVuMoTa, cv.VaiTroID AS ChucVuVaiTroID,
                       vt.TenVaiTro
                FROM HOSO hs
                JOIN CHUCVU cv ON hs.ChucVuID = cv.ChucVuID
                JOIN VAITRO vt ON cv.VaiTroID = vt.VaiTroID";

            using (MySqlConnection conn = GetConnection())
            {
                try
                {
                    await conn.OpenAsync();
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                HoSo hoSo = new HoSo
                                {
                                    HoSoID = reader["HoSoID"].ToString(),
                                    HoTen = reader["HoTen"].ToString(),
                                    GioiTinh = reader["GioiTinh"].ToString(),
                                    NgaySinh = reader["NgaySinh"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["NgaySinh"]),
                                    Email = reader["Email"].ToString(),
                                    DiaChi = reader["DiaChi"].ToString(),
                                    ChucVuID = reader["ChucVuID"].ToString(),
                                    TrangThaiHoSo = reader["TrangThaiHoSo"].ToString(),
                                    NgayTao = reader["NgayTao"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["NgayTao"]),
                                    NgayCapNhatGanNhat = reader["NgayCapNhatGanNhat"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["NgayCapNhatGanNhat"]),
                                    ChucVu = new ChucVu
                                    {
                                        ChucVuID = reader["ChucVuID"].ToString(),
                                        TenChucVu = reader["TenChucVu"].ToString(),
                                        MoTa = reader["ChucVuMoTa"].ToString(),
                                        VaiTroID = reader["ChucVuVaiTroID"].ToString(),
                                        VaiTro = new VaiTro
                                        {
                                            VaiTroID = reader["ChucVuVaiTroID"].ToString(),
                                            TenVaiTro = reader["TenVaiTro"].ToString()
                                        }
                                    }
                                };
                                hoSos.Add(hoSo);
                            }
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    throw new Exception($"Lỗi MySQL khi lấy tất cả hồ sơ: {ex.Message}", ex);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Lỗi chung khi lấy tất cả hồ sơ: {ex.Message}", ex);
                }
            }
            return hoSos;
        }

        // --- 2. Xuất Hồ sơ của một Giáo viên cụ thể ---
        // Lấy thông tin HOSO của một Giáo viên dựa vào GiaoVienID.
        public async Task<HoSo> GetHoSoByGiaoVienIDAsync(string giaoVienID)
        {
            if (string.IsNullOrWhiteSpace(giaoVienID)) return null;

            string query = @"
        SELECT hs.HoSoID, hs.HoTen, hs.GioiTinh, hs.NgaySinh, hs.Email, hs.DiaChi, hs.ChucVuID,
                hs.TrangThaiHoSo, hs.NgayTao, hs.NgayCapNhatGanNhat,
                cv.TenChucVu, cv.MoTa AS ChucVuMoTa, cv.VaiTroID AS ChucVuVaiTroID,
                vt.TenVaiTro
        FROM HOSO hs
        JOIN HOSOGIAOVIEN hsgv ON hs.HoSoID = hsgv.HoSoID -- Liên kết HOSO với HOSOGIAOVIEN
        JOIN GIAOVIEN gv ON hsgv.GiaoVienID = gv.GiaoVienID -- Liên kết HOSOGIAOVIEN với GIAOVIEN
        JOIN CHUCVU cv ON hs.ChucVuID = cv.ChucVuID
        JOIN VAITRO vt ON cv.VaiTroID = vt.VaiTroID
        WHERE gv.GiaoVienID = @GiaoVienID"; // Lọc theo GiaoVienID

            using (MySqlConnection conn = GetConnection())
            {
                try
                {
                    await conn.OpenAsync();
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@GiaoVienID", giaoVienID); // Thêm tham số
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync()) // Chỉ đọc một bản ghi duy nhất
                            {
                                return new HoSo
                                {
                                    HoSoID = reader["HoSoID"].ToString(),
                                    HoTen = reader["HoTen"].ToString(),
                                    GioiTinh = reader["GioiTinh"].ToString(),
                                    NgaySinh = reader["NgaySinh"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["NgaySinh"]),
                                    Email = reader["Email"].ToString(),
                                    DiaChi = reader["DiaChi"].ToString(),
                                    ChucVuID = reader["ChucVuID"].ToString(),
                                    TrangThaiHoSo = reader["TrangThaiHoSo"].ToString(),
                                    NgayTao = reader["NgayTao"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["NgayTao"]),
                                    NgayCapNhatGanNhat = reader["NgayCapNhatGanNhat"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["NgayCapNhatGanNhat"]),
                                    ChucVu = new ChucVu
                                    {
                                        ChucVuID = reader["ChucVuID"].ToString(),
                                        TenChucVu = reader["TenChucVu"].ToString(),
                                        MoTa = reader["ChucVuMoTa"].ToString(),
                                        VaiTroID = reader["ChucVuVaiTroID"].ToString(),
                                        VaiTro = new VaiTro
                                        {
                                            VaiTroID = reader["ChucVuVaiTroID"].ToString(),
                                            TenVaiTro = reader["TenVaiTro"].ToString()
                                        }
                                    }
                                };
                            }
                            return null; // Không tìm thấy hồ sơ cho GiaoVienID này
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    throw new Exception($"Lỗi MySQL khi lấy hồ sơ theo GiaoVienID: {ex.Message}", ex);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Lỗi chung khi lấy hồ sơ theo GiaoVienID: {ex.Message}", ex);
                }
            }
        }
        // --- 3. Xuất Hồ sơ của một Học sinh cụ thể ---
        // Lấy thông tin HOSO của một Học sinh dựa vào HocSinhID.
        public async Task<HoSo> GetHoSoByHocSinhIDAsync(string hocSinhID)
        {
            if (string.IsNullOrWhiteSpace(hocSinhID)) return null;

            string query = @"
            SELECT hs.HoSoID, hs.HoTen, hs.GioiTinh, hs.NgaySinh, hs.Email, hs.DiaChi, hs.ChucVuID,
                hs.TrangThaiHoSo, hs.NgayTao, hs.NgayCapNhatGanNhat,
                cv.TenChucVu, cv.MoTa AS ChucVuMoTa, cv.VaiTroID AS ChucVuVaiTroID,
                vt.TenVaiTro
            FROM HOSO hs
            JOIN HOSOHOCSINH hshs ON hs.HoSoID = hshs.HoSoID -- Liên kết HOSO với HOSOHOCSINH
            JOIN HOCSINH s ON hshs.HocSinhID = s.HocSinhID -- Liên kết HOSOHOCSINH với HOCSINH
            JOIN CHUCVU cv ON hs.ChucVuID = cv.ChucVuID
            JOIN VAITRO vt ON cv.VaiTroID = vt.VaiTroID
            WHERE s.HocSinhID = @HocSinhID"; // Lọc theo HocSinhID

            using (MySqlConnection conn = GetConnection())
            {
                try
                {
                    await conn.OpenAsync();
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@HocSinhID", hocSinhID); // Thêm tham số
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync()) // Chỉ đọc một bản ghi duy nhất
                            {
                                return new HoSo
                                {
                                    HoSoID = reader["HoSoID"].ToString(),
                                    HoTen = reader["HoTen"].ToString(),
                                    GioiTinh = reader["GioiTinh"].ToString(),
                                    NgaySinh = reader["NgaySinh"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["NgaySinh"]),
                                    Email = reader["Email"].ToString(),
                                    DiaChi = reader["DiaChi"].ToString(),
                                    ChucVuID = reader["ChucVuID"].ToString(),
                                    TrangThaiHoSo = reader["TrangThaiHoSo"].ToString(),
                                    NgayTao = reader["NgayTao"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["NgayTao"]),
                                    NgayCapNhatGanNhat = reader["NgayCapNhatGanNhat"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["NgayCapNhatGanNhat"]),
                                    ChucVu = new ChucVu
                                    {
                                        ChucVuID = reader["ChucVuID"].ToString(),
                                        TenChucVu = reader["TenChucVu"].ToString(),
                                        MoTa = reader["ChucVuMoTa"].ToString(),
                                        VaiTroID = reader["ChucVuVaiTroID"].ToString(),
                                        VaiTro = new VaiTro
                                        {
                                            VaiTroID = reader["ChucVuVaiTroID"].ToString(),
                                            TenVaiTro = reader["TenVaiTro"].ToString()
                                        }
                                    }
                                };
                            }
                            return null; // Không tìm thấy hồ sơ cho HocSinhID này
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    throw new Exception($"Lỗi MySQL khi lấy hồ sơ theo HocSinhID: {ex.Message}", ex);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Lỗi chung khi lấy hồ sơ theo HocSinhID: {ex.Message}", ex);
                }
            }
        }
    }
}