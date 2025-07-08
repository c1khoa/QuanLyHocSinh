using System;
using System.Collections.Generic;
using System.Configuration;
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

    // Entity mới cho tổng kết theo lớp
    public class TongKetLopItem
    {
        public int STT { get; set; }
        public string TenLop { get; set; } = "";
        public string MonHoc { get; set; } = "";
        public string NamHoc { get; set; } = "";
        public int HocKy { get; set; }
        public int SiSo { get; set; }
        public int SoLuongDat { get; set; }
        public double TiLeDat { get; set; }
    }

    // Fix for CS0120: An object reference is required for the non-static field, method, or property 'QuyDinhEntities.DiemDat'

    // Update the `DaDat` property to require an instance of `QuyDinhEntities` to access `DiemDat`.
    public class HocSinhChiTietItem
    {
        public int STT { get; set; }
        public string HocSinhID { get; set; } = "";
        public string HoTen { get; set; } = "";
        public int HocKy { get; set; }
        public double Diem15Phut { get; set; }
        public double Diem1Tiet { get; set; }
        public double DiemTrungBinh { get; set; }
        public string XepLoai { get; set; } = "";

        public bool DaDat => DiemTrungBinh >= GetDiem();

        private float GetDiem()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
            QuyDinhEntities quyDinh = QuyDinhDAL.GetQuyDinh();
            return quyDinh.DiemDat;
        }
    }
}
