using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuanLyHocSinh.Model.Entities;
namespace QuanLyHocSinh.Model.Relationships
{
    public class ChiTietMonHoc
    {
        public string ChiTietMonHocID { get; set; }
        public string GiaoVienID { get; set; }
        public string MonHocID { get; set; }
        public string LopDayID { get; set; }
        public DateTime NgayDay { get; set; }
        public string NoiDungDay { get; set; }

        public virtual GiaoVien GiaoVien { get; set; }
        public virtual MonHoc MonHoc { get; set; }
        public virtual Lop Lop { get; set; } 
    }
}
