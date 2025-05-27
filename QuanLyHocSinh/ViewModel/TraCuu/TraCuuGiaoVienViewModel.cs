using System;
using System.Windows.Input;
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

namespace QuanLyHocSinh.ViewModel.TraCuu
{
    public class TraCuuGiaoVienViewModel : BaseViewModel
    {
        private ObservableCollection<GiaoVien> _danhSachGiaoVien;
        public ObservableCollection<GiaoVien> DanhSachGiaoVien
        {
            get => _danhSachGiaoVien;
            set { _danhSachGiaoVien = value; OnPropertyChanged(nameof(DanhSachGiaoVien)); }
        }

        private ObservableCollection<GiaoVien> _allGiaoVien;

        //Lọc theo search
        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(nameof(SearchText)); Filter(); }
        }

        //Lọc theo bộ môn
        private string _selectedBoMon;
        public string SelectedBoMon
        {
            get => _selectedBoMon;
            set { _selectedBoMon = value; OnPropertyChanged(nameof(SelectedBoMon)); Filter(); }
        }

        //Lọc theo giới tính
        private string _selectedGioiTinh;
        public string SelectedGioiTinh
        {
            get => _selectedGioiTinh;
            set { _selectedGioiTinh = value; OnPropertyChanged(nameof(SelectedGioiTinh)); Filter(); }
        }

        //Lọc theo lớp
        private string _selectedLop;
        public string SelectedLop
        {
            get => _selectedLop;
            set { _selectedLop = value; OnPropertyChanged(nameof(SelectedLop)); Filter(); }
        }

        //Danh sách bộ môn
        private ObservableCollection<string> _danhSachBoMon;
        public ObservableCollection<string> DanhSachBoMon
        {
            get => _danhSachBoMon;
            set { _danhSachBoMon = value; OnPropertyChanged(nameof(DanhSachBoMon)); }
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

        private GiaoVien _selectedGiaoVien;
        public GiaoVien SelectedGiaoVien
        {
            get => _selectedGiaoVien;
            set { _selectedGiaoVien = value; OnPropertyChanged(nameof(SelectedGiaoVien)); }
        }

        //Lệnh edit và filter
        public ICommand EditCommand { get; }
        public ICommand FilterCommand { get; }

        private MainViewModel _mainVM;
        public TraCuuGiaoVienViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM;
            _allGiaoVien = new ObservableCollection<GiaoVien>(GiaoVienDAL.GetAllGiaoVien());
            DanhSachGiaoVien = new ObservableCollection<GiaoVien>(_allGiaoVien);

            //Lấy danh sách bộ môn duy nhất
            var dsBoMon = _allGiaoVien.Select(gv => gv.BoMon).Distinct().OrderBy(bm => bm).ToList();
            dsBoMon.Insert(0, "Tất cả");
            DanhSachBoMon = new ObservableCollection<string>(dsBoMon);

            //Lấy danh sách lớp duy nhất
            var dsLop = _allGiaoVien.Select(gv => gv.LopDayID).Distinct().OrderBy(l => l).ToList();
            dsLop.Insert(0, "Tất cả");
            DanhSachLop = new ObservableCollection<string>(dsLop);

            //Lấy danh sách giới tính duy nhất
            var dsGioiTinh = _allGiaoVien.Select(gv => gv.GioiTinh).Distinct().OrderBy(gt => gt).ToList();
            dsGioiTinh.Insert(0, "Tất cả");
            DanhSachGioiTinh = new ObservableCollection<string>(dsGioiTinh);

            //Khởi tạo các lệnh
            EditCommand = new RelayCommand(EditGiaoVien, () => SelectedGiaoVien != null);
            FilterCommand = new RelayCommand(Filter);

            //Mặc định chọn "Tất cả"
            SelectedBoMon = "Tất cả";
            SelectedGioiTinh = "Tất cả";
            SelectedLop = "Tất cả";
            _mainVM = mainVM;
        }

        private void Filter()
        {
            var filtered = _allGiaoVien.Where(gv =>
                (string.IsNullOrEmpty(SearchText) || gv.HoTen.ToLower().Contains(SearchText.ToLower()))
                && (SelectedBoMon == "Tất cả" || string.IsNullOrEmpty(SelectedBoMon) || gv.BoMon == SelectedBoMon)
                && (SelectedGioiTinh == "Tất cả" || string.IsNullOrEmpty(SelectedGioiTinh) || gv.GioiTinh == SelectedGioiTinh)
                && (SelectedLop == "Tất cả" || string.IsNullOrEmpty(SelectedLop) || gv.LopDayID == SelectedLop)
            ).ToList();

            DanhSachGiaoVien = new ObservableCollection<GiaoVien>(filtered);
        }

        private void EditGiaoVien()
        {
            if (SelectedGiaoVien == null) return;

            var dialog = new SuaGiaoVienDialog(SelectedGiaoVien);
            if (dialog.ShowDialog() == true)
            {
                // Refresh danh sách sau khi sửa
                _allGiaoVien = new ObservableCollection<GiaoVien>(GiaoVienDAL.GetAllGiaoVien());
                Filter();
            }
        }
    }
}
