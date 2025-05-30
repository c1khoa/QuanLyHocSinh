using QuanLyHocSinh.Model.Entities;

namespace QuanLyHocSinh.ViewModel.BaoCao
{
    public class TongKetMonDetailDialogViewModel
    {
        public TongKetMonItem Item { get; set; }
        public TongKetMonDetailDialogViewModel(TongKetMonItem item)
        {
            Item = item;
        }
    }
}
