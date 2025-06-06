    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using QuanLyHocSinh.Model.Entities;

    namespace QuanLyHocSinh.Model.Entities
    {
        public class Diem
        {
            public string DiemID { get; set; }
            public string MaHS { get; set; }
            public string HoTen { get; set; }
            public string Lop { get; set; }
            public string MonHoc { get; set; }
            public string MonHocID { get; set; }
            public float? DiemMieng { get; set; }
            public float? Diem15p { get; set; }
            public float? Diem1Tiet { get; set; }
            public float? DiemThi { get; set; }
            public float? DiemTB { get; set; }
            public string XepLoai { get; set; }
            public string NamHocID { get; set; }
            public int HocKy { get; set; }
        }
    }
