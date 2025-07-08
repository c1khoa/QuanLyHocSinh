using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using QuanLyHocSinh.Model.Entities;
using MaterialDesignThemes.Wpf;
using QuanLyHocSinh.View.Dialogs.MessageBox;
using System.Windows;
using System.Windows.Input;
using QuanLyHocSinh.ViewModel.TraCuu;
using ClosedXML.Excel;
using Microsoft.Win32;
using System.Configuration;

namespace QuanLyHocSinh.ViewModel.BaoCao
{
    public class TongKetMonDetailDialogViewModel : BaseViewModel
    {
        // Thông tin lớp và môn học
        private string _tenLop = "";
        public string TenLop
        {
            get => _tenLop;
            set { _tenLop = value; OnPropertyChanged(nameof(TenLop)); }
        }

        private string _monHoc = "";
        public string MonHoc
        {
            get => _monHoc;
            set { _monHoc = value; OnPropertyChanged(nameof(MonHoc)); }
        }

        private string _namHoc = "";
        public string NamHoc
        {
            get => _namHoc;
            set { _namHoc = value; OnPropertyChanged(nameof(NamHoc)); }
        }

        private int _hocKy;
        public int HocKy
        {
            get => _hocKy;
            set { _hocKy = value; OnPropertyChanged(nameof(HocKy)); }
        }

        // Danh sách môn học
        private ObservableCollection<string> _danhSachMonHoc;
        public ObservableCollection<string> DanhSachMonHoc
        {
            get => _danhSachMonHoc;
            set { _danhSachMonHoc = value; OnPropertyChanged(nameof(DanhSachMonHoc)); }
        }

        private string _selectedMonHoc;
        public string SelectedMonHoc
        {
            get => _selectedMonHoc;
            set { 
                _selectedMonHoc = value; 
                OnPropertyChanged(nameof(SelectedMonHoc)); 
                OnFilterChanged();
            }
        }

        // Danh sách năm học
        private ObservableCollection<string> _danhSachNamHoc;
        public ObservableCollection<string> DanhSachNamHoc
        {
            get => _danhSachNamHoc;
            set { _danhSachNamHoc = value; OnPropertyChanged(nameof(DanhSachNamHoc)); }
        }

        private string _selectedNamHoc;
        public string SelectedNamHoc
        {
            get => _selectedNamHoc;
            set { 
                _selectedNamHoc = value; 
                OnPropertyChanged(nameof(SelectedNamHoc)); 
                OnFilterChanged();
            }
        }

        // Danh sách học kỳ
        private ObservableCollection<string> _danhSachHocKy;
        public ObservableCollection<string> DanhSachHocKy
        {
            get => _danhSachHocKy;
            set { _danhSachHocKy = value; OnPropertyChanged(nameof(DanhSachHocKy)); }
        }

        private string _selectedHocKy;
        public string SelectedHocKy
        {
            get => _selectedHocKy;
            set { 
                _selectedHocKy = value; 
                OnPropertyChanged(nameof(SelectedHocKy)); 
                OnFilterChanged();
            }
        }

        // Danh sách học sinh
        private ObservableCollection<HocSinhChiTietItem> _danhSachHocSinh = new();
        public ObservableCollection<HocSinhChiTietItem> DanhSachHocSinh
        {
            get => _danhSachHocSinh;
            set { _danhSachHocSinh = value; OnPropertyChanged(nameof(DanhSachHocSinh)); }
        }

        // Thống kê
        private int _tongSoHocSinh;
        public int TongSoHocSinh
        {
            get => _tongSoHocSinh;
            set { _tongSoHocSinh = value; OnPropertyChanged(nameof(TongSoHocSinh)); }
        }

        private int _soLuongDat;
        public int SoLuongDat
        {
            get => _soLuongDat;
            set { _soLuongDat = value; OnPropertyChanged(nameof(SoLuongDat)); }
        }

        private double _tiLeDat;
        public double TiLeDat
        {
            get => _tiLeDat;
            set { _tiLeDat = value; OnPropertyChanged(nameof(TiLeDat)); }
        }

