using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuanLyHocSinh.Model.Entities;

namespace QuanLyHocSinh.Model.Relationships
{
    public class HoSoGiaoVu
    {
        public string HoSoGiaoVuID { get; set; } // CHAR(10), e.g., HGU001
        public string GiaoVuID { get; set; } 
        public string HoSoID { get; set; }     // CHAR(12) according to new database

        // Navigation properties
        public virtual GiaoVu GiaoVu { get; set; }
        public virtual HoSo HoSo { get; set; }
    }
} 