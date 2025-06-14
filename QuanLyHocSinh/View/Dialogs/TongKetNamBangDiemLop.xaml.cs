using System.Windows;

namespace QuanLyHocSinh.View.Dialogs
{
    public partial class TongKetNamBangDiemLop : Window
    {
        public TongKetNamBangDiemLop(string namHoc, int hocKy, string lopMacDinh)
        {
            InitializeComponent();
            var vm = new QuanLyHocSinh.ViewModel.BaoCao.TongKetNamBangDiemLopViewModel();
            vm.NamHoc = namHoc;
            vm.SelectedHocKy = hocKy == 0 ? "Cả năm" : hocKy.ToString();
            vm.SelectedLop = lopMacDinh;
            this.DataContext = vm;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