        // Commands
        public ICommand ExportExcelCommand { get; }

        public TongKetMonDetailDialogViewModel(string tenLop, string monHoc, ObservableCollection<string> danhSachMonHoc, int? hocKy, string namHoc)
        {
            // Khởi tạo commands
            ExportExcelCommand = new RelayCommand(ExportToExcel);

            // Khởi tạo thông tin
            TenLop = tenLop;
            DanhSachMonHoc = danhSachMonHoc;

            // Load dữ liệu filter
            LoadFilterData();
            
            // Đặt mặc định
            SelectedMonHoc = !string.IsNullOrEmpty(monHoc) ? monHoc : 
                            (DanhSachMonHoc?.FirstOrDefault() ?? "");
            SelectedNamHoc = !string.IsNullOrEmpty(namHoc) ? namHoc : 
                            (DanhSachNamHoc?.FirstOrDefault() ?? "");
            SelectedHocKy = "2"; // Mặc định học kỳ 2

            HocKy = 2;

            // Load dữ liệu học sinh
            LoadDanhSachHocSinh();
        }

        // Constructor cũ để tương thích ngược (nếu cần)
        public TongKetMonDetailDialogViewModel(TongKetLopItem lopItem)
        {
            // Khởi tạo commands
            ExportExcelCommand = new RelayCommand(ExportToExcel);

            // Khởi tạo thông tin từ item được chọn
            TenLop = lopItem.TenLop;
            
            // Load dữ liệu filter
            LoadFilterData();
            
            // Đặt mặc định 
            SelectedMonHoc = !string.IsNullOrEmpty(lopItem.MonHoc) ? lopItem.MonHoc : 
                            (DanhSachMonHoc?.FirstOrDefault() ?? "");
            SelectedNamHoc = !string.IsNullOrEmpty(lopItem.NamHoc) ? lopItem.NamHoc : 
                            (DanhSachNamHoc?.FirstOrDefault() ?? "");
            SelectedHocKy = "2"; // Mặc định học kỳ 2
            
            HocKy = 2;

            // Load dữ liệu học sinh
            LoadDanhSachHocSinh();
        }

        private void LoadFilterData()
        {
            try
            {
                
                // Load danh sách năm học
                var dsNamHoc = TongKetNamDAL.GetAllNamHoc().OrderByDescending(n => n).ToList();
                DanhSachNamHoc = new ObservableCollection<string>(dsNamHoc);
                
                // Load danh sách học kỳ (loại bỏ option "Tất cả")
                DanhSachHocKy = new ObservableCollection<string> { "1", "2" };
            }
            catch (Exception ex)
            {
                // Fallback nếu có lỗi
                DanhSachMonHoc = new ObservableCollection<string>();
                DanhSachNamHoc = new ObservableCollection<string>();
                DanhSachHocKy = new ObservableCollection<string> { "1", "2" };
            }
        }

        // Helper method để hiển thị thông báo
        private async Task ShowNotificationAsync(string title, string message)
        {
            try
            {
                await DialogHost.Show(new NotifyDialog(title, message), "DetailDialog");
            }
            catch
            {
                // Fallback về MessageBox nếu DialogHost không khả dụng
                MessageBox.Show(message, title, MessageBoxButton.OK, 
                    title.Contains("Lỗi") ? MessageBoxImage.Error : MessageBoxImage.Information);
            }
        }

