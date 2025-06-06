using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyHocSinh.Model.Entities
{
    public class GiaoVu
    {
        public string GiaoVuID { get; set; } 
        public string UserID { get; set; }   

        public string TenDangNhap { get; set; }
        public string MatKhau { get; set; } 
        public string VaiTroID { get; set; }

        public User User { get; set; } 
    }
}
