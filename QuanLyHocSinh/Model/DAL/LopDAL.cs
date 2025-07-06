using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System.Threading.Tasks;
using System.Linq;
using System.Configuration;
// Đảm bảo using QuanLyHocSinh.Model.Entities; nếu LopDAL ở namespace khác entities

namespace QuanLyHocSinh.Model.Entities
{
    public class LopDAL : BaseDAL
    {
        public LopDAL() : base() { }
        // LopDAL.cs
        public static string? LayTenLopTheoID(string lopID)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT TenLop FROM LOP WHERE LopID = @LopID";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@LopID", lopID);
                    var result = cmd.ExecuteScalar();
                    return result?.ToString();
                }
            }
        }

        public static int GetSiSo(string lopID)
        {
            int siSo = 0;

            string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                string query = "SELECT SiSo FROM LOP WHERE LopID = @LopID";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@LopID", lopID);

                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        siSo = Convert.ToInt32(result);
                    }
                }
            }

            return siSo;
        }


        // --- 1. Xuất toàn bộ Lớp ---
        public async Task<List<Lop>> GetAllLopAsync()
        {
            List<Lop> lops = new List<Lop>();
            string query = @"
                SELECT l.LopID, l.TenLop, l.SiSo, l.GVCNID AS MaGV_GVCN,
                       hoso.HoTen AS TenGVCN
                FROM LOP l
                LEFT JOIN GIAOVIEN gv ON l.GVCNID = gv.GiaoVienID
                LEFT JOIN HOSOGIAOVIEN hsgv ON gv.GiaoVienID = hsgv.GiaoVienID
                LEFT JOIN HOSO hoso ON hsgv.HoSoID = hoso.HoSoID;
            ";

            using (MySqlConnection conn = GetConnection())
            {
                try
                {
                    await conn.OpenAsync();
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            // Lấy chỉ số cột một lần
                            int lopIDOrdinal = reader.GetOrdinal("LopID");
                            int tenLopOrdinal = reader.GetOrdinal("TenLop");
                            int siSoOrdinal = reader.GetOrdinal("SiSo");
                            int maGVCNOrdinal = reader.GetOrdinal("MaGV_GVCN");
                            int tenGVCNOrdinal = reader.GetOrdinal("TenGVCN");

                            while (await reader.ReadAsync())
                            {
                                Lop lop = new Lop
                                {
                                    LopID = reader.GetString(lopIDOrdinal),
                                    TenLop = reader.GetString(tenLopOrdinal),
                                    SiSo = reader.GetInt32(siSoOrdinal),
                                    GVCNID = reader.IsDBNull(maGVCNOrdinal) ? null : reader.GetString(maGVCNOrdinal),
                                    GVCN = reader.IsDBNull(maGVCNOrdinal) ? null : new GiaoVien
                                    {
                                        MaGV = reader.GetString(maGVCNOrdinal),
                                        HoTen = reader.IsDBNull(tenGVCNOrdinal) ? null : reader.GetString(tenGVCNOrdinal)
                                    }
                                };
                                lops.Add(lop);
                            }
                        }
                    }
                }
                catch (MySqlException ex) { throw new Exception($"Lỗi MySQL khi lấy tất cả lớp: {ex.Message}", ex); }
                catch (Exception ex) { throw new Exception($"Lỗi chung khi lấy tất cả lớp: {ex.Message}", ex); }
            }
            return lops;
        }

        // --- 2. Xuất Học sinh thuộc Lớp kèm Điểm các môn ---
        public async Task<List<HocSinh>> GetHocSinhsWithDiemByLopIDAsync(string lopID, string namHocID = null, int? hocKy = null)
        {
            if (string.IsNullOrWhiteSpace(lopID)) throw new ArgumentException("LopID không được trống.");

            List<HocSinh> hocSinhs = new List<HocSinh>();
            string query = @"
                SELECT
                    hs.HocSinhID, hs.UserID, hshs.HoSoID, 
                    hoso.HoTen, hoso.GioiTinh, hoso.NgaySinh, hoso.Email, hoso.DiaChi, hoso.TrangThaiHoSo,
                    lop_info.TenLop, hshs.NienKhoa,

                    d.DiemID, d.NamHocID AS DiemNamHocID, d.HocKy AS DiemHocKy,
                    d.DiemTrungBinh, d.XepLoai,
                    mh.MonHocID, mh.TenMonHoc,
                    ctd.ChiTietDiemID, ctd.LoaiDiemID, ctd.GiaTri AS DiemGiaTri
                    -- ld.TenLoaiDiem, ld.HeSo (Không sử dụng trực tiếp trong mapping C# hiện tại)
                FROM HOCSINH hs
                JOIN HOSOHOCSINH hshs ON hs.HocSinhID = hshs.HocSinhID
                JOIN HOSO hoso ON hshs.HoSoID = hoso.HoSoID
                JOIN LOP lop_info ON hshs.LopHocID = lop_info.LopID
                LEFT JOIN DIEM d ON hs.HocSinhID = d.HocSinhID
                    AND (@NamHocID IS NULL OR d.NamHocID = @NamHocID)
                    AND (@HocKy IS NULL OR d.HocKy = @HocKy)
                LEFT JOIN MONHOC mh ON d.MonHocID = mh.MonHocID
                LEFT JOIN CHITIETDIEM ctd ON d.DiemID = ctd.DiemID
                LEFT JOIN LOAIDIEM ld ON ctd.LoaiDiemID = ld.LoaiDiemID
                WHERE hshs.LopHocID = @LopID
                ORDER BY hs.HocSinhID, d.DiemID, ctd.LoaiDiemID;
            ";

            using (MySqlConnection conn = GetConnection())
            {
                try
                {
                    await conn.OpenAsync();
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@LopID", lopID);
                        cmd.Parameters.AddWithValue("@NamHocID", (object)namHocID ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@HocKy", hocKy.HasValue ? (object)hocKy.Value : DBNull.Value);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            // Lấy chỉ số cột một lần
                            int hsHocSinhIDOrdinal = reader.GetOrdinal("HocSinhID");
                            int hsUserIDOrdinal = reader.GetOrdinal("UserID");
                            int hsHoSoIDOrdinal = reader.GetOrdinal("HoSoID");
                            int hoTenOrdinal = reader.GetOrdinal("HoTen");
                            int gioiTinhOrdinal = reader.GetOrdinal("GioiTinh");
                            int ngaySinhOrdinal = reader.GetOrdinal("NgaySinh");
                            int emailOrdinal = reader.GetOrdinal("Email");
                            int diaChiOrdinal = reader.GetOrdinal("DiaChi");
                            int trangThaiHoSoOrdinal = reader.GetOrdinal("TrangThaiHoSo");
                            int lopTenLopOrdinal = reader.GetOrdinal("TenLop");
                            int nienKhoaOrdinal = reader.GetOrdinal("NienKhoa");

                            int dDiemIDOrdinal = reader.GetOrdinal("DiemID");
                            int dNamHocIDOrdinal = reader.GetOrdinal("DiemNamHocID");
                            int dHocKyOrdinal = reader.GetOrdinal("DiemHocKy");
                            int dDiemTrungBinhOrdinal = reader.GetOrdinal("DiemTrungBinh");
                            int dXepLoaiOrdinal = reader.GetOrdinal("XepLoai");

                            int mhMonHocIDOrdinal = reader.GetOrdinal("MonHocID");
                            int mhTenMonHocOrdinal = reader.GetOrdinal("TenMonHoc");

                            int ctdChiTietDiemIDOrdinal = reader.GetOrdinal("ChiTietDiemID");
                            int ctdLoaiDiemIDOrdinal = reader.GetOrdinal("LoaiDiemID");
                            int ctdDiemGiaTriOrdinal = reader.GetOrdinal("DiemGiaTri");

                            var hocSinhsDict = new Dictionary<string, HocSinh>();
                            HocSinh currentHocSinh = null;
                            Diem diemMonHoc = null;

                            while (await reader.ReadAsync())
                            {
                                string currentHocSinhID_DB = reader.GetString(hsHocSinhIDOrdinal);

                                if (currentHocSinh == null || currentHocSinh.HocSinhID != currentHocSinhID_DB)
                                {
                                    if (!hocSinhsDict.TryGetValue(currentHocSinhID_DB, out currentHocSinh))
                                    {
                                        currentHocSinh = new HocSinh
                                        {
                                            HocSinhID = currentHocSinhID_DB,
                                            UserID = reader.GetString(hsUserIDOrdinal),
                                            HoSoID = reader.GetString(hsHoSoIDOrdinal),
                                            HoTen = reader.GetString(hoTenOrdinal),
                                            GioiTinh = reader.GetString(gioiTinhOrdinal),
                                            NgaySinh = reader.GetDateTime(ngaySinhOrdinal),
                                            Email = reader.GetString(emailOrdinal),
                                            DiaChi = reader.GetString(diaChiOrdinal),
                                            TenLop = reader.GetString(lopTenLopOrdinal),
                                            NienKhoa = reader.GetInt32(nienKhoaOrdinal),
                                            TrangThaiHoSo = reader.GetString(trangThaiHoSoOrdinal),
                                            Diems = new List<Diem>()
                                        };
                                        hocSinhsDict.Add(currentHocSinhID_DB, currentHocSinh);
                                        hocSinhs.Add(currentHocSinh);
                                    }
                                    diemMonHoc = null; 
                                }

                                if (!reader.IsDBNull(dDiemIDOrdinal))
                                {
                                    string diemID_DB = reader.GetString(dDiemIDOrdinal);
                                    if (diemMonHoc == null || diemMonHoc.DiemID != diemID_DB)
                                    {
                                        diemMonHoc = currentHocSinh.Diems.FirstOrDefault(d => d.DiemID == diemID_DB);
                                        if (diemMonHoc == null)
                                        {
                                            diemMonHoc = new Diem
                                            {
                                                DiemID = diemID_DB,
                                                MaHS = currentHocSinhID_DB,
                                                HoTen = currentHocSinh.HoTen, 
                                                Lop = currentHocSinh.TenLop, 
                                                MonHocID = reader.GetString(mhMonHocIDOrdinal),
                                                MonHoc = reader.GetString(mhTenMonHocOrdinal),
                                                NamHocID = reader.GetString(dNamHocIDOrdinal),
                                                HocKy = reader.GetInt32(dHocKyOrdinal),
                                                DiemTB = reader.IsDBNull(dDiemTrungBinhOrdinal) ? (float?)null : reader.GetFloat(dDiemTrungBinhOrdinal),
                                                XepLoai = reader.IsDBNull(dXepLoaiOrdinal) ? null : reader.GetString(dXepLoaiOrdinal),
                                            };
                                            currentHocSinh.Diems.Add(diemMonHoc);
                                        }
                                    }

                                    if (!reader.IsDBNull(ctdChiTietDiemIDOrdinal) && diemMonHoc != null)
                                    {
                                        string loaiDiemID_CT = reader.GetString(ctdLoaiDiemIDOrdinal);
                                        float giaTri_CT = reader.GetFloat(ctdDiemGiaTriOrdinal);

                                        switch (loaiDiemID_CT)
                                        {
                                            case "LD001": 
                                                diemMonHoc.DiemMieng = giaTri_CT;
                                                break;
                                            case "LD002": 
                                                diemMonHoc.Diem15p = giaTri_CT;
                                                break;
                                            case "LD003": 
                                                diemMonHoc.Diem1Tiet = giaTri_CT;
                                                break;
                                            case "LD004": 
                                                diemMonHoc.DiemThi = giaTri_CT;
                                                break;
                                        }
                                    }
                                }
                                else 
                                {
                                    diemMonHoc = null; 
                                }
                            }
                        }
                    }
                }
                catch (MySqlException ex) { throw new Exception($"Lỗi MySQL khi lấy học sinh và điểm theo lớp: {ex.Message}", ex); }
                catch (Exception ex) { throw new Exception($"Lỗi chung khi lấy học sinh và điểm theo lớp: {ex.Message}", ex); }
            }
            return hocSinhs;
        }

        // --- 3. Thêm Học sinh vào Lớp ---
        public async Task AddHocSinhToLopAsync(string hocSinhID, string lopID, int nienKhoa)
        {
            if (string.IsNullOrWhiteSpace(hocSinhID) || string.IsNullOrWhiteSpace(lopID) || nienKhoa <= 0)
            {
                throw new ArgumentException("HocSinhID, LopID không được trống và Niên khóa phải hợp lệ.");
            }

            using (MySqlConnection conn = GetConnection())
            {
                await conn.OpenAsync();
                using (MySqlTransaction transaction = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        string checkQuery = "SELECT COUNT(*) FROM HOSOHOCSINH WHERE HocSinhID = @HocSinhID AND LopHocID = @LopHocID AND NienKhoa = @NienKhoa";
                        using (MySqlCommand checkCmd = new MySqlCommand(checkQuery, conn, transaction))
                        {
                            checkCmd.Parameters.AddWithValue("@HocSinhID", hocSinhID);
                            checkCmd.Parameters.AddWithValue("@LopHocID", lopID);
                            checkCmd.Parameters.AddWithValue("@NienKhoa", nienKhoa);
                            long count = Convert.ToInt64(await checkCmd.ExecuteScalarAsync());
                            if (count > 0)
                            {
                                throw new InvalidOperationException($"Học sinh '{hocSinhID}' đã tồn tại trong lớp '{lopID}' cho niên khóa '{nienKhoa}'.");
                            }
                        }
                        
                        string hoSoID = await LopDALHelper.GetHoSoIDByHocSinhIDAsync(hocSinhID, conn, transaction);
                        if (string.IsNullOrWhiteSpace(hoSoID))
                        {
                            // Cân nhắc: Nếu không tìm thấy HoSoID, có thể là học sinh mới hoàn toàn.
                            // Lúc này, quy trình có thể cần tạo HOSO trước, rồi mới lấy HoSoID đó.
                            // Hiện tại, logic này sẽ báo lỗi nếu không tìm thấy HoSoID từ HOSOHOCSINH (cho thấy HS chưa từng có hồ sơ nào).
                            throw new InvalidOperationException($"Không tìm thấy Hồ sơ (HoSoID) cho Học sinh ID '{hocSinhID}'. Không thể thêm vào HOSOHOCSINH. Hãy đảm bảo học sinh đã có hồ sơ cơ bản.");
                        }

                        string newHoSoHocSinhID = await LopDALHelper.GenerateNewHoSoHocSinhIDAsync(conn, transaction);
                        string query = @"
                            INSERT INTO HOSOHOCSINH (HoSoHocSinhID, HocSinhID, LopHocID, NienKhoa, HoSoID)
                            VALUES (@HoSoHocSinhID, @HocSinhID, @LopHocID, @NienKhoa, @HoSoID)";

                        using (MySqlCommand cmd = new MySqlCommand(query, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@HoSoHocSinhID", newHoSoHocSinhID);
                            cmd.Parameters.AddWithValue("@HocSinhID", hocSinhID);
                            cmd.Parameters.AddWithValue("@LopHocID", lopID);
                            cmd.Parameters.AddWithValue("@NienKhoa", nienKhoa);
                            cmd.Parameters.AddWithValue("@HoSoID", hoSoID);
                            await cmd.ExecuteNonQueryAsync();
                        }

                        await LopDALHelper.UpdateSiSoLopAsync(lopID, 1, conn, transaction);

                        await transaction.CommitAsync();
                    }
                    catch (MySqlException ex) { await transaction.RollbackAsync(); throw new Exception($"Lỗi MySQL khi thêm học sinh vào lớp: {ex.Message}", ex); }
                    catch (Exception ex) { await transaction.RollbackAsync(); throw new Exception($"Lỗi chung khi thêm học sinh vào lớp: {ex.Message}", ex); }
                }
            }
        }

        // --- 4. Xóa Học sinh khỏi Lớp ---
        public async Task RemoveHocSinhFromLopAsync(string hocSinhID, string lopID, int? nienKhoa = null)
        {
            if (string.IsNullOrWhiteSpace(hocSinhID) || string.IsNullOrWhiteSpace(lopID))
            {
                throw new ArgumentException("HocSinhID và LopID không được trống.");
            }

            using (MySqlConnection conn = GetConnection())
            {
                await conn.OpenAsync();
                using (MySqlTransaction transaction = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        string query = "DELETE FROM HOSOHOCSINH WHERE HocSinhID = @HocSinhID AND LopHocID = @LopHocID";
                        if (nienKhoa.HasValue)
                        {
                            query += " AND NienKhoa = @NienKhoa";
                        }

                        using (MySqlCommand cmd = new MySqlCommand(query, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@HocSinhID", hocSinhID);
                            cmd.Parameters.AddWithValue("@LopHocID", lopID);
                            if (nienKhoa.HasValue)
                            {
                                cmd.Parameters.AddWithValue("@NienKhoa", nienKhoa.Value);
                            }
                            int rowsAffected = await cmd.ExecuteNonQueryAsync();
                            if (rowsAffected == 0)
                            {
                                string nienKhoaMsg = nienKhoa.HasValue ? $" cho niên khóa {nienKhoa.Value}" : "";
                                throw new InvalidOperationException($"Học sinh '{hocSinhID}' không tồn tại trong lớp '{lopID}'{nienKhoaMsg}, hoặc đã được xóa.");
                            }
                        }

                        await LopDALHelper.UpdateSiSoLopAsync(lopID, -1, conn, transaction);
                        await transaction.CommitAsync();
                    }
                    catch (MySqlException ex) { await transaction.RollbackAsync(); throw new Exception($"Lỗi MySQL khi xóa học sinh khỏi lớp: {ex.Message}", ex); }
                    catch (Exception ex) { await transaction.RollbackAsync(); throw new Exception($"Lỗi chung khi xóa học sinh khỏi lớp: {ex.Message}", ex); }
                }
            }
        }

        // --- 5. Quản lý Môn học của Lớp ---
        public async Task<List<MonHoc>> GetMonHocByLopIDAsync(string lopID)
        {
            if (string.IsNullOrWhiteSpace(lopID)) throw new ArgumentException("LopID không được trống.");

            List<MonHoc> monHocs = new List<MonHoc>();
            string query = @"
                SELECT DISTINCT mh.MonHocID, mh.TenMonHoc
                FROM MONHOC mh
                JOIN CHITIETMONHOC ctmh ON mh.MonHocID = ctmh.MonHocID
                WHERE ctmh.LopDayID = @LopID;";

            using (MySqlConnection conn = GetConnection())
            {
                try
                {
                    await conn.OpenAsync();
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@LopID", lopID);
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            int monHocIDOrdinal = reader.GetOrdinal("MonHocID");
                            int tenMonHocOrdinal = reader.GetOrdinal("TenMonHoc");

                            while (await reader.ReadAsync())
                            {
                                MonHoc mh = new MonHoc
                                {
                                    MonHocID = reader.GetString(monHocIDOrdinal),
                                    TenMonHoc = reader.GetString(tenMonHocOrdinal)
                                };
                                monHocs.Add(mh);
                            }
                        }
                    }
                }
                catch (MySqlException ex) { throw new Exception($"Lỗi MySQL khi lấy môn học theo lớp: {ex.Message}", ex); }
                catch (Exception ex) { throw new Exception($"Lỗi chung khi lấy môn học theo lớp: {ex.Message}", ex); }
            }
            return monHocs;
        }

        // Thêm phân công giảng dạy (ChiTietMonHoc)
        public async Task AddMonHocToLopAsync(string giaoVienID, string monHocID, string lopID, DateTime ngayDay, string noiDungDay = null)
        {
            if (string.IsNullOrWhiteSpace(giaoVienID) || string.IsNullOrWhiteSpace(monHocID) || string.IsNullOrWhiteSpace(lopID))
            {
                throw new ArgumentException("GiaoVienID, MonHocID, LopID không được trống.");
            }

            using (MySqlConnection conn = GetConnection())
            {
                await conn.OpenAsync();
                using (MySqlTransaction transaction = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        string newChiTietMonHocID = await LopDALHelper.GenerateNewChiTietMonHocIDAsync(conn, transaction);

                        string query = @"
                            INSERT INTO CHITIETMONHOC (ChiTietMonHocID, GiaoVienID, MonHocID, LopDayID, NgayDay, NoiDungDay)
                            VALUES (@ChiTietMonHocID, @GiaoVienID, @MonHocID, @LopDayID, @NgayDay, @NoiDungDay)";

                        using (MySqlCommand cmd = new MySqlCommand(query, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@ChiTietMonHocID", newChiTietMonHocID);
                            cmd.Parameters.AddWithValue("@GiaoVienID", giaoVienID);
                            cmd.Parameters.AddWithValue("@MonHocID", monHocID);
                            cmd.Parameters.AddWithValue("@LopDayID", lopID);
                            cmd.Parameters.AddWithValue("@NgayDay", ngayDay);
                            cmd.Parameters.AddWithValue("@NoiDungDay", (object)noiDungDay ?? DBNull.Value);
                            await cmd.ExecuteNonQueryAsync();
                        }
                        await transaction.CommitAsync();
                    }
                    catch (MySqlException ex) { await transaction.RollbackAsync(); throw new Exception($"Lỗi MySQL khi thêm môn học vào lớp: {ex.Message}", ex); }
                    catch (Exception ex) { await transaction.RollbackAsync(); throw new Exception($"Lỗi chung khi thêm môn học vào lớp: {ex.Message}", ex); }
                }
            }
        }
        public static void UpdateDanhSachLop(List<Lop> danhSachLop)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                // Bắt đầu transaction để đảm bảo tất cả cập nhật đều thành công
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string query = "UPDATE LOP SET TenLop = @TenLop WHERE LopID = @LopID";
                        
                        foreach (var lop in danhSachLop)
                        {
                            using (var cmd = new MySqlCommand(query, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@TenLop", lop.TenLop);
                                cmd.Parameters.AddWithValue("@LopID", lop.LopID);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        
                        // Lưu tất cả thay đổi nếu không có lỗi
                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        // Hủy bỏ tất cả thay đổi nếu có lỗi
                        transaction.Rollback();
                        throw; // Ném lại exception để ViewModel xử lý
                    }
                }
            }
        }
        // Xóa phân công giảng dạy (ChiTietMonHoc)
        public async Task RemoveMonHocAssignmentFromLopAsync(string chiTietMonHocID)
        {
            if (string.IsNullOrWhiteSpace(chiTietMonHocID))
            {
                throw new ArgumentException("ChiTietMonHocID không được trống.");
            }

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
                            if (rowsAffected == 0)
                            {
                                throw new InvalidOperationException($"Không tìm thấy bản ghi phân công môn học với ID: '{chiTietMonHocID}'.");
                            }
                        }
                        await transaction.CommitAsync();
                    }
                    catch (MySqlException ex) { await transaction.RollbackAsync(); throw new Exception($"Lỗi MySQL khi xóa phân công môn học khỏi lớp: {ex.Message}", ex); }
                    catch (Exception ex) { await transaction.RollbackAsync(); throw new Exception($"Lỗi chung khi xóa phân công môn học khỏi lớp: {ex.Message}", ex); }
                }
            }
        }

        // --- Helper Class (Dành cho các hàm Helper của LopDAL) ---
        public static class LopDALHelper
        {
            public static async Task UpdateSiSoLopAsync(string lopID, int change, MySqlConnection conn, MySqlTransaction transaction)
            {
                string query = "UPDATE LOP SET SiSo = GREATEST(0, SiSo + @Change) WHERE LopID = @LopID";
                using (MySqlCommand cmd = new MySqlCommand(query, conn, transaction))
                {
                    cmd.Parameters.AddWithValue("@Change", change);
                    cmd.Parameters.AddWithValue("@LopID", lopID);
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            public static async Task<string> GenerateNewHoSoHocSinhIDAsync(MySqlConnection conn, MySqlTransaction transaction)
            {
                string prefix = "HSHS"; 
                string query = "SELECT HoSoHocSinhID FROM HOSOHOCSINH ORDER BY LENGTH(HoSoHocSinhID) DESC, HoSoHocSinhID DESC LIMIT 1";
                using (MySqlCommand cmd = new MySqlCommand(query, conn, transaction))
                {
                    var result = await cmd.ExecuteScalarAsync();
                    if (result != null && result != DBNull.Value)
                    {
                        string lastId = result.ToString();
                        if (lastId.StartsWith(prefix) && lastId.Length > prefix.Length)
                        {
                            string numericPart = lastId.Substring(prefix.Length);
                            if (int.TryParse(numericPart, out int num))
                            {
                                num++;
                                return prefix + num.ToString("D4"); 
                            }
                        }
                    }
                    return prefix + "0001"; 
                }
            }

            public static async Task<string> GenerateNewChiTietMonHocIDAsync(MySqlConnection conn, MySqlTransaction transaction)
            {
                string prefix = "CTMH"; 
                string query = "SELECT ChiTietMonHocID FROM CHITIETMONHOC ORDER BY LENGTH(ChiTietMonHocID) DESC, ChiTietMonHocID DESC LIMIT 1";
                using (MySqlCommand cmd = new MySqlCommand(query, conn, transaction))
                {
                    var result = await cmd.ExecuteScalarAsync();
                    if (result != null && result != DBNull.Value)
                    {
                        string lastId = result.ToString();
                        if (lastId.StartsWith(prefix) && lastId.Length > prefix.Length)
                        {
                             string numericPart = lastId.Substring(prefix.Length);
                            if (int.TryParse(numericPart, out int num))
                            {
                                num++;
                                return prefix + num.ToString("D4"); 
                            }
                        }
                    }
                    return prefix + "0001"; 
                }
            }
            
            public static async Task<string> GetHoSoIDByHocSinhIDAsync(string hocSinhID, MySqlConnection conn, MySqlTransaction transaction)
            {
                // Truy vấn để lấy HoSoID từ bảng HOSO, giả sử có một cách nào đó để liên kết HocSinhID với HoSoID gốc.
                // Trong schema hiện tại, HOSOHOCSINH là nơi chứa HoSoID của học sinh cho một lớp/niên khóa cụ thể.
                // Nếu học sinh là mới và chưa có trong HOSOHOCSINH, nhưng đã có HOSO được tạo, bạn cần
                // một cách khác để lấy HoSoID đó (ví dụ: từ một bảng mapping HocSinh-HoSo hoặc một quy ước đặt ID).

                // Cách tiếp cận 1: Nếu HOSO được tạo trước và HocSinhDAL có phương thức lấy HoSoID từ HocSinhID (thông qua USERS -> HOCSINH -> ... -> HOSO)
                // (Giả sử có một trường HoSoID trực tiếp trên HocSinh entity hoặc 1 cách join phức tạp hơn)

                // Cách tiếp cận 2 (đơn giản hóa, dựa trên HOSOHOCSINH):
                // Lấy HoSoID từ một bản ghi HOSOHOCSINH đã có của học sinh này.
                // Điều này ngầm định rằng HoSoID của một học sinh là nhất quán.
                string query = "SELECT HoSoID FROM HOSOHOCSINH WHERE HocSinhID = @HocSinhID ORDER BY NienKhoa DESC LIMIT 1";
                // Lấy HoSoID từ niên khóa gần nhất nếu có nhiều bản ghi.

                // Nếu bạn có một bảng HOSO chung và HocSinhID chỉ là một UserID,
                // thì bạn cần JOIN từ USERS -> HOCSINH -> HOSOHOCSINH -> HOSO.
                // Tuy nhiên, mục đích ở đây là lấy HoSoID để insert vào HOSOHOCSINH.

                // Logic này cần được xem xét kỹ dựa trên quy trình tạo Học Sinh và Hồ Sơ của bạn.
                // Ví dụ: Có thể khi tạo User -> Học Sinh, bạn cũng tạo một HOSO và lưu HoSoID đó ở đâu đó.
                // Hoặc bạn có một bảng liên kết HOCSINH.HocSinhID với HOSO.HoSoID.
                // Với schema hiện tại, `HOSOHOCSINH` là nơi chứa thông tin `HoSoID` của một học sinh trong một lớp.
                // Nếu học sinh chưa từng vào lớp nào, sẽ không có HoSoID theo cách này.
                // => Đây là một điểm cần làm rõ trong thiết kế tổng thể của bạn.

                // Giả sử bạn có một cách lấy HoSoID GỐC của học sinh, không phụ thuộc vào việc đã ở lớp nào chưa:
                // Ví dụ, nếu bảng HOCSINH có cột HoSoID_Goc:
                // string query = "SELECT HoSoID_Goc FROM HOCSINH WHERE HocSinhID = @HocSinhID";
                // Hoặc nếu có một bảng riêng map HocSinhID với HoSoID chính:
                // string query = "SELECT HoSoID FROM MAP_HS_HOSO WHERE HocSinhID = @HocSinhID";

                // Hiện tại, giữ nguyên query cũ nhưng thêm ORDER BY để có tính nhất quán hơn nếu có nhiều bản ghi.
                using (MySqlCommand cmd = new MySqlCommand(query, conn, transaction))
                {
                    cmd.Parameters.AddWithValue("@HocSinhID", hocSinhID);
                    var result = await cmd.ExecuteScalarAsync();
                    return result?.ToString();
                }
            }
        }

        public async Task<string> GetLopIdByNameAsync(string tenLop)
        {
            string lopID = null;
            string query = "SELECT LopID FROM LOP WHERE TenLop = @TenLop";
            using (MySqlConnection conn = GetConnection())
            {
                await conn.OpenAsync();
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@TenLop", tenLop);
                    var result = await cmd.ExecuteScalarAsync();
                    if (result != null && result != DBNull.Value)
                    {
                        lopID = result.ToString();
                    }
                }
            }
            return lopID;
        }
    }
}