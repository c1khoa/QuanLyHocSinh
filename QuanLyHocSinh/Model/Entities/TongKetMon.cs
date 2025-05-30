using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyHocSinh.Model.Entities
{
    public class TongKetMonItem
    {
        public int STT { get; set; }
        public string HocSinhID { get; set; } = "";
        public string HoTen { get; set; } = "";
        public string TenLop { get; set; } = "";
        public string MonHoc { get; set; } = "";
        public string NamHoc { get; set; } = "";
        public int HocKy { get; set; }
        public double DiemTrungBinh { get; set; }
        public string XepLoai { get; set; } = "";
        public string GhiChu { get; set; } = "";
        public double DiemMieng { get; set; } = 0;
        public double Diem15Phut { get; set; } = 0;
        public double Diem1Tiet { get; set; } = 0;
        public double DiemThi { get; set; } = 0;
        public string DiemMiengStr => double.IsNaN(DiemMieng) ? "-" : DiemMieng.ToString();
        public string Diem15PhutStr => double.IsNaN(Diem15Phut) ? "-" : Diem15Phut.ToString();
        public string Diem1TietStr => double.IsNaN(Diem1Tiet) ? "-" : Diem1Tiet.ToString();
        public string DiemThiStr => double.IsNaN(DiemThi) ? "-" : DiemThi.ToString();
    }
}
