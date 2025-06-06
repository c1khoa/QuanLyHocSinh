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
        public string ChiTietQuyenID { get; set; } = string.Empty;

        public string QuyenID { get; set; } = string.Empty;

        public string VaiTroID { get; set; } = string.Empty;

        public string TuongTac { get; set; } = string.Empty;

        // Navigation properties
        public virtual Quyen Quyen { get; set; }
        public virtual VaiTro VaiTro { get; set; }
    }
}
