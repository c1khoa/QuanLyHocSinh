using QuanLyHocSinh.Model.Entities;
using System.Collections.Generic;

namespace QuanLyHocSinh.Service
{
    public static class UserService
    {
        private static List<User> _users = new List<User>();

        public static List<User> LayDanhSachTaiKhoan()
        {
            return _users;
        }

        public static bool ThemTaiKhoan(User user)
        {
            if (_users.Exists(u => u.TenDangNhap == user.TenDangNhap))
            {
                return false; // Tên đăng nhập đã tồn tại
            }

            _users.Add(user);
            return true;
        }
    }
}
