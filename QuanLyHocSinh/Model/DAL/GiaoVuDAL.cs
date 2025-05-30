using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using QuanLyHocSinh.Model.Entities;
using System.Threading.Tasks;

namespace QuanLyHocSinh.Model.Entities
{
    public class GiaoVuDAL : BaseDAL
    {

        // Phương thức lấy tất cả giáo vụ (bao gồm thông tin từ bảng USERS)
        public async Task<List<GiaoVu>> GetAllGiaoVuAsync()
        {
            List<GiaoVu> giaoVus = new List<GiaoVu>();
            string query = @"
                SELECT gv.GiaoVuID, gv.UserID,
                       u.TenDangNhap, u.MatKhau, u.VaiTroID
                FROM GIAOVU gv
                JOIN USERS u ON gv.UserID = u.UserID";

            using (MySqlConnection conn = GetConnection())
            {
                await conn.OpenAsync();
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            GiaoVu gv = new GiaoVu
                            {
                                GiaoVuID = reader["GiaoVuID"].ToString(),
                                UserID = reader["UserID"].ToString(),
                                TenDangNhap = reader["TenDangNhap"].ToString(),
                                MatKhau = reader["MatKhau"].ToString(),
                                VaiTroID = reader["VaiTroID"].ToString(),
                                User = new User // Tạo và gán đối tượng User
                                {
                                UserID = reader["UserID"].ToString(),
                                TenDangNhap = reader["TenDangNhap"].ToString(),
                                MatKhau = reader["MatKhau"].ToString(),
                                 VaiTroID = reader["VaiTroID"].ToString(),
                                }
                            };
                            giaoVus.Add(gv);
                        }
                    }
                }
            }
            return giaoVus;
        }

        // Phương thức thêm giáo vụ
        // Lưu ý: Thêm giáo vụ thường bao gồm thêm một User trước, sau đó mới thêm GiaoVu
        public async Task AddGiaoVuAsync(GiaoVu giaoVu)
        {
            using (MySqlConnection conn = GetConnection())
            {
                await conn.OpenAsync();
                using (MySqlTransaction transaction = await conn.BeginTransactionAsync()) // Bắt đầu transaction
                {
                    try
                    {
                        // 1. Thêm User (nếu UserID chưa tồn tại)
                        // Bạn cần kiểm tra xem UserID đã tồn tại chưa hoặc có một phương thức AddUser trong UserDAL
                        string userQuery = @"INSERT INTO USERS (UserID, TenDangNhap, MatKhau, VaiTroID)
                                             VALUES (@UserID, @TenDangNhap, @MatKhau, @VaiTroID)";
                        using (MySqlCommand userCmd = new MySqlCommand(userQuery, conn, transaction))
                        {
                            userCmd.Parameters.AddWithValue("@UserID", giaoVu.UserID);
                            userCmd.Parameters.AddWithValue("@TenDangNhap", giaoVu.TenDangNhap);
                            userCmd.Parameters.AddWithValue("@MatKhau", giaoVu.MatKhau);
                            userCmd.Parameters.AddWithValue("@VaiTroID", giaoVu.VaiTroID);
                            await userCmd.ExecuteNonQueryAsync();
                        }

                        // 2. Thêm GiaoVu
                        string giaoVuQuery = @"INSERT INTO GIAOVU (GiaoVuID, UserID)
                                              VALUES (@GiaoVuID, @UserID)";
                        using (MySqlCommand giaoVuCmd = new MySqlCommand(giaoVuQuery, conn, transaction))
                        {
                            giaoVuCmd.Parameters.AddWithValue("@GiaoVuID", giaoVu.GiaoVuID);
                            giaoVuCmd.Parameters.AddWithValue("@UserID", giaoVu.UserID);
                            await giaoVuCmd.ExecuteNonQueryAsync();
                        }

                        await transaction.CommitAsync(); // Commit transaction nếu mọi thứ thành công
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync(); // Rollback nếu có lỗi
                        throw new Exception("Lỗi khi thêm giáo vụ: " + ex.Message, ex);
                    }
                }
            }
        }

        // Phương thức cập nhật giáo vụ
        // Cần cập nhật cả thông tin trong bảng USERS và GIAOVU
        public async Task UpdateGiaoVuAsync(GiaoVu giaoVu)
        {
            using (MySqlConnection conn = GetConnection())
            {
                await conn.OpenAsync();
                using (MySqlTransaction transaction = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        // 1. Cập nhật thông tin User
                        string userQuery = @"UPDATE USERS
                                             SET TenDangNhap = @TenDangNhap, MatKhau = @MatKhau, VaiTroID = @VaiTroID
                                             WHERE UserID = @UserID";
                        using (MySqlCommand userCmd = new MySqlCommand(userQuery, conn, transaction))
                        {
                            userCmd.Parameters.AddWithValue("@TenDangNhap", giaoVu.TenDangNhap);
                            userCmd.Parameters.AddWithValue("@MatKhau", giaoVu.MatKhau);
                            userCmd.Parameters.AddWithValue("@VaiTroID", giaoVu.VaiTroID);
                            userCmd.Parameters.AddWithValue("@UserID", giaoVu.UserID);
                            await userCmd.ExecuteNonQueryAsync();
                        }

                        await transaction.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        throw new Exception("Lỗi khi cập nhật giáo vụ: " + ex.Message, ex);
                    }
                }
            }
        }

        // Phương thức xóa giáo vụ
        // Cần xóa cả GiaoVu và User (nếu không có các bảng khác phụ thuộc vào User đó)
        public async Task DeleteGiaoVuAsync(string giaoVuID)
        {
            using (MySqlConnection conn = GetConnection())
            {
                await conn.OpenAsync();
                using (MySqlTransaction transaction = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        // Lấy UserID của giáo vụ cần xóa
                        string getUserIDQuery = "SELECT UserID FROM GIAOVU WHERE GiaoVuID = @GiaoVuID";
                        string userID = null;
                        using (MySqlCommand cmd = new MySqlCommand(getUserIDQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@GiaoVuID", giaoVuID);
                            userID = (await cmd.ExecuteScalarAsync())?.ToString();
                        }

                        if (string.IsNullOrEmpty(userID))
                        {
                            throw new Exception($"Không tìm thấy giáo vụ với GiaoVuID: {giaoVuID}");
                        }

                        // 1. Xóa GiaoVu
                        string deleteGiaoVuQuery = "DELETE FROM GIAOVU WHERE GiaoVuID = @GiaoVuID";
                        using (MySqlCommand cmd = new MySqlCommand(deleteGiaoVuQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@GiaoVuID", giaoVuID);
                            await cmd.ExecuteNonQueryAsync();
                        }

                        // 2. Xóa User
                        string deleteUserQuery = "DELETE FROM USERS WHERE UserID = @UserID";
                        using (MySqlCommand cmd = new MySqlCommand(deleteUserQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@UserID", userID);
                            await cmd.ExecuteNonQueryAsync();
                        }

                        await transaction.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        throw new Exception("Lỗi khi xóa giáo vụ: " + ex.Message, ex);
                    }
                }
            }
        }
    }
}