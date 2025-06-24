using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyHocSinh.Model.Entities
{
    public class ChucVu
    {
        public string ChucVuID { get; set; }
        public string TenChucVu { get; set; }
        public string MoTa { get; set; }
        public string VaiTroID { get; set; } // Khóa ngoại

        // Navigation property (tùy chọn)
        public VaiTro VaiTro { get; set; }

        public static implicit operator string?(ChucVu? v)
        {
            throw new NotImplementedException();
        }
    }
}
