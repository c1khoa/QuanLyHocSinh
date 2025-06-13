using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyHocSinh.Model.Entities
{
    public class User
    {
        public string UserID { get; set; }
        public string TenDangNhap { get; set; }
        public string MatKhau { get; set; }

        public string VaiTroID { get; set; }
        public string HoTen {  get; set; }

        public VaiTro VaiTro { get; set; }  // Navigation property

        public object Clone()
        {
            return new User
            {
                UserID = this.UserID,
                TenDangNhap = this.TenDangNhap,
                MatKhau = this.MatKhau,
                VaiTroID = this.VaiTroID,
                HoTen = this.HoTen,
                VaiTro = this.VaiTro
            };
        }
    }

    public class QLHocSinhEntities : DbContext
    {
        public DbSet<User> Users { get; set; }
    
    }

}
