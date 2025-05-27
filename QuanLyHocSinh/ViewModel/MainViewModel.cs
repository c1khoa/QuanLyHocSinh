using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using QuanLyHocSinh.View.Controls.BaoCao;
using QuanLyHocSinh.View.Controls;
using QuanLyHocSinh.View.Windows;
using QuanLyHocSinh.View.Controls.TraCuu;
using QuanLyHocSinh.View.Controls.QuanLyTaiKhoan;
using QuanLyHocSinh.View.Controls.QuyDinh;
using System.Collections.ObjectModel;
using QuanLyHocSinh.ViewModel.QuanLyTaiKhoan;
using QuanLyHocSinh.ViewModel.TraCuu;
using QuanLyHocSinh.ViewModel.QuyDinh;
using QuanLyHocSinh.ViewModel.BaoCao;
using System.Windows.Controls;
using QuanLyHocSinh.Model.Entities;

namespace QuanLyHocSinh.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        // Các ViewModel con
        public TrangChuViewModel TrangChuVM { get; set; }
        public QuanLyTaiKhoanMainViewModel TaiKhoanVM { get; set; }
        public TraCuuHocSinhViewModel HocSinhVM { get; set; }
        public TraCuuGiaoVienViewModel GiaoVienVM { get; set; }
        public TraCuuDiemHocSinhViewModel DiemHocSinhVM { get; set; }
        public TongKetMonViewModel TongKetMonVM { get; set; }
        public TongKetNamHocViewModel TongKetNamHocVM { get; set; }
        public QuyDinhMainViewModel QuyDinhVM { get; set; }


        private BaseViewModel _currentView;
        public BaseViewModel CurrentView
        {
            get => _currentView;
            set
            {
                if (_currentView != value)
                {
                    _currentView = value;
                    OnPropertyChanged(nameof(CurrentView));
                }
            }
        }

        private User _currentUser;
                public User CurrentUser
        {
            get => _currentUser;
            set
            {
                if (_currentUser != value)
                {
                    _currentUser = value;
                    OnPropertyChanged(nameof(CurrentUser));
                    OnPropertyChanged(nameof(VaiTro));

                    var roleName = _currentUser?.VaiTro?.TenVaiTro?.Trim() ?? "null";
                    IsGiaoVuVisible = string.Equals(roleName, "Giáo vụ", StringComparison.OrdinalIgnoreCase);
                    IsNotHocSinhVisible = _currentUser != null && !string.Equals(roleName, "Học sinh", StringComparison.OrdinalIgnoreCase);
                    IsNotGiaoVuVisible = _currentUser != null && !string.Equals(roleName, "Giáo vụ", StringComparison.OrdinalIgnoreCase);
                }
            }
        }

        private bool _isGiaoVuVisible;
        public bool IsGiaoVuVisible
        {
            get => _isGiaoVuVisible;
            set
            {
                _isGiaoVuVisible = value;
                OnPropertyChanged(nameof(IsGiaoVuVisible));
            }
        }

        private bool _isNotHocSinhVisible;
        public bool IsNotHocSinhVisible
        {
            get => _isNotHocSinhVisible;
            set
            {
                _isNotHocSinhVisible = value;
                OnPropertyChanged(nameof(IsNotHocSinhVisible));
            }
        }
        private bool _isNotGiaoVuVisible;
        public bool IsNotGiaoVuVisible
        {
            get => _isNotGiaoVuVisible;
            set
            {
                _isNotGiaoVuVisible = value;
                OnPropertyChanged(nameof(IsNotGiaoVuVisible));
            }
        }



        public bool Isloaded { get; set; } = false;
        public ICommand LoadedWindowCommand { get; set; }
        // Các commend điều hướng
        public ICommand ShowTrangChuCommand  { get; set; }
        public ICommand ShowTaiKhoanCaNhanCommand { get; set; }
        public ICommand ShowQuanLyTaiKhoanCommand { get; set; }
        public ICommand ShowThongTinHocSinhCommand { get; set; }
        public ICommand ShowThongTinGiaoVienCommand { get; set; }
        public ICommand ShowDiemHocSinhCommand { get; set; }
        public ICommand ShowTongKetMonCommand { get; set; }
        public ICommand ShowTongKetNamHocCommand { get; set; }
        public ICommand ShowQuyDinhCommand { get; set; }
        // Mọi thứ xử lý nằm trong này
        public MainViewModel()
        {
            // Gán view mặc định là Trang chủ
            CurrentView = new TrangChuViewModel(this); // Trang chủ là UserControl mặc định ví dụ

            // Lệnh điều hướng

            ShowTrangChuCommand = new RelayCommand<object>((p) => true, (p) =>
            {
                CurrentView = new TrangChuViewModel(this);
            });
            ShowQuanLyTaiKhoanCommand = new RelayCommand<object>((p) => true, (p) => CurrentView = new QuanLyTaiKhoanMainViewModel(this));
            ShowTaiKhoanCaNhanCommand = new RelayCommand<object>((p) => true, (p) => CurrentView = new QuanLyTaiKhoanCaNhanViewModel(this));
            ShowThongTinHocSinhCommand = new RelayCommand<object>((p) => true, (p) => CurrentView = new TraCuuHocSinhViewModel(this));
            ShowThongTinGiaoVienCommand = new RelayCommand<object>((p) => true, (p) => CurrentView = new TraCuuGiaoVienViewModel(this));   
            ShowDiemHocSinhCommand = new RelayCommand<object>((p) => true, (p) => CurrentView = new TraCuuDiemHocSinhViewModel(this));
            ShowTongKetMonCommand = new RelayCommand<object>((p) => true, (p) => CurrentView = new TongKetMonViewModel(this));
            ShowTongKetNamHocCommand = new RelayCommand<object>((p) => true, (p) => CurrentView = new TongKetNamHocViewModel(this));
            ShowQuyDinhCommand = new RelayCommand<object>((p) => true, (p) => CurrentView = new QuyDinhMainViewModel(this));
        }
        protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (!Equals(field, newValue))
            {
                field = newValue;
                OnPropertyChanged(propertyName);
                return true;
            }

            return false;
        }


    }
}

