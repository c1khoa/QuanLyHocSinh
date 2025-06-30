using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using QuanLyHocSinh.Model.Entities;
using System.Configuration;

namespace QuanLyHocSinh.ViewModel.TraCuu
{
    public class LopItem : INotifyPropertyChanged
    {
        public string TenLop { get; set; }
        
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class MonHocItem : INotifyPropertyChanged
    {
        public string TenMonHoc { get; set; }
        
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

namespace QuanLyHocSinh.ViewModel.TraCuu
{
    public class SuaGiaoVienViewModel : INotifyPropertyChanged
    {
        private string _hoTen;
        public string HoTen
        {
            get => _hoTen;
            set { _hoTen = value; OnPropertyChanged(); }
        }

        private string _gioiTinh;
        public string GioiTinh
        {
            get => _gioiTinh;
            set { _gioiTinh = value; OnPropertyChanged(); }
        } 

        private DateTime _ngaySinh;
        public DateTime NgaySinh
        {
            get => _ngaySinh;
            set { _ngaySinh = value; OnPropertyChanged(); }
        }

        private string _email;
        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        private string _diaChi;
        public string DiaChi
        {
            get => _diaChi;
            set { _diaChi = value; OnPropertyChanged(); }
        }
        
        public ObservableCollection<string> DanhSachGioiTinh { get; } = 
            new ObservableCollection<string>(new[] { "Nam", "Nữ" });

        // Danh sách items cho việc chọn lớp và môn
        public ObservableCollection<LopItem> DanhSachLopItems { get; set; }
        public ObservableCollection<MonHocItem> DanhSachMonItems { get; set; }

        public ICommand SaveCommandGV { get; }
        public ICommand CancelCommandGV { get; }

        public event Action<bool> CloseDialog;
        
        private readonly GiaoVien _giaoVienGoc;

        public SuaGiaoVienViewModel(GiaoVien giaoVien)
        {
            _giaoVienGoc = giaoVien;

            HoTen = giaoVien.HoTen;
            GioiTinh = giaoVien.GioiTinh;
            NgaySinh = giaoVien.NgaySinh;
            Email = giaoVien.Email;
            DiaChi = giaoVien.DiaChi;

            // Khởi tạo danh sách lớp items
            var tatCaLop = GiaoVienDAL.GetAllLop();
            var lopDaDuocChon = new List<string>();
            
            if (!string.IsNullOrEmpty(giaoVien.LopDayID) && giaoVien.LopDayID != "Chưa phân công")
            {
                lopDaDuocChon = giaoVien.LopDayID.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries)
                                                 .Select(l => l.Trim()).ToList();
            }
            
            DanhSachLopItems = new ObservableCollection<LopItem>();
            foreach (var lop in tatCaLop)
            {
                DanhSachLopItems.Add(new LopItem 
                { 
                    TenLop = lop, 
                    IsSelected = lopDaDuocChon.Contains(lop) 
                });
            }

            // Khởi tạo danh sách môn học items
            InitializeMonHocItemsAsync(giaoVien.BoMon);

            SaveCommandGV = new RelayCommand(Save);
            CancelCommandGV = new RelayCommand(Cancel);
        }

        private async void InitializeMonHocItemsAsync(string currentBoMon)
        {
            try
            {
                var tatCaMonHoc = await GetAllMonHocAsync();
                var monDaDuocChon = new List<string>();
                
                if (!string.IsNullOrEmpty(currentBoMon) && currentBoMon != "Chưa phân công")
                {
                    monDaDuocChon = currentBoMon.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries)
                                                   .Select(m => m.Trim()).ToList();
                }
                
                DanhSachMonItems = new ObservableCollection<MonHocItem>();
                foreach (var mon in tatCaMonHoc)
                {
                    DanhSachMonItems.Add(new MonHocItem 
                    { 
                        TenMonHoc = mon, 
                        IsSelected = monDaDuocChon.Contains(mon) 
                    });
                }
                OnPropertyChanged(nameof(DanhSachMonItems));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi khởi tạo danh sách môn học: {ex.Message}");
                DanhSachMonItems = new ObservableCollection<MonHocItem>();
            }
        }
        
        private async System.Threading.Tasks.Task<List<string>> GetAllMonHocAsync()
        {
            try
            {
                var monHocDAL = new QuanLyHocSinh.Model.Entities.MonHocDAL();
                var monHocList = await monHocDAL.GetAllMonHocAsync();
                return monHocList.Select(mh => mh.TenMonHoc).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách môn học: {ex.Message}");
                return new List<string>();
            }
        }

        private void Save()
        {
            if (string.IsNullOrWhiteSpace(HoTen) || string.IsNullOrWhiteSpace(GioiTinh) ||
                string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(DiaChi))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin cơ bản!");
                return;
            }

            if (!Email.Contains("@") || (!Email.EndsWith(".com") && !Email.EndsWith(".vn")))
            {
                MessageBox.Show("Email phải có định dạng @teacher.com hoặc @teacher.vn!");
                return;
            }

            if (!Email.Contains("@teacher."))
            {
                MessageBox.Show("Email giáo viên phải có định dạng @teacher.com hoặc @teacher.vn!");
                return;
            }

            // Cập nhật thông tin cá nhân
            _giaoVienGoc.HoTen = HoTen;
            _giaoVienGoc.GioiTinh = GioiTinh;
            _giaoVienGoc.NgaySinh = NgaySinh;
            _giaoVienGoc.Email = Email;
            _giaoVienGoc.DiaChi = DiaChi;

            try
            {
                // Cập nhật thông tin cá nhân
                GiaoVienDAL.UpdateGiaoVien(_giaoVienGoc);
                
                // Cập nhật phân công dạy (lớp và môn)
                var lopDuocChon = DanhSachLopItems.Where(l => l.IsSelected).Select(l => l.TenLop).ToList();
                var monDuocChon = DanhSachMonItems.Where(m => m.IsSelected).Select(m => m.TenMonHoc).ToList();
                
                GiaoVienDAL.UpdatePhanCongDay(_giaoVienGoc.MaGV, lopDuocChon, monDuocChon);
                
                MessageBox.Show("Cập nhật thông tin thành công!");
                CloseDialog?.Invoke(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật: {ex.Message}");
            }
        }



        private void Cancel()
        {
            CloseDialog?.Invoke(false);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
