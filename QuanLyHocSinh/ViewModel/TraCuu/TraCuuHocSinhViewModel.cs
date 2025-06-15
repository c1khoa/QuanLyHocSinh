using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using QuanLyHocSinh.Model.Entities;
using QuanLyHocSinh.View.Dialogs;
using System.Configuration;
using QuanLyHocSinh.ViewModel;
using System.Windows;

namespace QuanLyHocSinh.ViewModel.TraCuu
{
    public class TraCuuHocSinhViewModel : BaseViewModel
    {
        //Danh sách học sinh
        private ObservableCollection<HocSinh> _danhSachHocSinh;
        public ObservableCollection<HocSinh> DanhSachHocSinh
        {
            get => _danhSachHocSinh;
            set { _danhSachHocSinh = value; OnPropertyChanged(nameof(DanhSachHocSinh)); }
        }

        //Tất cả học sinh
        private ObservableCollection<HocSinh> _allHocSinh;

        //Lọc theo search
        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(nameof(SearchText)); Filter(); }
        }

        //Lọc theo lớp
        private string _selectedLop;
        public string SelectedLop
        {
            get => _selectedLop;
            set { _selectedLop = value; OnPropertyChanged(nameof(SelectedLop)); Filter(); }
        }

        //Lọc theo giới tính
        private string _selectedGioiTinh;
        public string SelectedGioiTinh
        {
            get => _selectedGioiTinh;
            set { _selectedGioiTinh = value; OnPropertyChanged(nameof(SelectedGioiTinh)); Filter(); }
        }

        //Lọc theo niên khóa
        private string _selectedNienKhoa;
        public string SelectedNienKhoa
        {
            get => _selectedNienKhoa;
            set { _selectedNienKhoa = value; OnPropertyChanged(nameof(SelectedNienKhoa)); Filter(); }
        }

        //Danh sách lớp
        private ObservableCollection<string> _danhSachLop;
        public ObservableCollection<string> DanhSachLop
        {
            get => _danhSachLop;
            set { _danhSachLop = value; OnPropertyChanged(nameof(DanhSachLop)); }
        }

        //Danh sách giới tính
        private ObservableCollection<string> _danhSachGioiTinh;
        public ObservableCollection<string> DanhSachGioiTinh
        {
            get => _danhSachGioiTinh;
            set { _danhSachGioiTinh = value; OnPropertyChanged(nameof(DanhSachGioiTinh)); }
        }

        //Danh sách niên khóa
        private ObservableCollection<string> _danhSachNienKhoa;
        public ObservableCollection<string> DanhSachNienKhoa
        {
            get => _danhSachNienKhoa;
            set { _danhSachNienKhoa = value; OnPropertyChanged(nameof(DanhSachNienKhoa)); }
        }

        private HocSinh _selectedHocSinh;
        public HocSinh SelectedHocSinh
        {
            get => _selectedHocSinh;
            set { _selectedHocSinh = value; OnPropertyChanged(nameof(SelectedHocSinh)); }
        }

        //Lệnh edit và filter
        public ICommand EditCommand { get; }
        public ICommand FilterCommand { get; }
        public ICommand XemBangDiemCommand { get; }

        //Khởi tạo ViewModel
        private MainViewModel _mainVM;
        public TraCuuHocSinhViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM;
            if (_mainVM.CurrentUser.VaiTro.VaiTroID == "VT02")
            {
                // Lấy danh sách lớp mà giáo viên đang dạy
                var danhSachLopCuaGV = GiaoVienDAL.GetLopDayCuaUser(_mainVM.CurrentUser.UserID);

                // Lấy học sinh chỉ trong các lớp đó
                _allHocSinh = new ObservableCollection<HocSinh>(
                    HocSinhDAL.GetAllHocSinh().Where(hs => danhSachLopCuaGV.Contains(hs.TenLop))
                );
                
            }
            else if (_mainVM.CurrentUser.VaiTro.VaiTroID == "VT01")
            {
                // Lấy danh sách lớp mà giáo viên đang dạy
                var danhSachLopCuaHS = HocSinhDAL.GetLopHocCuaUser(_mainVM.CurrentUser.UserID);

                // Lấy học sinh chỉ trong các lớp đó
                _allHocSinh = new ObservableCollection<HocSinh>(
                    HocSinhDAL.GetAllHocSinh().Where(hs => danhSachLopCuaHS.Contains(hs.TenLop))
                );

            }
            else
            {
                // Nếu là giáo vụ hoặc admin, load toàn bộ học sinh
                _allHocSinh = new ObservableCollection<HocSinh>(HocSinhDAL.GetAllHocSinh());
            }

            // Lấy danh sách lớp duy nhất từ danh sách học sinh
            var dsLop = _allHocSinh.Select(hs => hs.TenLop).Distinct().OrderBy(l => l).ToList();
            dsLop.Insert(0, "Tất cả"); // Thêm giá trị "Tất cả" vào đầu danh sách
            DanhSachLop = new ObservableCollection<string>(dsLop);

            //Lấy danh sách giới tính duy nhất từ danh sách học sinh
            var dsGioiTinh = _allHocSinh.Select(hs => hs.GioiTinh).Distinct().OrderBy(gt => gt).ToList();
            dsGioiTinh.Insert(0, "Tất cả");
            DanhSachGioiTinh = new ObservableCollection<string>(dsGioiTinh);

            //Lấy danh sách niên khóa duy nhất từ danh sách học sinh
            DanhSachNienKhoa = new ObservableCollection<string>(HocSinhDAL.GetAllNienKhoa());
            DanhSachNienKhoa.Insert(0, "Tất cả");

            //Khởi tạo các lệnh
            EditCommand = new RelayCommand(EditHocSinh, () => SelectedHocSinh != null);
            FilterCommand = new RelayCommand(Filter);
            XemBangDiemCommand = new RelayCommand<HocSinh>(XemBangDiem);

            // Mặc định chọn "Tất cả"
            SelectedLop = "Tất cả";
            SelectedGioiTinh = "Tất cả";
            SelectedNienKhoa = "Tất cả";
        }

        //Lọc học sinh theo tên và lớp
        private void Filter()
        {
            var filtered = _allHocSinh.Where(hs =>
                (string.IsNullOrEmpty(SearchText) || hs.HoTen.ToLower().Contains(SearchText.ToLower()))
                && (SelectedLop == "Tất cả" || string.IsNullOrEmpty(SelectedLop) || hs.TenLop == SelectedLop)
                && (SelectedGioiTinh == "Tất cả" || string.IsNullOrEmpty(SelectedGioiTinh) || hs.GioiTinh == SelectedGioiTinh)
                && (SelectedNienKhoa == "Tất cả" || string.IsNullOrEmpty(SelectedNienKhoa) || hs.NienKhoa.ToString() == SelectedNienKhoa)
            ).ToList();

            DanhSachHocSinh = new ObservableCollection<HocSinh>(filtered);
        }

        private void EditHocSinh()
        {
            if (SelectedHocSinh == null) return;

            var dialog = new SuaHocSinhDialog(SelectedHocSinh);
            if (dialog.ShowDialog() == true)
            {
                // Refresh danh sách sau khi sửa
                _allHocSinh = new ObservableCollection<HocSinh>(HocSinhDAL.GetAllHocSinh());
                Filter();
            }
        }

        private void XemBangDiem(HocSinh hs)
        {
            if (hs == null) return;

            string namHoc = SelectedNienKhoa != null && SelectedNienKhoa != "Tất cả" ? SelectedNienKhoa : null;
            int? hocKy = null;

            var dialog = new BangDiemHocSinh(); // assuming using directive is present
            dialog.DataContext = new TraCuuBangDiemHocSinhViewModel(hs, _mainVM, namHoc, hocKy);
            dialog.ShowDialog();
        }

    }
}