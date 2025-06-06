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
        public string PhanQuyenID { get; set; } = string.Empty;

        public string QuyenID { get; set; } = string.Empty;

        public string GiaoVuPhanQuyenID { get; set; } = string.Empty;

        public string UserDuocPhanQuyenID { get; set; } = string.Empty;

        public DateTime NgayPhanQuyen { get; set; }
        // Navigation properties
        public virtual Quyen Quyen { get; set; }
        public virtual GiaoVu GiaoVuPhanQuyen { get; set; } 
        public virtual User UserDuocPhanQuyen { get; set; } 
    }
}
