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

namespace QuanLyHocSinh.ViewModel.TraCuu
{   
    public class TraCuuHocSinhViewModel : INotifyPropertyChanged
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

        //Danh sách lớp
        private ObservableCollection<string> _danhSachLop;
        public ObservableCollection<string> DanhSachLop
        {
            get => _danhSachLop;
            set { _danhSachLop = value; OnPropertyChanged(nameof(DanhSachLop)); }
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

        //Khởi tạo ViewModel
        public TraCuuHocSinhViewModel()
        {
            _allHocSinh = new ObservableCollection<HocSinh>(HocSinhDAL.GetAllHocSinh());
            DanhSachHocSinh = new ObservableCollection<HocSinh>(_allHocSinh);

            // Lấy danh sách lớp duy nhất từ danh sách học sinh
            var dsLop = _allHocSinh.Select(hs => hs.TenLop).Distinct().OrderBy(l => l).ToList();
            dsLop.Insert(0, "Tất cả"); // Thêm giá trị "Tất cả" vào đầu danh sách
            DanhSachLop = new ObservableCollection<string>(dsLop);

            //Khởi tạo các lệnh
            EditCommand = new RelayCommand(EditHocSinh, () => SelectedHocSinh != null);
            FilterCommand = new RelayCommand(Filter);

            // Mặc định chọn "Tất cả"
            SelectedLop = "Tất cả";
        }
        
        //Lọc học sinh theo tên và lớp
        private void Filter()
        {
            var filtered = _allHocSinh.Where(hs =>
                (string.IsNullOrEmpty(SearchText) || hs.HoTen.ToLower().Contains(SearchText.ToLower()))
                && (SelectedLop == "Tất cả" || string.IsNullOrEmpty(SelectedLop) || hs.TenLop == SelectedLop)
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}