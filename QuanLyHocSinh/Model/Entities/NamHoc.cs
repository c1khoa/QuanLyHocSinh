using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyHocSinh.Model.Entities
{
    public class NamHoc
    {
        public string NamHocID { get; set; }
        public string MoTa { get; set; }
        public DateTime BatDau { get; set; }
        public DateTime KetThuc { get; set; }
    }
}
