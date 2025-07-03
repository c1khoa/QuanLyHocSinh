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
using System.Windows;

namespace QuanLyHocSinh.ViewModel.TraCuu
{
    public class TraCuuDiemHocSinhViewModel : BaseViewModel
    {
        private ObservableCollection<Diem> _danhSachDiem;
        public ObservableCollection<Diem> DanhSachDiem
        {
            get => _danhSachDiem;
            set { _danhSachDiem = value; OnPropertyChanged(nameof(DanhSachDiem)); }
        }

        private ObservableCollection<Diem> _allDiem;
    
        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(nameof(SearchText)); Filter(); }
        }

        private string _selectedLop;
        public string SelectedLop
        {
            get => _selectedLop;
            set { _selectedLop = value; OnPropertyChanged(nameof(SelectedLop)); Filter(); }
        }

        private string _selectedMonHoc;
        public string SelectedMonHoc
        {
            get => _selectedMonHoc;
            set { _selectedMonHoc = value; OnPropertyChanged(nameof(SelectedMonHoc)); Filter(); }
        }

        private ObservableCollection<string> _danhSachNamHoc;
        public ObservableCollection<string> DanhSachNamHoc
        {
            get => _danhSachNamHoc;
            set { _danhSachNamHoc = value; OnPropertyChanged(nameof(DanhSachNamHoc)); }
        }

        private ObservableCollection<string> _danhSachHocKy;
        public ObservableCollection<string> DanhSachHocKy
        {
            get => _danhSachHocKy;
            set { _danhSachHocKy = value; OnPropertyChanged(nameof(DanhSachHocKy)); }
        }

        private ObservableCollection<string> _danhSachLop;
        public ObservableCollection<string> DanhSachLop
        {
            get => _danhSachLop;
            set { _danhSachLop = value; OnPropertyChanged(nameof(DanhSachLop)); }
        }

        private ObservableCollection<string> _danhSachMonHoc;
        public ObservableCollection<string> DanhSachMonHoc
        {
            get => _danhSachMonHoc;
            set { _danhSachMonHoc = value; OnPropertyChanged(nameof(DanhSachMonHoc)); }
        }

        private string _selectedNamHoc;
        public string SelectedNamHoc
        {
            get => _selectedNamHoc;
            set { _selectedNamHoc = value; OnPropertyChanged(nameof(SelectedNamHoc)); Filter(); }
        }

        private string _selectedHocKy;
        public string SelectedHocKy
        {
            get => _selectedHocKy;
            set { _selectedHocKy = value; OnPropertyChanged(nameof(SelectedHocKy)); Filter(); }
        }

        private Diem _selectedDiem;
        public Diem SelectedDiem
        {
            get => _selectedDiem;
            set { _selectedDiem = value; OnPropertyChanged(nameof(SelectedDiem)); }
        }

        public ICommand EditCommand { get; }
        public ICommand FilterCommand { get; }

        private MainViewModel _mainVM;
        public TraCuuDiemHocSinhViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM;
            _allDiem = new ObservableCollection<Diem>(DiemDAL.GetAllDiemHocSinh());
            DanhSachDiem = new ObservableCollection<Diem>(_allDiem);

            var dsLop = _allDiem.Select(d => d.Lop).Distinct().OrderBy(l => l).ToList();
            dsLop.Insert(0, "Tất cả");
            DanhSachLop = new ObservableCollection<string>(dsLop);

            var dsMonHoc = _allDiem.Select(d => d.MonHoc).Distinct().OrderBy(m => m).ToList();
            dsMonHoc.Insert(0, "Tất cả");
            DanhSachMonHoc = new ObservableCollection<string>(dsMonHoc);

            var dsNamHoc = _allDiem.Select(d => d.NamHocID).Distinct().OrderBy(n => n).ToList();
            dsNamHoc.Insert(0, "Tất cả");
            DanhSachNamHoc = new ObservableCollection<string>(dsNamHoc);

            var dsHocKy = _allDiem.Select(d => d.HocKy.ToString()).Distinct().OrderBy(h => h).ToList();
            dsHocKy.Insert(0, "Tất cả");
            DanhSachHocKy = new ObservableCollection<string>(dsHocKy);

            EditCommand = new RelayCommand(EditDiem, () => SelectedDiem != null);
            FilterCommand = new RelayCommand(Filter);

            SelectedLop = "Tất cả";
            SelectedMonHoc = "Tất cả";
            SelectedNamHoc = "Tất cả";
            SelectedHocKy = "Tất cả";
        }

        private void Filter()
        {
            DanhSachDiem = new ObservableCollection<Diem>(
                _allDiem.Where(d =>
                    (string.IsNullOrEmpty(SearchText) || d.HoTen.ToLower().Contains(SearchText.ToLower()))
                    && (SelectedLop == "Tất cả" || d.Lop == SelectedLop)
                    && (SelectedMonHoc == "Tất cả" || d.MonHoc == SelectedMonHoc)
                    && (SelectedNamHoc == "Tất cả" || d.NamHocID == SelectedNamHoc)
                    && (SelectedHocKy == "Tất cả" || d.HocKy.ToString() == SelectedHocKy)
                )
            );
        }


        private void EditDiem()
        {
            if (SelectedDiem == null) return;

            var dialog = new SuaDiemDialog(SelectedDiem);
            if (dialog.ShowDialog() == true)
            {
                _allDiem = new ObservableCollection<Diem>(DiemDAL.GetAllDiemHocSinh());
                Filter();
            }
        }
    }
}