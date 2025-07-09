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
using System.Windows; // Thêm namespace này để sử dụng Visibility
using System.Windows.Controls;
using System.Windows.Input; // Make sure this is included for CommandManager

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
        private DateTime? _ngaySinh; // Changed to nullable DateTime
        private readonly MainViewModel _mainVM;
        private UserControl _currentControl;

        public ObservableCollection<MonHoc> DanhSachBoMon { get; set; } = new();
        public string BoMon { get; set; } // BoMonID
        public QuyDinhTuoiEntities QuyDinhTuoiHocSinh { get; set; }
        public QuyDinhTuoiEntities QuyDinhTuoiGiaoVien { get; set; }
        public QuyDinhEntities QuyDinhEntities { get; set; }

        public DateTime? NgaySinh // Changed to nullable DateTime
        {
            get => _ngaySinh;
            set
            {
                _ngaySinh = value;
                OnPropertyChanged();
                KiemTraTuoi(); // Kiểm tra mỗi khi ngày sinh thay đổi
                CommandManager.InvalidateRequerySuggested(); // Notify command to re-evaluate CanExecute
            }
        }

        private string _canhBaoTuoi;

        public string CanhBaoTuoi
        {
            get => _canhBaoTuoi;
            set
            {
                _canhBaoTuoi = value;
                OnPropertyChanged();
                // Cập nhật Visibility mỗi khi CanhBaoTuoi thay đổi
                CanhBaoTuoiVisibility = string.IsNullOrWhiteSpace(_canhBaoTuoi) ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        private Visibility _canhBaoTuoiVisibility;

        // Thuộc tính mới để điều khiển Visibility của cảnh báo tuổi
        public Visibility CanhBaoTuoiVisibility
        {
            get => _canhBaoTuoiVisibility;
            set
            {
                _canhBaoTuoiVisibility = value;
                OnPropertyChanged();
            }
        }

        private void KiemTraTuoi()
        {
            if (!NgaySinh.HasValue)
            {
                CanhBaoTuoi = "Vui lòng chọn ngày sinh.";
                return;
            }

            var today = DateTime.Today;
            var ngaySinhValue = NgaySinh.Value;

            int tuoi = today.Year - ngaySinhValue.Year;
            if (ngaySinhValue > today.AddYears(-tuoi)) tuoi--;

            // Use the loaded age regulations
            int minTuoiHS = QuyDinhTuoiHocSinh?.TuoiToiThieu ?? 15;
            int maxTuoiHS = QuyDinhTuoiHocSinh?.TuoiToiDa ?? 20;
            int minTuoiGV = QuyDinhTuoiGiaoVien?.TuoiToiThieu ?? 22;
            int maxTuoiGV = QuyDinhTuoiGiaoVien?.TuoiToiDa ?? 60;

            if (VaiTroID == "VT01") // Học sinh
            {
                if (tuoi < minTuoiHS || tuoi > maxTuoiHS)
                {
                    CanhBaoTuoi = $"Tuổi học sinh chỉ từ {minTuoiHS} đến {maxTuoiHS} tuổi.";
                }
                else
                {
                    CanhBaoTuoi = "";
                }
            }
            else if (VaiTroID == "VT02" || VaiTroID == "VT03") // Giáo viên hoặc Giáo vụ
            {
                if (tuoi < minTuoiGV || tuoi > maxTuoiGV)
                {
                    CanhBaoTuoi = $"Tuổi {(VaiTroID == "VT02" ? "giáo viên" : "giáo vụ")} chỉ từ {minTuoiGV} đến {maxTuoiGV} tuổi.";
                }
                else
                {
                    CanhBaoTuoi = "";
                }
            }
            else
            {
                CanhBaoTuoi = ""; // No specific age restriction for other roles or if VaiTroID is not set
            }
        }

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
                        SelectedTenChucVu = DanhSachTenChucVu.FirstOrDefault(); // Use FirstOrDefault for safety
                    }
                    if (_vaiTroID == "VT02")
                    {
                        LoadBoMon(); // Tải danh sách bộ môn nếu là Giáo viên
                    }
                    else
                    {
                        DanhSachBoMon.Clear(); // Clear if not a teacher
                        BoMon = string.Empty;
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
                    KiemTraTuoi(); // Re-check age when role changes
                    CommandManager.InvalidateRequerySuggested(); // Notify command to re-evaluate CanExecute
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
                    CommandManager.InvalidateRequerySuggested();
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
                    CommandManager.InvalidateRequerySuggested();
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
                    CommandManager.InvalidateRequerySuggested();
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
                    CommandManager.InvalidateRequerySuggested();
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
                    CommandManager.InvalidateRequerySuggested();
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
                    CommandManager.InvalidateRequerySuggested();
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
                var boMons = UserService.LayDanhSachBoMon();
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

            // Initialize NgaySinh to a valid default value based on student rules, or null
            NgaySinh = DateTime.Today.AddYears(-15);

            // Initialize CanhBaoTuoiVisibility to Collapsed initially
            _canhBaoTuoiVisibility = Visibility.Collapsed;

            AddAccountCommand = new RelayCommand(ExecuteAddAccount, CanExecuteAddAccount);
            CancelCommand = new RelayCommand(ExecuteCancel);
            QuayLaiCommand = new RelayCommand(ExecuteQuayLai);

            QuyDinhTuoiHocSinh = QuyDinhTuoiDAL.GetQuyDinhTuoi("QDHS");
            QuyDinhTuoiGiaoVien = QuyDinhTuoiDAL.GetQuyDinhTuoi("QDGV");
            QuyDinhEntities = QuyDinhDAL.GetQuyDinh();
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
                {
                    VaiTroID = Roles.First().VaiTroID;
                }
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
                if (!CanExecuteAddAccount())
                {
                    await DialogHost.Show(new ErrorDialog("Lỗi", "Vui lòng điền đầy đủ và chính xác thông tin bắt buộc."), "RootDialog_Add");
                    return;
                }

                // Kiểm tra lại CanhBaoTuoi trước khi thêm
                if (!string.IsNullOrEmpty(CanhBaoTuoi))
                {
                    await DialogHost.Show(new ErrorDialog("Thất bại", CanhBaoTuoi), "RootDialog_Add");
                    return;
                }

                if (VaiTroID == "VT01") // Học sinh
                {
                    // Kiểm tra sĩ số lớp
                    int siSoHienTai = LopDAL.GetSiSo(SelectedLopHocID);
                    int siSoToiDa = QuyDinhEntities?.SiSoLop_ToiDa ?? 40; // Mặc định 40 nếu không có quy định

                    if (siSoHienTai >= siSoToiDa)
                    {
                        await DialogHost.Show(
                            new ErrorDialog("Thất bại", $"Lớp đã đạt sĩ số tối đa ({siSoToiDa} học sinh). Không thể thêm học sinh mới."),
                            "RootDialog_Add");
                        return;
                    }
                }

                // Kiểm tra trùng lặp lớp dạy cho giáo viên
                if (VaiTroID == "VT02")
                {
                    System.Collections.Generic.List<string?> danhSachLopDay = new System.Collections.Generic.List<string?> { LopDayID1, LopDayID2, LopDayID3, LopDayIDCN };
                    foreach (var lopID in danhSachLopDay.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct())
                    {
                        if (GiaoVienDAL.DaPhanCongDay(TenDangNhap, lopID, BoMon))
                        {
                            string? tenLop = LopDAL.LayTenLopTheoID(lopID);
                            string? tenMon = MonHocDAL.LayTenMonHocTheoID(BoMon);
                            await DialogHost.Show(
                                new ErrorDialog("Thất bại", $"Có giáo viên đã được phân công dạy môn {tenMon} tại lớp {tenLop} rồi."),
                                "RootDialog_Add");
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
                    MatKhau = MatKhau, // Consider hashing password here if not already hashed
                    VaiTroID = VaiTroID,
                    MaHoSo = MaHoSo,
                    MaHoSoCaNhan = MaHoSoCaNhan,
                    GioiTinh = GioiTinh,
                    DiaChi = DiaChi,
                    NgaySinh = NgaySinh.Value, // Use .Value after checking .HasValue
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
                ClearForm();
                UserID = UserService.LayUserIDMoi();
                MaHoSo = UserService.LayHoSoIDMoi();
                MaHoSoCaNhan = UserService.LayMaHoSoCaNhanMoi(VaiTroID);
                TenDangNhap = UserService.GetNextLoginUsername(VaiTroID);
                MatKhau = "123456";
                Email = $"{TenDangNhap}@{(VaiTroID == "VT01" ? "student" : VaiTroID == "VT02" ? "teacher" : "admin")}.com";
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

            // Validate date of birth first
            // Chỉ kiểm tra NgaySinh nếu nó là null, còn CanhBaoTuoi thì đã được cập nhật khi NgaySinh thay đổi
            if (NgaySinh == null || CanhBaoTuoiVisibility == Visibility.Visible)
            {
                return false;
            }

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
            MatKhau = "123456"; // Reset to default password
            DiaChi = string.Empty;
            NgaySinh = DateTime.Today.AddYears(-15); // Reset to a default that might trigger validation for the selected role
            SelectedLopHocID = null; // Clear selected class for students
            LopDayID1 = null;
            LopDayID2 = null;
            LopDayID3 = null;
            LopDayIDCN = null;
            SelectedTenChucVu = null; // Clear selected position
            BoMon = string.Empty; // Clear selected subject
        }

        public string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}