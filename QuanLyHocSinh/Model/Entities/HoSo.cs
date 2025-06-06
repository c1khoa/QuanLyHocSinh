using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyHocSinh.Model.Entities
{
    public class HoSo
    {
        public string HoSoID { get; set; }
        public string HoTen { get; set; }
        public string GioiTinh { get; set; }
        public DateTime NgaySinh { get; set; }
        public string Email { get; set; }
        public string DiaChi { get; set; }
        public string ChucVuID { get; set; } // Khóa ngoại
        public string TrangThaiHoSo { get; set; }
        public DateTime NgayTao { get; set; }
        public DateTime NgayCapNhatGanNhat { get; set; }

        // Navigation property
        public ChucVu ChucVu { get; set; }
    }
}
