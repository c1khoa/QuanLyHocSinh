using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuanLyHocSinh.Model.Entities;
namespace QuanLyHocSinh.Model.Relationships
{
    public class ChiTietQuyen
    {
        public string ChiTietQuyenID { get; set; } // CHAR(8), e.g., CTQ00001
        public string QuyenID { get; set; } 
        public string VaiTroID { get; set; } 
        public string TuongTac { get; set; } // CHAR(8) in DB

        // Navigation properties
        public virtual Quyen Quyen { get; set; }
        public virtual VaiTro VaiTro { get; set; }
    }
}
