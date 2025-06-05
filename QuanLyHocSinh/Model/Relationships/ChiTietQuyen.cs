using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyHocSinh.Model.Relationships
{
    class ChiTietQuyen
    {
        public string ChiTietQuyenID { get; set; } = string.Empty;
        public string QuyenID { get; set; } = string.Empty;
        public string VaiTroID { get; set; } = string.Empty;
        public string TuongTac { get; set; } = string.Empty;
    }
}
