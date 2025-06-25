using DocumentFormat.OpenXml.Office2016.Drawing.Command;
using DocumentFormat.OpenXml.Spreadsheet;
using MaterialDesignThemes.Wpf;
using QuanLyHocSinh.Model.Entities;
using QuanLyHocSinh.Service;
using QuanLyHocSinh.View.Dialogs;
using QuanLyHocSinh.View.Dialogs.MessageBox;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace QuanLyHocSinh.ViewModel.QuanLyTaiKhoan
{
    public class QuanLyTaiKhoanThemViewModel : BaseViewModel
    {
        private string _userID = string.Empty;
        private string _hoTen = string.Empty;
        private string _tenDangNhap = string.Empty;
        private string _matKhau = string.Empty;
        private string _vaiTroID = string.Empty;
        private string _maHoSo;
        private string _maHoSoCaNhan;
        private string _lopDayID1;
        private string _lopDayID2;
        private string _lopDayID3;
        private string _lopDayIDCN;
        private string _gioiTinh;
        private string _boMon = string.Empty;
        private string _selectedTenChucVu;
        private string _diaChi = string.Empty;
        private DateTime _ngaySinh;
        private readonly MainViewModel _mainVM;
        private UserControl _currentControl;
        public ObservableCollection<MonHoc> DanhSachBoMon { get; set; } = new();
        public string BoMon { get; set; } // BoMonID


        public string VaiTroID
        {
            get => _vaiTroID;
            set
            {
                if (_vaiTroID != value)
                {
                    _vaiTroID = value;
                    OnPropertyChanged();

                    TenDangNhap = UserService.GetNextLoginUsername(_vaiTroID);
                    MaHoSoCaNhan = UserService.LayMaHoSoCaNhanMoi(_vaiTroID);

                    OnPropertyChanged(nameof(LaHocSinhHoacGiaoVien));
                    OnPropertyChanged(nameof(LaGiaoVu));
                    OnPropertyChanged(nameof(LaGiaoVienChuNhiem));
                    OnPropertyChanged(nameof(LaGiaoVienBoMon));
                    OnPropertyChanged(nameof(LaGiaoVien));
                    OnPropertyChanged(nameof(LaHocSinh));

                    var chucVuList = UserService.LayDanhSachTenChucVuTheoVaiTro(_vaiTroID);
                    DanhSachTenChucVu = new ObservableCollection<string>(chucVuList);
                    if (_vaiTroID == "VT03")
                    {
                        SelectedTenChucVu = DanhSachTenChucVu[0];
                    }
                    if (_vaiTroID == "VT02")
                    {
                        LoadBoMon(); // Tải danh sách bộ môn nếu là Giao Vụ
                    }
                    // Tự động cập nhật Email theo VaiTroID
                    switch (_vaiTroID)
                    {
                        case "VT01":
                            Email = $"{TenDangNhap}@student.com";
                            break;
                        case "VT02":
                            Email = $"{TenDangNhap}@teacher.com";
                            break;
                        case "VT03":
                            Email = $"{TenDangNhap}@admin.com";
                            break;
                        default:
                            Email = $"{TenDangNhap}@unknown.com";
                            break;
                    }

                }
            }
        }
        private string _email;
        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }
        public string BoMonID
        {
            get => _boMon;
            set
            {
                if (_boMon != value)
                {
                    _boMon = value;
                    OnPropertyChanged();
                }
            }
        }

        private ObservableCollection<Lop> _danhSachLop1;
        public ObservableCollection<Lop> DanhSachLop1
        {
            get => _danhSachLop1;
            set
            {
                _danhSachLop1 = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<Lop> _danhSachLop2;
        public ObservableCollection<Lop> DanhSachLop2
        {
            get => _danhSachLop2;
            set
            {
                _danhSachLop2 = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<Lop> _danhSachLop3;
        public ObservableCollection<Lop> DanhSachLop3
        {
            get => _danhSachLop3;
            set
            {
                _danhSachLop3 = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<Lop> _danhSachLopCN;
        public ObservableCollection<Lop> DanhSachLopCN
        {
            get => _danhSachLopCN;
            set
            {
                _danhSachLopCN = value;
                OnPropertyChanged();
            }
        }

        public string LopDayID1
        {
            get => _lopDayID1;
            set
            {
                if (_lopDayID1 != value)
                {
                    _lopDayID1 = value;
                    OnPropertyChanged();
                    CapNhatDanhSachLop();
                }
            }
        }

        public string LopDayID2
        {
            get => _lopDayID2;
            set
            {
                if (_lopDayID2 != value)
                {
                    _lopDayID2 = value;
                    OnPropertyChanged();
                    CapNhatDanhSachLop();
                }
            }
        }

        public string LopDayID3
        {
            get => _lopDayID3;
            set
            {
                if (_lopDayID3 != value)
                {
                    _lopDayID3 = value;
                    OnPropertyChanged();
                    CapNhatDanhSachLop();
                }
            }
        }

        public string LopDayIDCN
        {
            get => _lopDayIDCN;
            set
            {
                if (_lopDayIDCN != value)
                {
                    _lopDayIDCN = value;
                    OnPropertyChanged();
                    CapNhatDanhSachLop();
                }
            }
        }


        public bool LaHocSinhHoacGiaoVien => VaiTroID == "VT01" || VaiTroID == "VT02";
        public bool LaGiaoVu => VaiTroID == "VT03";
        public bool LaGiaoVienChuNhiem => VaiTroID == "VT02" && SelectedTenChucVu == "Giáo viên chủ nhiệm";
        public bool LaGiaoVienBoMon => VaiTroID == "VT02" && SelectedTenChucVu == "Giáo viên bộ môn";
        public bool LaGiaoVien => VaiTroID == "VT02";

        public bool LaHocSinh => VaiTroID == "VT01";
        public string SelectedTenChucVu
        {
            get => _selectedTenChucVu;
            set
            {
                if (_selectedTenChucVu != value)
                {
                    _selectedTenChucVu = value;
                    OnPropertyChanged();

                    OnPropertyChanged(nameof(LaGiaoVienBoMon));
                    OnPropertyChanged(nameof(LaGiaoVienChuNhiem));
                    LopDayID3 = null;
                    LopDayIDCN = null;
                }
            }
        }




        public string MaHoSo
        {
            get => _maHoSo;
            set { _maHoSo = value; OnPropertyChanged(); }
        }

        public string MaHoSoCaNhan
        {
            get => _maHoSoCaNhan;
            set { _maHoSoCaNhan = value; OnPropertyChanged(); }
        }

        public string UserID
        {
            get => _userID;
            set { _userID = value; OnPropertyChanged(); }
        }

        public string HoTen
        {
            get => _hoTen;
            set { _hoTen = value; OnPropertyChanged(); }
        }

        public string TenDangNhap
        {
            get => _tenDangNhap;
            set { _tenDangNhap = value; OnPropertyChanged(); }
        }

        public string MatKhau
        {
            get => _matKhau;
            set { _matKhau = value; OnPropertyChanged(); }
        }

        public string GioiTinh
        {
            get => _gioiTinh;
            set { _gioiTinh = value; OnPropertyChanged(); }
        }

        public string DiaChi
        {
            get => _diaChi;
            set { _diaChi = value; OnPropertyChanged(); }
        }

        public DateTime NgaySinh
        {
            get => _ngaySinh;
            set { _ngaySinh = value; OnPropertyChanged(); }
        }

        public UserControl CurrentControl
        {
            get => _currentControl;
            set { _currentControl = value; OnPropertyChanged(nameof(CurrentControl)); }
        }

        public ObservableCollection<VaiTro> Roles { get; } = new();
        public ObservableCollection<string> DanhSachGioiTinh { get; set; } = new ObservableCollection<string> { "Nam", "Nữ" };
        public ObservableCollection<Lop> DanhSachLop { get; set; }
        public string SelectedLopHocID { get; set; }

        public ObservableCollection<string> DanhSachTenChucVu { get; set; } = new();

        public ICommand AddAccountCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand QuayLaiCommand { get; }

        public event Action<User>? AccountAddedSuccessfully;
        public event Action? CancelRequested;
        private void CapNhatDanhSachLop()
        {
            var allLop = DanhSachLop ?? new ObservableCollection<Lop>();

            DanhSachLop1 = new ObservableCollection<Lop>(
                allLop.Where(l =>
                    l.LopID != LopDayID2 && l.LopID != LopDayID3 && l.LopID != LopDayIDCN
                )
            );

            DanhSachLop2 = new ObservableCollection<Lop>(
                allLop.Where(l =>
                    l.LopID != LopDayID1 && l.LopID != LopDayID3 && l.LopID != LopDayIDCN
                )
            );

            DanhSachLop3 = new ObservableCollection<Lop>(
                allLop.Where(l =>
                    l.LopID != LopDayID1 && l.LopID != LopDayID2 && l.LopID != LopDayIDCN
                )
            );

            DanhSachLopCN = new ObservableCollection<Lop>(
                allLop.Where(l =>
                    l.LopID != LopDayID1 && l.LopID != LopDayID2 && l.LopID != LopDayID3
                )
            );

            OnPropertyChanged(nameof(DanhSachLop1));
            OnPropertyChanged(nameof(DanhSachLop2));
            OnPropertyChanged(nameof(DanhSachLop3));
            OnPropertyChanged(nameof(DanhSachLopCN));
        }

        private async void LoadBoMon()
        {
            try
            {
                DanhSachBoMon.Clear();
                var boMons = UserService.LayDanhSachBoMon(); // Trả về List<MonHoc>
                foreach (var boMon in boMons)
                {
                    DanhSachBoMon.Add(boMon);
                }
            }
            catch (Exception ex)
            {
                await DialogHost.Show(new ErrorDialog("Lỗi", $"Lỗi khi tải danh sách bộ môn: {ex.Message}"), "RootDialog");
            }
        }
        public QuanLyTaiKhoanThemViewModel(MainViewModel? mainVM = null)
        {
            _mainVM = mainVM ?? throw new ArgumentNullException(nameof(mainVM));
            CurrentControl = new ThemTaiKhoanUC(this);
            LoadRoles();
            MaHoSo = UserService.LayHoSoIDMoi();
            DanhSachLop = new ObservableCollection<Lop>(UserService.LayDanhSachLopHoc());
            CapNhatDanhSachLop();

            UserID = UserService.LayUserIDMoi();
            MatKhau = "123456";
            MaHoSoCaNhan = UserService.LayMaHoSoCaNhanMoi(VaiTroID);

            NgaySinh = DateTime.Today.AddYears(0); // Mặc định 0 tuổi

            AddAccountCommand = new RelayCommand(ExecuteAddAccount, CanExecuteAddAccount);
            CancelCommand = new RelayCommand(ExecuteCancel);
            QuayLaiCommand = new RelayCommand(ExecuteQuayLai);
        }

        

        private async void LoadRoles()
        {
            try
            {
                Roles.Clear();
                var rolesList = UserService.GetAllRoles();
                foreach (var role in rolesList)
                {
                    Roles.Add(role);
                }

                if (Roles.Any())
                    VaiTroID = Roles.First().VaiTroID;
            }
            catch (Exception ex)
            {
                await DialogHost.Show(new NotifyDialog("Lỗi", $"❌ Lỗi khi tải danh sách vai trò: {ex.Message}"), "RootDialog_Add");
            }
        }

        public void ExecuteQuayLai()
        {
            CurrentControl = new ThemTaiKhoanUC(this);
        }
        public async void ExecuteAddAccount()
        {
            try
            {
                var tuoi = DateTime.Today.Year - NgaySinh.Year;
                if (NgaySinh > DateTime.Today.AddYears(-tuoi)) tuoi--;

                if (VaiTroID == "VT01") // Học sinh
                {
                    if (!string.IsNullOrEmpty(SelectedLopHocID) && tuoi < 15)
                    {
                        var tenLop = DanhSachLop?.FirstOrDefault(l => l.LopID == SelectedLopHocID)?.TenLop;

                        var _ = await DialogHost.Show(
                            new ConfirmDialog($"⚠️ Học sinh chỉ mới {tuoi} tuổi, quá nhỏ để học lớp {tenLop}. Bạn có muốn tiếp tục không?"),
                            "RootDialog_Add");

                        if (_?.ToString() == "False")
                            return;
                    }
                }
                else
                {
                    if (tuoi < 22)
                    {
                        if (VaiTroID == "VT02")
                        {
                            var _ = await DialogHost.Show(
                            new ConfirmDialog($"⚠️Giáo viên chưa đủ 22 tuổi. Bạn có muốn tiếp tục không?"),
                            "RootDialog_Add");
                            if (_?.ToString() == "False")
                                return;
                        }
                        else
                        {
                            var _ = await DialogHost.Show(
                            new ConfirmDialog($"⚠️Giáo vụ chưa đủ 22 tuổi. Bạn có muốn tiếp tục không?"),
                            "RootDialog_Add");
                            if (_?.ToString() == "False")
                                return;
                        }
                    }
                }



                // Tạo tài khoản mới
                var userMoi = new User
                {
                    UserID = UserID,
                    HoTen = HoTen,
                    TenDangNhap = TenDangNhap,
                    MatKhau = MatKhau,
                    VaiTroID = VaiTroID,
                    MaHoSo = MaHoSo,
                    MaHoSoCaNhan = MaHoSoCaNhan,
                    GioiTinh = GioiTinh,
                    DiaChi = DiaChi,
                    NgaySinh = NgaySinh,
                    ChucVu = SelectedTenChucVu,
                    Email = Email,
                    LopHocID = VaiTroID == "VT01" ? SelectedLopHocID : null,
                    HocSinhID = VaiTroID == "VT01" ? TenDangNhap : null,
                    LopDayID1 = VaiTroID == "VT02" ? LopDayID1 : null,
                    LopDayID2 = VaiTroID == "VT02" ? LopDayID2 : null,
                    LopDayID3 = VaiTroID == "VT02" ? LopDayID3 : null,
                    LopDayIDCN = VaiTroID == "VT02" ? LopDayIDCN : null,
                    GiaoVienID = VaiTroID == "VT02" ? TenDangNhap : null,
                    BoMon = VaiTroID == "VT02" ? BoMon : null,
                    GiaoVuID = VaiTroID == "VT03" ? TenDangNhap : null
                };

                bool result = UserService.ThemTaiKhoan(userMoi);

                if (!result)
                {
                    var err = UserService.LastErrorMessage;
                    await DialogHost.Show(new ErrorDialog("Thất bại", $"{err}"), "RootDialog_Add");
                    return;
                }

                await DialogHost.Show(new NotifyDialog("Thành công", " Đã thêm tài khoản!!"), "RootDialog_Add");

                AccountAddedSuccessfully?.Invoke(userMoi);

                // Reset form
                UserID = UserService.LayUserIDMoi();
                MaHoSo = UserService.LayHoSoIDMoi();
                MaHoSoCaNhan = UserService.LayMaHoSoCaNhanMoi(VaiTroID);
                TenDangNhap = UserService.GetNextLoginUsername(VaiTroID);
                MatKhau = "123456";
                Email = $"{TenDangNhap}@{(VaiTroID == "VT01" ? "student" : VaiTroID == "VT02" ? "teacher" : "admin")}.com";

                ClearForm();
            }
            catch (Exception ex)
            {
                await DialogHost.Show(new ErrorDialog("Lỗi", $"{ex.Message}"), "RootDialog_Add");
            }
        }



        public bool CanExecuteAddAccount()
        {
            // Các trường bắt buộc chung
            var requiredFields = new[]
            {
        UserID, HoTen, TenDangNhap, MatKhau, VaiTroID,
        GioiTinh, SelectedTenChucVu, MaHoSoCaNhan, MaHoSo
    };

            if (requiredFields.Any(string.IsNullOrWhiteSpace))
                return false;

            // Học sinh: phải chọn lớp học
            if (VaiTroID == "VT01" && string.IsNullOrWhiteSpace(SelectedLopHocID))
                return false;

            // Giáo viên: phải có BoMon + (ít nhất 1 lớp dạy hoặc là GVCN có lớp chủ nhiệm)
            if (VaiTroID == "VT02")
            {
                if (string.IsNullOrWhiteSpace(BoMon))
                    return false;

                bool isGVCN = SelectedTenChucVu == "Giáo viên chủ nhiệm";
                bool hasLopDay = !string.IsNullOrWhiteSpace(LopDayID1) ||
                                 !string.IsNullOrWhiteSpace(LopDayID2) ||
                                 !string.IsNullOrWhiteSpace(LopDayID3);

                if (!isGVCN && !hasLopDay)
                    return false;

                if (isGVCN && string.IsNullOrWhiteSpace(LopDayIDCN))
                    return false;
            }

            return true;
        }




        public void ExecuteCancel()
        {
            CancelRequested?.Invoke();
        }

        private void ClearForm()
        {
            HoTen = string.Empty;
            TenDangNhap = string.Empty;
            MatKhau = string.Empty;
            DiaChi = string.Empty;
            NgaySinh = DateTime.Today.AddYears(-15);
            VaiTroID = Roles.Any() ? Roles.First().VaiTroID : string.Empty;
        }

        public string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}
