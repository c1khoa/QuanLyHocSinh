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
using QuanLyHocSinh.Model.DAL;
using QuanLyHocSinh.View.Dialogs;
using System.Configuration;

namespace QuanLyHocSinh.ViewModel.TraCuu
{
    public class LopCheckboxItem : BaseViewModel
    {
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }

        public string TenLop { get; set; }

        public LopCheckboxItem(string tenLop)
        {
            TenLop = tenLop;
            IsSelected = false;
        }
    }

    public class BoMonCheckboxItem : BaseViewModel
    {
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }

        public string TenBoMon { get; set; }

        public BoMonCheckboxItem(string tenBoMon)
        {
            TenBoMon = tenBoMon;
            IsSelected = false;
        }
    }

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

        //Danh sách bộ môn
        private ObservableCollection<string> _danhSachBoMon;
        public ObservableCollection<string> DanhSachBoMon
        {
            get => _danhSachBoMon;
            set { _danhSachBoMon = value; OnPropertyChanged(nameof(DanhSachBoMon)); }
        }

        //Danh sách lớp checkbox
        private ObservableCollection<LopCheckboxItem> _danhSachLopCheckbox;
        public ObservableCollection<LopCheckboxItem> DanhSachLopCheckbox
        {
            get => _danhSachLopCheckbox;
            set { _danhSachLopCheckbox = value; OnPropertyChanged(nameof(DanhSachLopCheckbox)); }
        }

        //Text hiển thị các lớp đã chọn
        public string SelectedLopsText
        {
            get
            {
                var selectedLops = DanhSachLopCheckbox?.Where(x => x.IsSelected).Select(x => x.TenLop).ToList();
                if (selectedLops == null || selectedLops.Count == 0)
                    return "Tất cả";
                if (selectedLops.Count == 1)
                    return selectedLops[0];
                return $"{selectedLops.Count} lớp đã chọn";
            }
        }

        //Danh sách bộ môn checkbox
        private ObservableCollection<BoMonCheckboxItem> _danhSachBoMonCheckbox;
        public ObservableCollection<BoMonCheckboxItem> DanhSachBoMonCheckbox
        {
            get => _danhSachBoMonCheckbox;
            set { _danhSachBoMonCheckbox = value; OnPropertyChanged(nameof(DanhSachBoMonCheckbox)); }
        }

        //Text hiển thị các bộ môn đã chọn
        public string SelectedBoMonsText
        {
            get
            {
                var selectedBoMons = DanhSachBoMonCheckbox?.Where(x => x.IsSelected).Select(x => x.TenBoMon).ToList();
                if (selectedBoMons == null || selectedBoMons.Count == 0)
                    return "Tất cả";
                if (selectedBoMons.Count == 1)
                    return selectedBoMons[0];
                return $"{selectedBoMons.Count} bộ môn đã chọn";
            }
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
        public ICommand LopCheckboxChangedCommand { get; }

        private MainViewModel _mainVM;
        public TraCuuGiaoVienViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM;
            _allGiaoVien = new ObservableCollection<GiaoVien>(GiaoVienDAL.GetAllGiaoVien());
            DanhSachGiaoVien = new ObservableCollection<GiaoVien>(_allGiaoVien);

            var allLopList = new List<string>();
            foreach (var gv in _allGiaoVien)
            {
                if (!string.IsNullOrEmpty(gv.LopDayID))
                {
                    var lopArray = gv.LopDayID.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var lop in lopArray)
                    {
                        var lopTrimmed = lop.Trim();
                        if (!allLopList.Contains(lopTrimmed))
                        {
                            allLopList.Add(lopTrimmed);
                        }
                    }
                }
            }
            allLopList.Sort();
            
            if (_allGiaoVien.Any(gv => string.IsNullOrEmpty(gv.LopDayID) || gv.LopDayID.Trim() == ""))
            {
                allLopList.Add("Chưa phân công");
            }
            
            DanhSachLopCheckbox = new ObservableCollection<LopCheckboxItem>();
            foreach (var lop in allLopList)
            {
                var checkboxItem = new LopCheckboxItem(lop);
                checkboxItem.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(LopCheckboxItem.IsSelected))
                    {
                        Filter();
                        OnPropertyChanged(nameof(SelectedLopsText));
                    }
                };
                DanhSachLopCheckbox.Add(checkboxItem);
            }
            
            OnPropertyChanged(nameof(SelectedLopsText));

            var allBoMonSet = new HashSet<string>();
            foreach (var gv in _allGiaoVien)
            {
                if (!string.IsNullOrEmpty(gv.BoMon))
                {   
                    var monHocList = gv.BoMon.Split(',').Select(m => m.Trim()).Where(m => !string.IsNullOrEmpty(m));
                    foreach (var mon in monHocList)
                    {
                        allBoMonSet.Add(mon);
                    }
                }
            }
            
            var sortedBoMon = allBoMonSet.OrderBy(x => x).ToList();
            if (_allGiaoVien.Any(gv => string.IsNullOrEmpty(gv.BoMon) || gv.BoMon.Trim() == ""))
            {
                sortedBoMon.Add("Chưa phân công");
            }
            
            DanhSachBoMonCheckbox = new ObservableCollection<BoMonCheckboxItem>();
            foreach (var boMon in sortedBoMon)
            {
                var checkboxItem = new BoMonCheckboxItem(boMon);
                checkboxItem.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(BoMonCheckboxItem.IsSelected))
                    {
                        Filter();
                        OnPropertyChanged(nameof(SelectedBoMonsText));
                    }
                };
                DanhSachBoMonCheckbox.Add(checkboxItem);
            }
            
            OnPropertyChanged(nameof(SelectedBoMonsText));

            var dsGioiTinh = _allGiaoVien.Select(gv => gv.GioiTinh).Distinct().OrderBy(gt => gt).ToList();
            dsGioiTinh.Insert(0, "Tất cả");
            DanhSachGioiTinh = new ObservableCollection<string>(dsGioiTinh);

            EditCommand = new RelayCommand(EditGiaoVien, () => SelectedGiaoVien != null);
            FilterCommand = new RelayCommand(Filter);
            LopCheckboxChangedCommand = new RelayCommand(Filter);

            SelectedBoMon = "Tất cả";
            SelectedGioiTinh = "Tất cả";
            _mainVM = mainVM;
        }

        private void Filter()
        {
            var selectedLops = DanhSachLopCheckbox?.Where(x => x.IsSelected).Select(x => x.TenLop).ToList() ?? new List<string>();
            var selectedBoMons = DanhSachBoMonCheckbox?.Where(x => x.IsSelected).Select(x => x.TenBoMon).ToList() ?? new List<string>();

            var filtered = _allGiaoVien.Where(gv =>
                (string.IsNullOrEmpty(SearchText) || gv.HoTen.ToLower().Contains(SearchText.ToLower()))
                && (selectedBoMons.Count == 0 || IsGiaoVienTeachesAllSelectedBoMons(gv, selectedBoMons))
                && (SelectedGioiTinh == "Tất cả" || string.IsNullOrEmpty(SelectedGioiTinh) || gv.GioiTinh == SelectedGioiTinh)
                && (selectedLops.Count == 0 || IsGiaoVienTeachesAllSelectedLops(gv, selectedLops))
            ).ToList();

            DanhSachGiaoVien = new ObservableCollection<GiaoVien>(filtered);
            OnPropertyChanged(nameof(SelectedLopsText));
            OnPropertyChanged(nameof(SelectedBoMonsText));
        }

        private bool IsGiaoVienTeachesAllSelectedLops(GiaoVien gv, List<string> selectedLops)
        {
            if (selectedLops.Contains("Chưa phân công"))
            {
                return string.IsNullOrEmpty(gv.LopDayID) || gv.LopDayID.Trim() == "";
            }
            
            if (string.IsNullOrEmpty(gv.LopDayID))
                return false;

            var lopArrayOfGiaoVien = gv.LopDayID.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                                .Select(l => l.Trim())
                                                .ToList();

            return selectedLops.All(selectedLop => lopArrayOfGiaoVien.Contains(selectedLop));
        }

        private bool IsGiaoVienTeachesAllSelectedBoMons(GiaoVien gv, List<string> selectedBoMons)
        {
            if (selectedBoMons.Contains("Chưa phân công"))
            {
                return string.IsNullOrEmpty(gv.BoMon) || gv.BoMon.Trim() == "";
            }
            
            if (string.IsNullOrEmpty(gv.BoMon))
                return false;

            var boMonArrayOfGiaoVien = gv.BoMon.Split(',')
                                              .Select(m => m.Trim())
                                              .Where(m => !string.IsNullOrEmpty(m))
                                              .ToList();

            return selectedBoMons.All(selectedBoMon => boMonArrayOfGiaoVien.Contains(selectedBoMon));
        }

        private void EditGiaoVien()
        {
            if (SelectedGiaoVien == null) return;

            var dialog = new SuaGiaoVienDialog(SelectedGiaoVien);
            if (dialog.ShowDialog() == true)
            {
                _allGiaoVien = new ObservableCollection<GiaoVien>(GiaoVienDAL.GetAllGiaoVien());
                
                RefreshLopCheckboxList();
                RefreshBoMonCheckboxList();
                
                Filter();
            }
        }

        private void RefreshLopCheckboxList()
        {
            var currentSelectedLops = DanhSachLopCheckbox?.Where(x => x.IsSelected).Select(x => x.TenLop).ToList() ?? new List<string>();

            var allLopList = new List<string>();
            foreach (var gv in _allGiaoVien)
            {
                if (!string.IsNullOrEmpty(gv.LopDayID))
                {
                    var lopArray = gv.LopDayID.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var lop in lopArray)
                    {
                        var lopTrimmed = lop.Trim();
                        if (!allLopList.Contains(lopTrimmed))
                        {
                            allLopList.Add(lopTrimmed);
                        }
                    }
                }
            }
            allLopList.Sort();

            if (_allGiaoVien.Any(gv => string.IsNullOrEmpty(gv.LopDayID) || gv.LopDayID.Trim() == ""))
            {
                allLopList.Add("Chưa phân công");
            }

            DanhSachLopCheckbox = new ObservableCollection<LopCheckboxItem>();
            foreach (var lop in allLopList)
            {
                var checkboxItem = new LopCheckboxItem(lop);
                checkboxItem.IsSelected = currentSelectedLops.Contains(lop);
                checkboxItem.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(LopCheckboxItem.IsSelected))
                    {
                        Filter();
                        OnPropertyChanged(nameof(SelectedLopsText));
                    }
                };
                DanhSachLopCheckbox.Add(checkboxItem);
            }
            
            OnPropertyChanged(nameof(SelectedLopsText));
        }

        private void RefreshBoMonCheckboxList()
        {
            var currentSelectedBoMons = DanhSachBoMonCheckbox?.Where(x => x.IsSelected).Select(x => x.TenBoMon).ToList() ?? new List<string>();

            var allBoMonSet = new HashSet<string>();
            foreach (var gv in _allGiaoVien)
            {
                if (!string.IsNullOrEmpty(gv.BoMon))
                {
                    var monHocList = gv.BoMon.Split(',').Select(m => m.Trim()).Where(m => !string.IsNullOrEmpty(m));
                    foreach (var mon in monHocList)
                    {
                        allBoMonSet.Add(mon);
                    }
                }
            }
                    
            var sortedBoMon = allBoMonSet.OrderBy(x => x).ToList();
            if (_allGiaoVien.Any(gv => string.IsNullOrEmpty(gv.BoMon) || gv.BoMon.Trim() == ""))
            {
                sortedBoMon.Add("Chưa phân công");
            }
            
            DanhSachBoMonCheckbox = new ObservableCollection<BoMonCheckboxItem>();
            foreach (var boMon in sortedBoMon)
            {
                var checkboxItem = new BoMonCheckboxItem(boMon);
                checkboxItem.IsSelected = currentSelectedBoMons.Contains(boMon);
                checkboxItem.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(BoMonCheckboxItem.IsSelected))
                    {
                        Filter();
                        OnPropertyChanged(nameof(SelectedBoMonsText));
                    }
                };
                DanhSachBoMonCheckbox.Add(checkboxItem);
            }

            OnPropertyChanged(nameof(SelectedBoMonsText));
        }
    }
}
