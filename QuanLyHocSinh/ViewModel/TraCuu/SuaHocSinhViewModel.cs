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
    public class SuaHocSinhViewModel : BaseViewModel
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

        private string _tenLop;
        public string TenLop
        {
            get => _tenLop;
            set { _tenLop = value; OnPropertyChanged(); }
        }

        private int _nienKhoa;
        public int NienKhoa
        {
            get => _nienKhoa;
            set { _nienKhoa = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> DanhSachGioiTinh { get; } =
            new ObservableCollection<string> { "Nam", "Nữ" };

        public ObservableCollection<string> DanhSachLop { get; set; }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event Action<bool> CloseDialog;

        private readonly HocSinh _hocSinhGoc;

        public SuaHocSinhViewModel(HocSinh hocSinh)
        {
            _hocSinhGoc = hocSinh;

            HoTen = hocSinh.HoTen;
            GioiTinh = hocSinh.GioiTinh;
            NgaySinh = hocSinh.NgaySinh;
            Email = hocSinh.Email;
            DiaChi = hocSinh.DiaChi;            TenLop = hocSinh.TenLop;
            NienKhoa = hocSinh.NienKhoa;

            // Lấy tất cả lớp từ database, không chỉ lớp có học sinh
            DanhSachLop = new ObservableCollection<string>(
                HocSinhDAL.GetAllLop()
            );

            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(Cancel);
        }

        private void Save()
        {
            if (string.IsNullOrWhiteSpace(HoTen) || string.IsNullOrWhiteSpace(GioiTinh) ||
                string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(DiaChi) ||
                string.IsNullOrWhiteSpace(TenLop) || NienKhoa <= 0)
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin!");
                return;
            }

            if (!Email.Contains("@") || (!Email.EndsWith(".com") && !Email.EndsWith(".vn")))
            {
                MessageBox.Show("Email phải có định dạng @student.com hoặc @student.vn!");
                return;
            }

            if (!Email.Contains("@student."))
            {
                MessageBox.Show("Email học sinh phải có định dạng @student.com hoặc @student.vn!");
                return;
            }

            _hocSinhGoc.HoTen = HoTen;
            _hocSinhGoc.GioiTinh = GioiTinh;
            _hocSinhGoc.NgaySinh = NgaySinh;
            _hocSinhGoc.Email = Email;
            _hocSinhGoc.DiaChi = DiaChi;
            _hocSinhGoc.TenLop = TenLop;
            _hocSinhGoc.NienKhoa = NienKhoa;

            try
            {
                HocSinhDAL.UpdateHocSinh(_hocSinhGoc);
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

    }
}