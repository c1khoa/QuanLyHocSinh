using QuanLyHocSinh.Model.Entities;
using QuanLyHocSinh.View.Dialogs;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows;
using System.Threading.Tasks;
using System.Linq;
namespace QuanLyHocSinh.ViewModel.QuyDinh
{
    public class QuyDinhMainViewModel : BaseViewModel
    {
        private MainViewModel _mainVM;

        // Quy định tuổi
        private QuyDinhTuoiEntities _quyDinhHocSinh;
        public QuyDinhTuoiEntities QuyDinhHocSinh { get => _quyDinhHocSinh; set { _quyDinhHocSinh = value; OnPropertyChanged(); } }

        private QuyDinhTuoiEntities _quyDinhGiaoVien;
        public QuyDinhTuoiEntities QuyDinhGiaoVien { get => _quyDinhGiaoVien; set { _quyDinhGiaoVien = value; OnPropertyChanged(); } }

        // Quy định môn học
        private int _soLuongMonHoc;
        public int SoLuongMonHoc { get => _soLuongMonHoc; set { _soLuongMonHoc = value; OnPropertyChanged(); } }

        private float _diemDat;
        public float DiemDat { get => _diemDat; set { _diemDat = value; OnPropertyChanged(); } }

        public ObservableCollection<MonHoc> DanhSachMonHoc { get; set; } = new();

        // Quy định lớp
        private int _siSoLopToiDa;
        public int SiSoLopToiDa { get => _siSoLopToiDa; set { _siSoLopToiDa = value; OnPropertyChanged(); } }

        public ObservableCollection<Lop> DanhSachLop { get; set; } = new();

        // Quy định khác
        private string _quyDinhKhac;
        public string QuyDinhKhac { get => _quyDinhKhac; set { _quyDinhKhac = value; OnPropertyChanged(); } }

        // Các command chỉnh sửa
        public ICommand EditQuyDinhTuoiCommand { get; }
        public ICommand EditQuyDinhMonHocCommand { get; }
        public ICommand EditQuyDinhLopCommand { get; }
        public ICommand EditQuyDinhKhacCommand { get; }

        public QuyDinhMainViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM;

            EditQuyDinhTuoiCommand = new RelayCommand(OpenEditQuyDinhTuoiDialog);
            EditQuyDinhMonHocCommand = new RelayCommand(OpenEditQuyDinhMonHocDialog);
            EditQuyDinhLopCommand = new RelayCommand(OpenEditQuyDinhLopDialog);
            EditQuyDinhKhacCommand = new RelayCommand(OpenEditQuyDinhKhacDialog);

            LoadData();
        }

        // Tải dữ liệu từ DAL/service
        private async Task LoadData()
        {
            try
            {
                QuyDinhHocSinh = QuyDinhTuoiDAL.GetQuyDinhTuoi("QDHS");
                QuyDinhGiaoVien = QuyDinhTuoiDAL.GetQuyDinhTuoi("QDGV");

                var quyDinh = QuyDinhDAL.GetQuyDinh();
                SoLuongMonHoc = quyDinh.SoLuongMonHoc;
                DiemDat = quyDinh.DiemDat;
                SiSoLopToiDa = quyDinh.SiSoLop_ToiDa;
                QuyDinhKhac = quyDinh.QuyDinhKhac;

                var monHocDal = new MonHocDAL();
                DanhSachMonHoc = new ObservableCollection<MonHoc>(await monHocDal.GetAllMonHocAsync());
                OnPropertyChanged(nameof(DanhSachMonHoc));

                var lopDal = new LopDAL();
                DanhSachLop = new ObservableCollection<Lop>(await lopDal.GetAllLopAsync());
                OnPropertyChanged(nameof(DanhSachLop));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải dữ liệu quy định: " + ex.Message);
            }
        }

        // Các hàm mở dialog chỉnh sửa
        // Sửa quy định tuổi
        private async void OpenEditQuyDinhTuoiDialog()
        {

            var dialog = new SuaQuyDinhTuoiDialog();
            var vm = new SuaQuyDinhTuoiViewModel(dialog, QuyDinhHocSinh, QuyDinhGiaoVien);
            dialog.DataContext = vm;
            if (dialog.ShowDialog() == true)
                await LoadData();
        }

        // Sửa quy định môn học
        private async void OpenEditQuyDinhMonHocDialog()
        {
            var danhSachMonHocClone = DanhSachMonHoc.Select(monHoc => monHoc.Clone()).ToList();
            var dialog = new SuaQuyDinhMonHocDialog();
            var vm = new SuaQuyDinhMonHocViewModel(dialog, SoLuongMonHoc, DiemDat, danhSachMonHocClone);
            dialog.DataContext = vm;
            if (dialog.ShowDialog() == true)
                await LoadData();
        }

        // Sửa quy định lớp
        private async void OpenEditQuyDinhLopDialog()
        {
            var danhSachLopClone = DanhSachLop.Select(lop => lop.Clone()).ToList();
            var dialog = new SuaQuyDinhLopDialog();
            var vm = new SuaQuyDinhLopViewModel(dialog, SiSoLopToiDa, danhSachLopClone);
            dialog.DataContext = vm;
            if (dialog.ShowDialog() == true)
                await LoadData();
        }
        
        private async void OpenEditQuyDinhKhacDialog()
        {
            var dialog = new SuaQuyDinhKhacDialog();
            var vm = new SuaQuyDinhKhacViewModel(dialog, QuyDinhKhac);
            dialog.DataContext = vm;
            if (dialog.ShowDialog() == true)
                await LoadData();
        }
    }
}