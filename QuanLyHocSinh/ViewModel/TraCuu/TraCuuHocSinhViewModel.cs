using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using QuanLyHocSinh.Model.Entities;
using QuanLyHocSinh.Model.DAL;
using QuanLyHocSinh.View.Dialogs;
using System.Configuration;
using QuanLyHocSinh.ViewModel;
using System.Windows;

namespace QuanLyHocSinh.ViewModel.TraCuu
{
    public class TraCuuHocSinhViewModel : BaseViewModel
    {
        //Danh sách học sinh
        private ObservableCollection<HocSinhDanhSachItem> _danhSachHocSinh;
        public ObservableCollection<HocSinhDanhSachItem> DanhSachHocSinh
        {
            get => _danhSachHocSinh;
            set { _danhSachHocSinh = value; OnPropertyChanged(nameof(DanhSachHocSinh)); }
        }

        //Tất cả học sinh
        private List<HocSinhDanhSachItem> _allHocSinh;

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

        //Danh sách lớp
        private ObservableCollection<string> _danhSachLop;
        public ObservableCollection<string> DanhSachLop
        {
            get => _danhSachLop;
            set { _danhSachLop = value; OnPropertyChanged(nameof(DanhSachLop)); }
        }

        //Học sinh được chọn
        private HocSinhDanhSachItem _selectedHocSinh;
        public HocSinhDanhSachItem SelectedHocSinh
        {
            get => _selectedHocSinh;
            set { _selectedHocSinh = value; OnPropertyChanged(nameof(SelectedHocSinh)); }
        }

        public ICommand FilterCommand { get; }
        public ICommand XemBangDiemCommand { get; }

        private MainViewModel _mainVM;
        public TraCuuHocSinhViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM;
            LoadData();

            FilterCommand = new RelayCommand(Filter);
            XemBangDiemCommand = new RelayCommand<HocSinhDanhSachItem>(XemBangDiem);

            SelectedLop = "Tất cả";
        }

        private void LoadData()
        {
            string currentNamHoc = GetCurrentNamHoc();
            
            if (_mainVM.CurrentUser.VaiTro.VaiTroID == "VT02")
            {
                var danhSachLopCuaGV = GiaoVienDAL.GetLopDayCuaUser(_mainVM.CurrentUser.UserID);
                
                _allHocSinh = HocSinhDAL.GetDanhSachHocSinhWithDiemTB(null, null, null, currentNamHoc)
                    .Where(hs => danhSachLopCuaGV.Contains(hs.TenLop)).ToList();
            }
            else if (_mainVM.CurrentUser.VaiTro.VaiTroID == "VT01")
            {
                var danhSachLopCuaHS = HocSinhDAL.GetLopHocCuaUser(_mainVM.CurrentUser.UserID);
                
                _allHocSinh = HocSinhDAL.GetDanhSachHocSinhWithDiemTB(null, null, null, currentNamHoc)
                    .Where(hs => danhSachLopCuaHS.Contains(hs.TenLop)).ToList();
            }
            else
            {
                _allHocSinh = HocSinhDAL.GetDanhSachHocSinhWithDiemTB(null, null, null, currentNamHoc);
            }

            var dsLop = _allHocSinh.Select(hs => hs.TenLop).Distinct().OrderBy(l => l).ToList();
            dsLop.Insert(0, "Tất cả");
            DanhSachLop = new ObservableCollection<string>(dsLop);

            Filter();
        }

        private string GetCurrentNamHoc()
        {
            var allNamHoc = HocSinhDAL.GetAllNamHoc();
            return allNamHoc.OrderByDescending(x => x).FirstOrDefault() ?? DateTime.Now.Year.ToString();
        }

        //Lọc học sinh theo tên và lớp
        private void Filter()
        {
            if (_allHocSinh == null) return;

            var filtered = _allHocSinh.Where(hs =>
                (string.IsNullOrEmpty(SearchText) || hs.HoTen.ToLower().Contains(SearchText.ToLower()))
                && (SelectedLop == "Tất cả" || string.IsNullOrEmpty(SelectedLop) || hs.TenLop == SelectedLop)
            ).ToList();

            int stt = 1;
            foreach (var item in filtered)
            {
                item.STT = stt++;
            }

            DanhSachHocSinh = new ObservableCollection<HocSinhDanhSachItem>(filtered);
        }

        private void XemBangDiem(HocSinhDanhSachItem hsItem)
        {
            if (hsItem == null) return;

            var hocSinh = new HocSinh
            {
                HocSinhID = hsItem.HocSinhID,
                HoTen = hsItem.HoTen,
                GioiTinh = hsItem.GioiTinh,
                NgaySinh = hsItem.NgaySinh,
                Email = hsItem.Email,
                DiaChi = hsItem.DiaChi,
                TenLop = hsItem.TenLop,
                NienKhoa = hsItem.NienKhoa
            };

            string namHoc = null;
            int? hocKy = null;

            var dialog = new BangDiemHocSinh();
            var viewModel = new TraCuuBangDiemHocSinhViewModel(hocSinh, _mainVM, namHoc, hocKy);
            
            
            viewModel.RequestMainWindowRefresh += () => {
                LoadData();
            };
            
            dialog.DataContext = viewModel;
            dialog.ShowDialog();
        }
    }
}