using QuanLyHocSinh.Model.Entities;
using QuanLyHocSinh.ViewModel;
using System.Collections.Generic;
using System.Windows;

namespace QuanLyHocSinh.View
{
    public partial class DanhSachHocSinhWindow : Window
    {
        public DanhSachHocSinhWindow(Lop lop, IEnumerable<HocSinh> hocSinhs)
        {
            InitializeComponent();
            DataContext = new DanhSachHocSinhViewModel(lop, hocSinhs);
        }
    }
}