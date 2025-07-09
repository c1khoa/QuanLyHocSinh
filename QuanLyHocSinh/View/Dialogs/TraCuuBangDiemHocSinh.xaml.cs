using System.Windows;
using QuanLyHocSinh.Model.Entities;
using QuanLyHocSinh.ViewModel;
using QuanLyHocSinh.ViewModel.TraCuu;

namespace QuanLyHocSinh.View.Dialogs
{
    public partial class BangDiemHocSinh : Window
    {
        public BangDiemHocSinh()
        {
            InitializeComponent();
            
            // Subscribe event khi DataContext thay đổi
            this.DataContextChanged += BangDiemHocSinh_DataContextChanged;
        }
        
        private void BangDiemHocSinh_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // Unsubscribe từ ViewModel cũ nếu có
            if (e.OldValue is TraCuuBangDiemHocSinhViewModel oldVm)
            {
                oldVm.RequestDialogActivation -= OnRequestDialogActivation;
            }
            
            // Subscribe vào ViewModel mới
            if (e.NewValue is TraCuuBangDiemHocSinhViewModel newVm)
            {
                newVm.RequestDialogActivation += OnRequestDialogActivation;
            }
        }
        
        private void OnRequestDialogActivation()
        {
            // Activate dialog để đưa lên trên
            this.Activate();
            this.Topmost = true;
            this.Topmost = false;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as TraCuuBangDiemHocSinhViewModel;
            vm?.NotifyMainWindowRefresh();
            
            // Unsubscribe event trước khi đóng để tránh memory leak
            if (vm != null)
            {
                vm.RequestDialogActivation -= OnRequestDialogActivation;
            }
            
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
            dialog.Owner = this;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            if (dialog.ShowDialog() == true)
            {
                vm.RefreshBangDiem();
                
                this.Activate();
                this.Topmost = true;
                this.Topmost = false;
            }
        }
        private void DiemDataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is TraCuuBangDiemHocSinhViewModel vm)
            {
                var suaDiemColumn = DiemDataGrid.Columns
                    .FirstOrDefault(c => c.Header?.ToString() == "Sửa điểm");

                if (suaDiemColumn != null)
                {
                    suaDiemColumn.Visibility = vm.IsGiaoVienVisible ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }

    }
}
