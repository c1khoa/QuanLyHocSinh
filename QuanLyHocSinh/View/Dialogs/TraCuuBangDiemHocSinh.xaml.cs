using System.Windows;
using QuanLyHocSinh.Model.Entities;
using QuanLyHocSinh.ViewModel.TraCuu;

namespace QuanLyHocSinh.View.Dialogs
{
    public partial class BangDiemHocSinh : Window
    {
        public BangDiemHocSinh()
        {
            InitializeComponent();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void SuaDiem_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button;
            var item = button.DataContext as TongKetMonItem;
            if (item == null) return;

            var vm = DataContext as TraCuuBangDiemHocSinhViewModel;
            if (vm == null) return;

            var diem = new Diem
            {
                MaHS = vm.HocSinhID,
                HoTen = vm.HoTen,
                Lop = vm.TenLop,
                MonHoc = item.MonHoc,
                NamHocID = vm.NamHoc,
                HocKy = vm.HocKy,
                DiemMieng = (float?)item.DiemMieng,
                Diem15p = (float?)item.Diem15Phut,
                Diem1Tiet = (float?)item.Diem1Tiet,
                DiemThi = (float?)item.DiemThi,
                DiemTB = (float?)item.DiemTrungBinh
            };

            var dialog = new SuaDiemDialog(diem);
            if (dialog.ShowDialog() == true)
            {
                // Refresh bảng điểm sau khi sửa
                vm.RefreshBangDiem();
            }
        }
    }
}
