using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyHocSinh.Model.Entities
{
    public class Lop
    {
        public string LopID { get; set; }
        public string TenLop { get; set; }
        public int SiSo { get; set; }
        public string GVCNID { get; set; }
        public GiaoVien GVCN { get; set; }
        public Lop Clone()
        {
            // MemberwiseClone tạo một bản sao nông, đủ cho các kiểu dữ liệu giá trị (string, int,..)
            return (Lop)this.MemberwiseClone();
        }
    }
}
