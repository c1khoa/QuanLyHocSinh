using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyHocSinh.Model.Entities
{
    public class TongKetNamHocItem
    {
        public int STT { get; set; }
        public string HocSinhID { get; set; }
        public string HoTen { get; set; }
        public string TenLop { get; set; }
        public string NamHoc { get; set; }
        public int HocKy { get; set; }
        public double DiemTrungBinh { get; set; }
        public string XepLoai { get; set; }
        public string GhiChu { get; set; }
        public double? DiemTBHK1 { get; set; }
        public double? DiemTBHK2 { get; set; }
        public double? DiemTBCaNam { get; set; }
        public string KetQua { get; set; }
    }

    public class BangDiemLopItem
    {
        public int STT { get; set; }
        public string HocSinhID { get; set; }
        public string HoTen { get; set; }
        public Dictionary<string, double> DiemTungMon { get; set; } = new(); // key: tên môn, value: điểm TB
        public double DiemTrungBinh { get; set; }
        public string XepLoai { get; set; }
        // Có thể bổ sung property thống kê nếu cần
    }

    public class ThongKeNamHoc
    {
        public double DiemTrungBinhLop { get; set; }
        public List<(string MonHoc, double DiemTB)> DiemTrungBinhTungMon { get; set; } = new();
        public double TiLeDat { get; set; }
        public int SoLuongGioi { get; set; }
        public int SoLuongKha { get; set; }
        public int SoLuongTrungBinh { get; set; }
        public int SoLuongYeu { get; set; }
        public int SoLuongKem { get; set; }
    }

    public class HocSinhThongTin
    {
        public string HoTen { get; set; }
        public string HocSinhID { get; set; }
        public string TenLop { get; set; }
        public string NamHoc { get; set; }
        public DateTime NgaySinh { get; set; }
    }

    public class BangDiemMonHocItem
    {
        public string MonHoc { get; set; }
        public double? DiemTBHK1 { get; set; }
        public double? DiemTBHK2 { get; set; }
        public double? DiemTBCaNam { get; set; }
    }
}
