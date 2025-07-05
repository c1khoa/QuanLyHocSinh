using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyHocSinh.Model.Entities
{
    public class MonHoc
    {
        public string MonHocID { get; set; }
        public string TenMonHoc { get; set; }
        public MonHoc Clone()
        {
            return (MonHoc)this.MemberwiseClone();
        }
    }
}
