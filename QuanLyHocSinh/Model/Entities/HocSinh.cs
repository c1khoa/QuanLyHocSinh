using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuanLyHocSinh.Model.Entities;

namespace QuanLyHocSinh.Model.Entities
{
    public class HocSinh
    {
        public string HocSinhID { get; set; }
        public string UserID { get; set; }
        public string HoTen { get; set; }
        public string GioiTinh { get; set; }
        public DateTime NgaySinh { get; set; }
        public string Email { get; set; }
        public string DiaChi { get; set; }
        public string TenLop { get; set; }
        public int NienKhoa { get; set; }
        public string TrangThaiHoSo { get; set; }

        public string HoSoID { get; set; }
        public User User { get; set; } 
        public HoSo HoSo { get; set; }
        public Lop Lop { get; set; }
        public List<Diem> Diems { get; set; } = new List<Diem>();
    }

    // Entity mới cho danh sách học sinh với điểm TB HK1, HK2
    public class HocSinhDanhSachItem
    {
        public int STT { get; set; }
        public string HocSinhID { get; set; } = "";
        public string HoTen { get; set; } = "";
        public string TenLop { get; set; } = "";
        public double? DiemTBHK1 { get; set; }
        public double? DiemTBHK2 { get; set; }
        public string NamHoc { get; set; } = "";
        
        public string GioiTinh { get; set; } = "";
        public DateTime NgaySinh { get; set; }
        public string Email { get; set; } = "";
        public string DiaChi { get; set; } = "";
        public int NienKhoa { get; set; }
    }

    // Entity mới cho trang Thông tin danh sách lớp
    public class HocSinhLopItem
    {
        public int STT { get; set; }
        public string HocSinhID { get; set; } = "";
        public string HoTen { get; set; } = "";
        public string GioiTinh { get; set; } = "";
        public DateTime NgaySinh { get; set; }
        public int NamSinh { get; set; }
        public string Email { get; set; } = "";
        public string DiaChi { get; set; } = "";
        public string TenLop { get; set; } = "";
        public int NienKhoa { get; set; }
    }
}
