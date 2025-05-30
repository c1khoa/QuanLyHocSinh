using System;
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

        private string _lopDayID;
        public string LopDayID
        {
            get => _lopDayID;
            set { _lopDayID = value; OnPropertyChanged(); }
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
        
        private string _boMon;
        public string BoMon
        {
            get => _boMon;
            set { _boMon = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> DanhSachGioiTinh { get; } = 
            new ObservableCollection<string>(new[] { "Nam", "Nữ" });

        public ObservableCollection<string> DanhSachLop { get; set; }

        public ObservableCollection<string> DanhSachBoMon { get; set; }

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
            LopDayID = giaoVien.LopDayID;
            DiaChi = giaoVien.DiaChi;
            BoMon = giaoVien.BoMon;

            DanhSachLop = new ObservableCollection<string>(
                GiaoVienDAL.GetAllLop()
            );

            DanhSachBoMon = new ObservableCollection<string>(
                GiaoVienDAL.GetAllGiaoVien()
                    .Select(gv => gv.BoMon)
                    .Distinct()
                    .OrderBy(bm => bm)

            );

            SaveCommandGV = new RelayCommand(Save);
            CancelCommandGV = new RelayCommand(Cancel);
        }
        
        private void Save()
        {
            if (string.IsNullOrWhiteSpace(HoTen) || string.IsNullOrWhiteSpace(GioiTinh) ||
                string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(DiaChi) ||
                string.IsNullOrWhiteSpace(BoMon) || string.IsNullOrWhiteSpace(LopDayID))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin!");
                return;
            }

            if (!Email.Contains("@") || (!Email.EndsWith(".com") && !Email.EndsWith(".vn")))
            {
                MessageBox.Show("Email không hợp lệ!");
                return;
            }

            _giaoVienGoc.HoTen = HoTen;
            _giaoVienGoc.GioiTinh = GioiTinh;
            _giaoVienGoc.NgaySinh = NgaySinh;
            _giaoVienGoc.Email = Email;
            _giaoVienGoc.DiaChi = DiaChi;
            _giaoVienGoc.BoMon = BoMon;
            _giaoVienGoc.LopDayID = LopDayID;

            try
            {
                GiaoVienDAL.UpdateGiaoVien(_giaoVienGoc);
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
