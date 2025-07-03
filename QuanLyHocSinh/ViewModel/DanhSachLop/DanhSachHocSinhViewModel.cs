using QuanLyHocSinh.Model.Entities;
using System.Collections.ObjectModel;

namespace QuanLyHocSinh.ViewModel
{
    public class DanhSachHocSinhViewModel : BaseViewModel
    {
        public string TenLop { get; set; }
        public ObservableCollection<HocSinh> DanhSachHocSinh { get; set; }

        public DanhSachHocSinhViewModel(Lop lop, IEnumerable<HocSinh> hocSinhs)
        {
            TenLop = $"Danh sách học sinh lớp: {lop.TenLop}";
            DanhSachHocSinh = new ObservableCollection<HocSinh>(hocSinhs);
        }
    }
}