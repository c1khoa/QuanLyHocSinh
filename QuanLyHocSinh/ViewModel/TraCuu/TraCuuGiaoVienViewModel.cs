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
    public class TraCuuGiaoVienViewModel : INotifyPropertyChanged
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

        //Danh sách bộ môn
        private ObservableCollection<string> _danhSachBoMon;
        public ObservableCollection<string> DanhSachBoMon
        {
            get => _danhSachBoMon;
            set { _danhSachBoMon = value; OnPropertyChanged(nameof(DanhSachBoMon)); }
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

        public TraCuuGiaoVienViewModel()
        {
            _allGiaoVien = new ObservableCollection<GiaoVien>(GiaoVienDAL.GetAllGiaoVien());
            DanhSachGiaoVien = new ObservableCollection<GiaoVien>(_allGiaoVien);

            //Lấy danh sách bộ môn duy nhất
            var dsBoMon = _allGiaoVien.Select(gv => gv.BoMon).Distinct().OrderBy(bm => bm).ToList();
            dsBoMon.Insert(0, "Tất cả");
            DanhSachBoMon = new ObservableCollection<string>(dsBoMon);

            //Khởi tạo các lệnh
            EditCommand = new RelayCommand(EditGiaoVien, () => SelectedGiaoVien != null);
            FilterCommand = new RelayCommand(Filter);

            //Mặc định chọn "Tất cả"
            SelectedBoMon = "Tất cả";
        }

        private void Filter()
        {
            var filtered = _allGiaoVien.Where(gv =>
                (string.IsNullOrEmpty(SearchText) || gv.HoTen.ToLower().Contains(SearchText.ToLower()))
                && (SelectedBoMon == "Tất cả" || string.IsNullOrEmpty(SelectedBoMon) || gv.BoMon == SelectedBoMon)
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
