using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System.Threading.Tasks;

namespace QuanLyHocSinh.Model.Entities
{
    public class UserDAL : BaseDAL
    {

        public async Task<List<User>> GetAllUsersAsync()
        {
            List<User> users = new List<User>();
            string query = @"
                SELECT u.UserID, u.TenDangNhap, u.MatKhau, u.VaiTroID,
                       vt.TenVaiTro
                FROM USERS u
                JOIN VAITRO vt ON u.VaiTroID = vt.VaiTroID";

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
                                User user = new User
                                {
                                    UserID = reader["UserID"].ToString(),
                                    TenDangNhap = reader["TenDangNhap"].ToString(),
                                    MatKhau = reader["MatKhau"].ToString(),
                                    VaiTroID = Convert.ToInt32(reader["VaiTroID"]),
                                    VaiTro = new VaiTro
                                    {
                                        VaiTroID = Convert.ToInt32(reader["VaiTroID"]),
                                        TenVaiTro = reader["TenVaiTro"].ToString(),
                                    }
                                };
                                users.Add(user);
                            }
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    throw new Exception($"Lỗi MySQL khi lấy tất cả người dùng: {ex.Message}", ex);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Lỗi chung khi lấy tất cả người dùng: {ex.Message}", ex);
                }
            }
            return users;
        }

        // --- 2. Thêm Người dùng (Create User) ---
        // UserID trong DB của bạn là CHAR(8) PRIMARY KEY, nên bạn cần tự sinh ID hoặc đảm bảo nó duy nhất.
        public async Task AddUserAsync(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user), "Đối tượng người dùng không được null.");
            }
            // Loại bỏ kiểm tra user.UserID ở đây, vì nó sẽ được sinh tự động
            if (string.IsNullOrWhiteSpace(user.TenDangNhap) ||
                string.IsNullOrWhiteSpace(user.MatKhau) ||
                user.VaiTroID == null)
            {
                throw new ArgumentException("Các trường TenDangNhap, MatKhau, VaiTroID không được để trống.");
            }

            using (MySqlConnection conn = GetConnection())
            {
                await conn.OpenAsync();
                using (MySqlTransaction transaction = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        // 1. Sinh UserID mới ngay trong DAL
                        string newUserID = await GenerateNewUserIDAsync(conn, transaction); // Truyền conn và transaction
                        user.UserID = newUserID; // Gán ID mới vào đối tượng user

                        // 2. Kiểm tra TenDangNhap đã tồn tại chưa (UserID đã được đảm bảo duy nhất qua sinh ID)
                        if (await IsTenDangNhapExistsAsync(user.TenDangNhap, conn, transaction))
                        {
                            throw new InvalidOperationException($"Tên đăng nhập '{user.TenDangNhap}' đã tồn tại.");
                        }

                        string query = @"
                            INSERT INTO USERS (UserID, TenDangNhap, MatKhau, VaiTroID)
                            VALUES (@UserID, @TenDangNhap, @MatKhau, @VaiTroID)";

                        using (MySqlCommand cmd = new MySqlCommand(query, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@UserID", user.UserID); // Dùng ID đã sinh
                            cmd.Parameters.AddWithValue("@TenDangNhap", user.TenDangNhap);
                            cmd.Parameters.AddWithValue("@MatKhau", user.MatKhau);
                            cmd.Parameters.AddWithValue("@VaiTroID", user.VaiTroID);
                            await cmd.ExecuteNonQueryAsync();
                        }
                        await transaction.CommitAsync();
                    }
                    catch (MySqlException ex)
                    {
                        await transaction.RollbackAsync();
                        if (ex.Number == 1062)
                        {
                            throw new Exception("Lỗi trùng lặp dữ liệu: Tên đăng nhập đã tồn tại.", ex); // UserID giờ đã duy nhất
                        }
                        throw new Exception($"Lỗi MySQL khi thêm người dùng: {ex.Message}", ex);
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        throw new Exception($"Lỗi chung khi thêm người dùng: {ex.Message}", ex);
                    }
                }
            }
        }


        public async Task<string> GenerateNewUserIDAsync(MySqlConnection conn = null, MySqlTransaction transaction = null)
        {
            string prefix = "U";
            string query = "SELECT UserID FROM USERS ORDER BY LENGTH(UserID) DESC, UserID DESC LIMIT 1";

            // Nếu không có kết nối được truyền vào, tự tạo và quản lý kết nối
            bool manageConnectionLocally = (conn == null);
            if (manageConnectionLocally)
            {
                conn = GetConnection();
                await conn.OpenAsync();
            }

            try
            {
                using (MySqlCommand cmd = new MySqlCommand(query, conn, transaction)) // Truyền transaction vào command
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
                                return prefix + num.ToString("D6"); // Định dạng U000001
                            }
                        }
                    }
                    return prefix + "000001";
                }
            }
            catch (MySqlException ex)
            {
                throw new Exception($"Lỗi MySQL khi tạo UserID mới: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi chung khi tạo UserID mới: {ex.Message}", ex);
            }
            finally
            {
                if (manageConnectionLocally && conn.State == System.Data.ConnectionState.Open)
                {
                    conn.Close(); // Đóng kết nối nếu nó được tạo và mở cục bộ
                }
            }
        }

