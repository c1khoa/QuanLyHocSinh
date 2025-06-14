using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuanLyHocSinh.Model.Entities;
namespace QuanLyHocSinh.Model.Relationships
{
    public class ChiTietDiem
    {
        public string ChiTietDiemID { get; set; }
        public string DiemID { get; set; }
        public string LoaiDiemID { get; set; }
        public float GiaTri { get; set; }

        // Navigation properties (tùy chọn)
        public virtual Diem Diem { get; set; } 
        public LoaiDiem LoaiDiem { get; set; }
    }
}
