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
        public string HoSoGiaoVienID { get; set; }
        public string GiaoVienID { get; set; } 
        public string HoSoID { get; set; }    
        public DateTime NgayBatDauLamViec { get; set; }

        // Navigation properties
        public virtual GiaoVien GiaoVien { get; set; }
        public virtual HoSo HoSo { get; set; }
    }
}
