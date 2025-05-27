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

namespace QuanLyHocSinh.ViewModel.TraCuu
{
    public class TraCuuDiemHocSinhViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Diem> _danhSachDiem;
        public ObservableCollection<Diem> DanhSachDiem
        {
            get => _danhSachDiem;
            set { _danhSachDiem = value; OnPropertyChanged(nameof(DanhSachDiem)); }
        }

        private ObservableCollection<Diem> _allDiem;

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

        //Lọc theo môn học
        private string _selectedMonHoc;
        public string SelectedMonHoc
        {
            get => _selectedMonHoc;
            set { _selectedMonHoc = value; OnPropertyChanged(nameof(SelectedMonHoc)); Filter(); }
        }

        //Danh sách năm học
        private ObservableCollection<string> _danhSachNamHoc;
        public ObservableCollection<string> DanhSachNamHoc
        {
            get => _danhSachNamHoc;
            set { _danhSachNamHoc = value; OnPropertyChanged(nameof(DanhSachNamHoc)); }
        }

        //Danh sách học kỳ
        private ObservableCollection<string> _danhSachHocKy;
        public ObservableCollection<string> DanhSachHocKy
        {
            get => _danhSachHocKy;
            set { _danhSachHocKy = value; OnPropertyChanged(nameof(DanhSachHocKy)); }
        }

        //Danh sách lớp
        private ObservableCollection<string> _danhSachLop;
        public ObservableCollection<string> DanhSachLop
        {
            get => _danhSachLop;
            set { _danhSachLop = value; OnPropertyChanged(nameof(DanhSachLop)); }
        }

        //Danh sách môn học
        private ObservableCollection<string> _danhSachMonHoc;
        public ObservableCollection<string> DanhSachMonHoc
        {
            get => _danhSachMonHoc;
            set { _danhSachMonHoc = value; OnPropertyChanged(nameof(DanhSachMonHoc)); }
        }

        //Danh sách năm học
        private string _selectedNamHoc;
        public string SelectedNamHoc
        {
            get => _selectedNamHoc;
            set { _selectedNamHoc = value; OnPropertyChanged(nameof(SelectedNamHoc)); Filter(); }
        }

        //Danh sách học kỳ
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

        //Lệnh edit và filter
        public ICommand EditCommand { get; }
        public ICommand FilterCommand { get; }

        public TraCuuDiemHocSinhViewModel()
        {
            _allDiem = new ObservableCollection<Diem>(DiemDAL.GetAllDiemHocSinh());
            DanhSachDiem = new ObservableCollection<Diem>(_allDiem);

            //Lấy danh sách lớp và môn học duy nhất
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

            //Khởi tạo các lệnh
            EditCommand = new RelayCommand(EditDiem, () => SelectedDiem != null);
            FilterCommand = new RelayCommand(Filter);

            //Mặc định chọn "Tất cả"
            SelectedLop = "Tất cả";
            SelectedMonHoc = "Tất cả";
            SelectedNamHoc = "Tất cả";
            SelectedHocKy = "Tất cả";
        }

        private void Filter()
        {
            DanhSachDiem = new ObservableCollection<Diem>(
                _allDiem.Where(d => 
                    (SelectedLop == "Tất cả" || d.Lop == SelectedLop) &&
                    (SelectedMonHoc == "Tất cả" || d.MonHoc == SelectedMonHoc) &&
                    (SelectedNamHoc == "Tất cả" || d.NamHocID == SelectedNamHoc) &&
                    (SelectedHocKy == "Tất cả" || d.HocKy.ToString() == SelectedHocKy)
                )
            );
        }

        private void EditDiem()
        {
            if (SelectedDiem == null) return;

            var dialog = new SuaDiemDialog(SelectedDiem);
            if (dialog.ShowDialog() == true)
            {
                // Refresh danh sách sau khi sửa
                _allDiem = new ObservableCollection<Diem>(DiemDAL.GetAllDiemHocSinh());
                Filter();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
