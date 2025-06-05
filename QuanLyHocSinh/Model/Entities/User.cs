using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyHocSinh.Model.Entities
{
    public class User
    {
        public string UserID { get; set; } = string.Empty;
        public string TenDangNhap { get; set; } = string.Empty;
        public string MatKhau { get; set; } = string.Empty;
        public string VaiTroID { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;

        // Khởi tạo mặc định để tránh null
        public VaiTro VaiTro { get; set; } = new VaiTro();
    }
}