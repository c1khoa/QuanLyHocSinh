using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using QuanLyHocSinh.Model.DAL;
using QuanLyHocSinh.Model.Entities;
using MaterialDesignThemes.Wpf;
using QuanLyHocSinh.View.Dialogs.MessageBox;
using System.Configuration;
using QuanLyHocSinh.Service;

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

        private string _chucVu;
        public string ChucVu
        {
            get => _chucVu;
            set { _chucVu = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> DanhSachGioiTinh { get; } =
            new ObservableCollection<string> { "Nam", "Nữ" };

        public ObservableCollection<string> DanhSachLop { get; set; }
        public ObservableCollection<string> DanhSachChucVu { get; set; }
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
            DiaChi = hocSinh.DiaChi;            
            TenLop = hocSinh.TenLop;
            NienKhoa = hocSinh.NienKhoa;
            ChucVu = hocSinh.ChucVu;

            DanhSachLop = new ObservableCollection<string>(
                HocSinhDAL.GetAllLop()
            );
            DanhSachChucVu = new ObservableCollection<string>(
                UserService.LayDanhSachTenChucVuTheoVaiTro("VT01")
            );

            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(Cancel);
        }

        private async void Save()
        {
            try
        {
            if (string.IsNullOrWhiteSpace(HoTen) || string.IsNullOrWhiteSpace(GioiTinh) ||
                string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(DiaChi) ||
                string.IsNullOrWhiteSpace(TenLop) || NienKhoa <= 0)
            {
                    await ShowNotificationAsync("Cảnh báo", "⚠️ Vui lòng nhập đầy đủ thông tin!");
                return;
            }

            if (!Email.Contains("@") || (!Email.EndsWith(".com") && !Email.EndsWith(".vn")))
            {
                    await ShowNotificationAsync("Cảnh báo", "⚠️ Email phải có định dạng @student.com hoặc @student.vn!");
                return;
            }

            if (!Email.Contains("@student."))
            {
                    await ShowNotificationAsync("Cảnh báo", "⚠️ Email học sinh phải có định dạng @student.com hoặc @student.vn!");
                return;
            }

            _hocSinhGoc.HoTen = HoTen;
            _hocSinhGoc.GioiTinh = GioiTinh;
            _hocSinhGoc.NgaySinh = NgaySinh;
            _hocSinhGoc.Email = Email;
            _hocSinhGoc.DiaChi = DiaChi;
            _hocSinhGoc.TenLop = TenLop;
            _hocSinhGoc.NienKhoa = NienKhoa;
            _hocSinhGoc.ChucVu = ChucVu;

                string message;
                bool result = HocSinhDAL.UpdateHocSinh(_hocSinhGoc, out message);
                if (!result)
                {
                    await DialogHost.Show(new ErrorDialog("Cập nhật thất bại", message), "RootDialog_SuaHS");
                    return;
                }
                await DialogHost.Show(new NotifyDialog("Thông báo", "✅ Cập nhật thông tin thành công!"), "RootDialog_SuaHS");
                CloseDialog?.Invoke(true);
            }
            catch (Exception ex)
            {
                await DialogHost.Show(new ErrorDialog("Lỗi", $"❌ Lỗi khi cập nhật: {ex.Message}"), "RootDialog_SuaHS");
            }
        }

        private void Cancel()
        {
            CloseDialog?.Invoke(false);
        }

        private async Task ShowNotificationAsync(string title, string message)
        {
            try
            {
                await DialogHost.Show(new ErrorDialog(title, message), "RootDialog_SuaHS");
            }
            catch
            {
                MessageBox.Show(message, title, MessageBoxButton.OK, 
                    title.Contains("Lỗi") ? MessageBoxImage.Error : MessageBoxImage.Information);
            }
        }


    }
}