        private async void LoadDanhSachHocSinh()
        {
            try
            {
                if (string.IsNullOrEmpty(SelectedNamHoc) || string.IsNullOrEmpty(SelectedHocKy) || string.IsNullOrEmpty(SelectedMonHoc))
                    return;

                int hocKy = int.Parse(SelectedHocKy);
                
                // Lấy danh sách học sinh trong lớp
                var danhSach = TongKetMonDAL.GetHocSinhTrongLop(TenLop, SelectedMonHoc, hocKy, SelectedNamHoc);
                DanhSachHocSinh = new ObservableCollection<HocSinhChiTietItem>(danhSach);

                // Cập nhật các giá trị hiển thị
                MonHoc = SelectedMonHoc;
                NamHoc = SelectedNamHoc;
                HocKy = hocKy;

                // Tính thống kê
                CalculateStatistics();
            }
            catch (Exception ex)
            {
                await ShowNotificationAsync("Lỗi", $"❌ Lỗi khi tải dữ liệu học sinh: {ex.Message}");
            }
        }
        private async void CalculateStatistics()
        {

            TongSoHocSinh = DanhSachHocSinh.Count;
            SoLuongDat = DanhSachHocSinh.Count(hs => hs.DaDat);
            TiLeDat = TongSoHocSinh > 0 ? Math.Round((SoLuongDat * 100.0) / TongSoHocSinh, 2) : 0;
        }

        private void OnFilterChanged()
        {
            LoadDanhSachHocSinh();
        }

        private async void ExportToExcel()
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*",
                    FileName = $"ChiTietHocSinh_{TenLop}_{MonHoc}_{NamHoc}_HK{HocKy}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("Chi tiết học sinh");

                        // Tiêu đề
                        worksheet.Cell(1, 1).Value = "CHI TIẾT HỌC SINH LỚP";
                        worksheet.Range("A1:F1").Merge();
                        worksheet.Cell(1, 1).Style.Font.Bold = true;
                        worksheet.Cell(1, 1).Style.Font.FontSize = 16;
                        worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        // Thông tin lớp
                        worksheet.Cell(3, 1).Value = $"Năm học: {NamHoc}";
                        worksheet.Cell(3, 3).Value = $"Lớp: {TenLop}";
                        worksheet.Cell(4, 1).Value = $"Môn học: {MonHoc}";
                        worksheet.Cell(4, 3).Value = $"Học kỳ: {HocKy}";

                        // Header
                        int row = 6;
                        worksheet.Cell(row, 1).Value = "STT";
                        worksheet.Cell(row, 2).Value = "Họ tên";
                        worksheet.Cell(row, 3).Value = "Điểm 15 phút";
                        worksheet.Cell(row, 4).Value = "Điểm 1 tiết";
                        worksheet.Cell(row, 5).Value = "Điểm trung bình";
                        worksheet.Cell(row, 6).Value = "Xếp loại";

                        // Định dạng header
                        var headerRange = worksheet.Range($"A{row}:F{row}");
                        headerRange.Style.Font.Bold = true;
                        headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
                        headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        // Dữ liệu
                        row++;
                        foreach (var hs in DanhSachHocSinh)
                        {
                            worksheet.Cell(row, 1).Value = hs.STT;
                            worksheet.Cell(row, 2).Value = hs.HoTen;
                            worksheet.Cell(row, 3).Value = hs.Diem15Phut;
                            worksheet.Cell(row, 4).Value = hs.Diem1Tiet;
                            worksheet.Cell(row, 5).Value = hs.DiemTrungBinh;
                            worksheet.Cell(row, 6).Value = hs.XepLoai;
                            row++;
                        }

                        // Thống kê
                        row += 2;
                        worksheet.Cell(row, 1).Value = "THỐNG KÊ";
                        worksheet.Cell(row, 1).Style.Font.Bold = true;
                        row++;
                        worksheet.Cell(row, 1).Value = $"Tổng số học sinh: {TongSoHocSinh}";
                        row++;
                        worksheet.Cell(row, 1).Value = $"Số lượng đạt: {SoLuongDat}";
                        row++;
                        worksheet.Cell(row, 1).Value = $"Tỉ lệ đạt: {TiLeDat:F2}%";

                        // Auto fit columns
                        worksheet.ColumnsUsed().AdjustToContents();

                        workbook.SaveAs(saveFileDialog.FileName);
                    }

                    await ShowNotificationAsync("Thông báo", "✅ Xuất Excel thành công!");
                }
            }
            catch (Exception ex)
            {
                await ShowNotificationAsync("Lỗi", $"❌ Lỗi khi xuất Excel: {ex.Message}");
            }
        }
    }
}
