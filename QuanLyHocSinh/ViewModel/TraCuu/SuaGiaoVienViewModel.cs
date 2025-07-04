using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;
using QuanLyHocSinh.Model.Entities;
using QuanLyHocSinh.Service;
using QuanLyHocSinh.View.Controls.DanhSachLop;
using QuanLyHocSinh.View.Dialogs.MessageBox;
using QuanLyHocSinh.View.RoleControls;

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
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class SuaGiaoVienViewModel : INotifyPropertyChanged
    {
        // Properties
        private string _lopChuNhiem;

        public string LopChuNhiem
        {
            get => _lopChuNhiem;
            set
            {
                if (_lopChuNhiem != value)
                {
                    // 🔹 Lưu lớp chủ nhiệm cũ để bỏ tick
                    string lopCu = _lopChuNhiem;

                    _lopChuNhiem = value;
                    OnPropertyChanged();

                    // 🔹 Bỏ tick lớp chủ nhiệm cũ nếu có
                    if (!string.IsNullOrEmpty(lopCu))
                    {
                        var itemCu = DanhSachLopItems.FirstOrDefault(l => l.TenLop == lopCu);
                        if (itemCu != null)
                        {
                            itemCu.IsSelected = false;
                        }
                    }

                    // 🔹 Tick lớp chủ nhiệm mới
                    if (!string.IsNullOrEmpty(_lopChuNhiem))
                    {
                        var itemMoi = DanhSachLopItems.FirstOrDefault(l => l.TenLop == _lopChuNhiem);
                        if (itemMoi != null)
                        {
                            itemMoi.IsSelected = true;
                        }
                    }
                }
            }
        }

        private Visibility _isGVCNVisible = Visibility.Collapsed;
        public Visibility IsGVCNVisible
        {
            get => _isGVCNVisible;
            set { _isGVCNVisible = value; OnPropertyChanged(); }
        }

        private string _tenChucVu;

        public string TenChucVu
        {
            get => _tenChucVu;
            set
            {
                if (_tenChucVu != value)
                {
                    _tenChucVu = value;
                    OnPropertyChanged();

                    // ✅ Cập nhật hiển thị ComboBox lớp chủ nhiệm
                    IsGVCNVisible = _tenChucVu == "Giáo viên chủ nhiệm" ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }


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
        public string _maGV;
            public string MaGV
        {
            get => _maGV;
            set { _maGV = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> DanhSachGioiTinh { get; } =
            new ObservableCollection<string> { "Nam", "Nữ" };

        public ObservableCollection<string> DanhSachTenChucVu { get; set; } = new();
        public ObservableCollection<LopItem> DanhSachLopItems { get; set; } = new();
        public ObservableCollection<MonHocItem> DanhSachMonItems { get; set; } = new();
        public ObservableCollection<string> DanhSachLop { get; set; } = new();

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
            TenChucVu = giaoVien.ChucVu;
            LopChuNhiem = giaoVien.LopChuNhiemID;
            MaGV = giaoVien.MaGV;

            DanhSachTenChucVu = new ObservableCollection<string>(
                UserService.LayDanhSachTenChucVuTheoVaiTro(giaoVien.VaiTroID));

            var tatCaLop = GiaoVienDAL.GetAllLop();
            var lopDuocChon = new List<string>();
            if (!string.IsNullOrEmpty(giaoVien.LopDayID) && giaoVien.LopDayID != "Chưa phân công")
            {
                lopDuocChon = giaoVien.LopDayID.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries)
                                               .Select(l => l.Trim()).ToList();
            }

            foreach (var lop in tatCaLop)
            {
                DanhSachLop.Add(lop);
                DanhSachLopItems.Add(new LopItem
                {
                    TenLop = lop,
                    IsSelected = lopDuocChon.Contains(lop)
                });
            }

            _ = InitializeMonHocItemsAsync(giaoVien.BoMon);

            SaveCommandGV = new RelayCommand(Save);
            CancelCommandGV = new RelayCommand(Cancel);
        }

        private async Task InitializeMonHocItemsAsync(string currentBoMon)
        {
            try
            {
                var monDaDuocChon = !string.IsNullOrEmpty(currentBoMon) && currentBoMon != "Chưa phân công"
                    ? currentBoMon.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries).Select(m => m.Trim()).ToList()
                    : new List<string>();

                var monHocDAL = new MonHocDAL();
                var tatCaMonHoc = await monHocDAL.GetAllMonHocAsync();

                DanhSachMonItems.Clear();
                foreach (var mon in tatCaMonHoc)
                {
                    DanhSachMonItems.Add(new MonHocItem
                    {
                        TenMonHoc = mon.TenMonHoc,
                        IsSelected = monDaDuocChon.Contains(mon.TenMonHoc)
                    });
                }
            }
            catch (Exception ex)
            {
                await ShowNotificationAsync("Lỗi", $"❌ Lỗi khi khởi tạo môn học: {ex.Message}");
            }
        }
        private async void Save()
        {
            try
            {
                // Kiểm tra các trường bắt buộc
                if (string.IsNullOrWhiteSpace(HoTen) || string.IsNullOrWhiteSpace(GioiTinh) ||
                    string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(DiaChi))
                {
                    await ShowError("Cảnh báo", "⚠️ Vui lòng nhập đầy đủ thông tin!");
                    return;
                }

                // Kiểm tra tuổi
                var tuoi = DateTime.Today.Year - NgaySinh.Year;
                if (NgaySinh > DateTime.Today.AddYears(-tuoi)) tuoi--;

                if (tuoi < 22)
                {
                    var confirmResult = await DialogHost.Show(
                        new ConfirmDialog($"⚠️ Giáo viên chưa đủ 22 tuổi. Bạn có muốn tiếp tục không?"),
                        "RootDialog_Main");

                    if (confirmResult?.ToString() == "False")
                        return;
                }

                // Lấy danh sách lớp và môn được chọn
                var lopDuocChon = DanhSachLopItems.Where(l => l.IsSelected).Select(l => l.TenLop).ToList();
                var monDuocChon = DanhSachMonItems.Where(m => m.IsSelected).Select(m => m.TenMonHoc).ToList();

                // Kiểm tra giới hạn lớp và môn
                if (lopDuocChon.Count > 3)
                {
                    await ShowError("Cảnh báo", "⚠️ Mỗi giáo viên chỉ được dạy tối đa 3 lớp!");
                    return;
                }

                if (monDuocChon.Count > 2)
                {
                    await ShowError("Cảnh báo", "⚠️ Mỗi giáo viên chỉ được dạy tối đa 2 môn học!");
                    return;
                }

                // Cập nhật thông tin chung
                _giaoVienGoc.HoTen = HoTen;
                _giaoVienGoc.GioiTinh = GioiTinh;
                _giaoVienGoc.NgaySinh = NgaySinh;
                _giaoVienGoc.Email = Email;
                _giaoVienGoc.DiaChi = DiaChi;
                _giaoVienGoc.MaGV = MaGV;
                _giaoVienGoc.ChucVu = TenChucVu;

                // Gán lớp chủ nhiệm (nếu là GVCN)
                if (TenChucVu == "Giáo viên chủ nhiệm")
                {
                    if (!string.IsNullOrWhiteSpace(LopChuNhiem))
                    {
                        string? loi;
                        bool thanhCong = GiaoVienDAL.CapNhatLopChuNhiem(_giaoVienGoc.MaGV, LopChuNhiem, out loi);

                        if (!thanhCong)
                        {
                            await ShowError("Lỗi", $"⚠️ {loi}");
                            return;
                        }
                        //// Nếu gán lớp chủ nhiệm mới thành công, mới xóa lớp chủ nhiệm cũ (nếu khác lớp mới)
                        //if (_giaoVienGoc.LopChuNhiemID != null && _giaoVienGoc.LopChuNhiemID != LopChuNhiem)
                        //{
                        //    GiaoVienDAL.ClearLopChuNhiem(_giaoVienGoc.MaGV);
                        //}

                        _giaoVienGoc.LopChuNhiemID = LopChuNhiem;
                    }
                    else
                    {
                        _giaoVienGoc.LopChuNhiemID = null;
                    }
                }
                else
                {
                    // Nếu không phải GVCN thì xóa thông tin lớp chủ nhiệm
                    GiaoVienDAL.ClearLopChuNhiem(_giaoVienGoc.MaGV);
                    _giaoVienGoc.LopChuNhiemID = null;
                }

                // Cập nhật thông tin giáo viên
                GiaoVienDAL.UpdateGiaoVien(_giaoVienGoc);
                string message;
                // Cập nhật phân công dạy
                bool thanhcong = GiaoVienDAL.UpdatePhanCongDay(_giaoVienGoc.MaGV, lopDuocChon, monDuocChon, out message);
                if (thanhcong)
                {
                    await ShowNotificationAsync("Thành công", "✅ Cập nhật thông tin thành công!");
                    CloseDialog?.Invoke(true);
                }
                else
                {
                    await ShowError("Lỗi", $"{message}");
                }

            }
            catch (Exception ex)
            {
                await ShowError("Lỗi", $"❌ Lỗi khi lưu: {ex.Message}");
            }
        }




        private void Cancel() => CloseDialog?.Invoke(false);

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private async Task ShowNotificationAsync(string title, string message)
        {
            try
            {
                await DialogHost.Show(new NotifyDialog(title, message), "RootDialog_SuaGV");
            }
            catch
            {
                MessageBox.Show(message, title, MessageBoxButton.OK,
                    title.Contains("Lỗi") ? MessageBoxImage.Error : MessageBoxImage.Information);
            }
        }
        private async Task ShowError(string title, string message)
        {
            try
            {
                await DialogHost.Show(new ErrorDialog(title, message), "RootDialog_SuaGV");
            }
            catch
            {
                MessageBox.Show(message, title, MessageBoxButton.OK,
                    title.Contains("Lỗi") ? MessageBoxImage.Error : MessageBoxImage.Information);
            }
        }
    }
}
