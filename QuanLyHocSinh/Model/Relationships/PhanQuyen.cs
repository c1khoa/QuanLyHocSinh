using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyHocSinh.Model.Relationships
{
    class PhanQuyen
    {
        public string PhanQuyenID { get; set; } = string.Empty;
        public string QuyenID { get; set; } = string.Empty;
        public string GiaoVuPhanQuyenID { get; set; } = string.Empty;
        public string UserDuocPhanQuyenID { get; set; } = string.Empty;
        public DateTime NgayPhanQuyen { get; set; }
    }
}
