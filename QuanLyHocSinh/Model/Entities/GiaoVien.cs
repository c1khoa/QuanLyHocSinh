using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuanLyHocSinh.Model.Entities;

namespace QuanLyHocSinh.Model.Entities
{
    public class GiaoVien
    {
        public string UserID { get; set; }
        public string MaGV { get; set; }
        public string HoSoID { get; set; }
        public string HoTen { get; set; }
        public DateTime NgaySinh { get; set; }
        public string GioiTinh { get; set; }
        public string LopDayID { get; set; }
        public string Email { get; set; }
        public string BoMon { get; set; }
        public string DiaChi { get; set; }
        public User User { get; set; }
        public HoSo HoSo { get; set; }
    }
}
