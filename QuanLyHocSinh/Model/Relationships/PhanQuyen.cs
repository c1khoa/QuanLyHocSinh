using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuanLyHocSinh.Model.Entities;
namespace QuanLyHocSinh.Model.Relationships
{
    public class PhanQuyen
    {
        public string PhanQuyenID { get; set; } // CHAR(8), e.g., PQ000001
        public string QuyenID { get; set; }               
        public string GiaoVuPhanQuyenID { get; set; }   
        public string UserDuocPhanQuyenID { get; set; } 
        public DateTime NgayPhanQuyen { get; set; }

        // Navigation properties
        public virtual Quyen Quyen { get; set; }
        public virtual GiaoVu GiaoVuPhanQuyen { get; set; } 
        public virtual User UserDuocPhanQuyen { get; set; } 
    }
}