// --- 3. Cập nhật một Người dùng (Update User) ---
public async Task UpdateUserAsync(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user), "Đối tượng người dùng không được null.");
            }
            if (string.IsNullOrWhiteSpace(user.UserID))
            {
                throw new ArgumentException("UserID không được trống khi cập nhật.");
            }
            if (string.IsNullOrWhiteSpace(user.TenDangNhap) ||
                string.IsNullOrWhiteSpace(user.MatKhau) || // Mật khẩu có thể không được cập nhật nếu không thay đổi
                user.VaiTroID == null)
            {
                throw new ArgumentException("Các trường TenDangNhap, MatKhau, VaiTroID không được để trống.");
            }

            using (MySqlConnection conn = GetConnection())
            {
                try
                {
                    await conn.OpenAsync();
                    // Optional: Begin transaction if multiple related updates
                    string query = @"
                        UPDATE USERS
                        SET TenDangNhap = @TenDangNhap, MatKhau = @MatKhau, VaiTroID = @VaiTroID
                        WHERE UserID = @UserID";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@TenDangNhap", user.TenDangNhap);
                        cmd.Parameters.AddWithValue("@MatKhau", user.MatKhau); // Nhắc lại: Mã hóa mật khẩu!
                        cmd.Parameters.AddWithValue("@VaiTroID", user.VaiTroID);
                        cmd.Parameters.AddWithValue("@UserID", user.UserID); // Dùng cho điều kiện WHERE

                        int rowsAffected = await cmd.ExecuteNonQueryAsync();
                        if (rowsAffected == 0)
                        {
                            // Nếu không có hàng nào bị ảnh hưởng, có thể UserID không tồn tại
                            throw new InvalidOperationException($"Không tìm thấy người dùng với UserID: {user.UserID} để cập nhật.");
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    throw new Exception($"Lỗi MySQL khi cập nhật người dùng: {ex.Message}", ex);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Lỗi chung khi cập nhật người dùng: {ex.Message}", ex);
                }
            }
        }

        // --- 4. Xóa Người dùng (Delete User) ---

        public async Task DeleteUserAsync(string userID)
        {
            if (string.IsNullOrWhiteSpace(userID))
            {
                throw new ArgumentException("UserID không được trống khi xóa.", nameof(userID));
            }

            using (MySqlConnection conn = GetConnection())
            {
                await conn.OpenAsync();
                // Transaction vẫn tốt để giữ cho tính nhất quán hoặc nếu bạn có thêm logic sau này.
                using (MySqlTransaction transaction = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        string query = "DELETE FROM USERS WHERE UserID = @UserID";
                        using (MySqlCommand cmd = new MySqlCommand(query, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@UserID", userID);
                            int rowsAffected = await cmd.ExecuteNonQueryAsync();
                            if (rowsAffected == 0)
                            {
                                throw new InvalidOperationException($"Không tìm thấy người dùng với UserID: {userID} để xóa.");
                            }
                        }
                        await transaction.CommitAsync();
                    }
                    catch (MySqlException ex)
                    {
                        await transaction.RollbackAsync();
                        // Lỗi 1451 (Cannot delete or update a parent row) sẽ không xảy ra nếu ON DELETE CASCADE được cấu hình đúng.
                        // Nếu vẫn xảy ra, nghĩa là có FK nào đó chưa có CASCADE, hoặc có lỗi khác.
                        throw new Exception($"Lỗi MySQL khi xóa người dùng: {ex.Message}", ex);
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        throw new Exception($"Lỗi chung khi xóa người dùng: {ex.Message}", ex);
                    }
                }
            }
        }

        // --- Các phương thức trợ giúp (Helper Methods) --

        // Kiểm tra xem TenDangNhap đã tồn tại chưa
        private async Task<bool> IsTenDangNhapExistsAsync(string tenDangNhap, MySqlConnection conn, MySqlTransaction transaction = null)
        {
            string query = "SELECT COUNT(*) FROM USERS WHERE TenDangNhap = @TenDangNhap";
            using (MySqlCommand cmd = new MySqlCommand(query, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@TenDangNhap", tenDangNhap);
                long count = (long)await cmd.ExecuteScalarAsync();
                return count > 0;
            }
        }

        // Phương thức lấy User theo UserID (hữu ích cho Update, Delete)
        public async Task<User> GetUserByIDAsync(string userID)
        {
            if (string.IsNullOrWhiteSpace(userID)) return null;

            string query = @"
                SELECT u.UserID, u.TenDangNhap, u.MatKhau, u.VaiTroID,
                       vt.TenVaiTro
                FROM USERS u
                JOIN VAITRO vt ON u.VaiTroID = vt.VaiTroID
                WHERE u.UserID = @UserID";

            using (MySqlConnection conn = GetConnection())
            {
                try
                {
                    await conn.OpenAsync();
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserID", userID);
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                return new User
                                {
                                    UserID = reader["UserID"].ToString(),
                                    TenDangNhap = reader["TenDangNhap"].ToString(),
                                    MatKhau = reader["MatKhau"].ToString(),
                                    VaiTroID = Convert.ToInt32(reader["VaiTroID"]),
                                    VaiTro = new VaiTro
                                    {
                                        VaiTroID = Convert.ToInt32(reader["VaiTroID"]),
                                        TenVaiTro = reader["TenVaiTro"].ToString(),
                                    }
                                };
                            }
                            return null; // Không tìm thấy
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    throw new Exception($"Lỗi MySQL khi lấy người dùng theo ID: {ex.Message}", ex);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Lỗi chung khi lấy người dùng theo ID: {ex.Message}", ex);
                }
            }
        }

        // Phương thức lấy User theo TenDangNhap (hữu ích cho đăng nhập)
        public async Task<User> GetUserByTenDangNhapAsync(string tenDangNhap)
        {
            if (string.IsNullOrWhiteSpace(tenDangNhap)) return null;

            string query = @"
                SELECT u.UserID, u.TenDangNhap, u.MatKhau, u.VaiTroID,
                       vt.TenVaiTro
                FROM USERS u
                JOIN VAITRO vt ON u.VaiTroID = vt.VaiTroID
                WHERE u.TenDangNhap = @TenDangNhap";

            using (MySqlConnection conn = GetConnection())
            {
                try
                {
                    await conn.OpenAsync();
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@TenDangNhap", tenDangNhap);
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                return new User
                                {
                                    UserID = reader["UserID"].ToString(),
                                    TenDangNhap = reader["TenDangNhap"].ToString(),
                                    MatKhau = reader["MatKhau"].ToString(),
                                    VaiTroID = Convert.ToInt32(reader["VaiTroID"]),
                                    VaiTro = new VaiTro
                                    {
                                        VaiTroID = Convert.ToInt32(reader["VaiTroID"]),
                                        TenVaiTro = reader["TenVaiTro"].ToString(),
                                    }
                                };
                            }
                            return null; // Không tìm thấy
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    throw new Exception($"Lỗi MySQL khi lấy người dùng theo tên đăng nhập: {ex.Message}", ex);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Lỗi chung khi lấy người dùng theo tên đăng nhập: {ex.Message}", ex);
                }
            }
        }
    }
}