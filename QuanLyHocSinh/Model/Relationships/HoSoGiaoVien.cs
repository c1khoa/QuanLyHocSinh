using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuanLyHocSinh.Model.Entities;
namespace QuanLyHocSinh.Model.Relationships
{
    public class HoSoGiaoVien
    {
        public string HoSoGiaoVienID { get; set; } // CHAR(8), e.g., HGV001
        public string GiaoVienID { get; set; } 
        public string HoSoID { get; set; }     
        public string LopDayID { get; set; }   // CHAR(4) for LOP.LopID (Lớp GVCN)
                                               // Schema has CHAR(8) for HOSOGIAOVIEN.LopDayID, potential mismatch
        public DateTime NgayBatDauLamViec { get; set; }

        // Navigation properties
        public virtual GiaoVien GiaoVien { get; set; }
        public virtual HoSo HoSo { get; set; }
        public virtual Lop Lop { get; set; } 
    }
}
