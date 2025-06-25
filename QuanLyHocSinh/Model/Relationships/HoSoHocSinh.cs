using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuanLyHocSinh.Model.Entities;
namespace QuanLyHocSinh.Model.Relationships
{
    public class HoSoHocSinh
    {
        public string HoSoHocSinhID { get; set; } 
        public string HocSinhID { get; set; } 
        public string HoSoID { get; set; }    
        public string LopHocID { get; set; } 
        public int NienKhoa { get; set; }

        // Navigation properties
        public virtual HocSinh HocSinh { get; set; }
        public virtual HoSo HoSo { get; set; }
        public virtual Lop Lop { get; set; } 
    }
}
