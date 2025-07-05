using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace QuanLyHocSinh.Model.Entities
{
    public class QuyDinhTuoiEntities
    {
        public string QuyDinhTuoiID { get; set; } // "QDHS" hoặc "QDGV"
        public int TuoiToiThieu { get; set; }
        public int TuoiToiDa { get; set; }
    }

}