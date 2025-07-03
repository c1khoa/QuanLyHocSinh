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
                OnHocKyChanged();
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

        public TongKetMonDetailDialogViewModel(string tenLop, string monHoc, int? hocKy, string namHoc)
        {
            // Khởi tạo commands
            ExportExcelCommand = new RelayCommand(ExportToExcel);

            // Khởi tạo thông tin
            TenLop = tenLop;
            MonHoc = monHoc;
            NamHoc = namHoc;
            HocKy = hocKy ?? 0; // Nếu null thì hiển thị 0 để biết là "Tất cả"

            // Khởi tạo danh sách học kỳ
            DanhSachHocKy = new ObservableCollection<string> { "Tất cả", "1", "2" };
            
            // Thiết lập học kỳ được chọn
            if (hocKy == null || hocKy == 0)
                SelectedHocKy = "Tất cả";
            else
                SelectedHocKy = hocKy.ToString();

            // Load dữ liệu học sinh
            LoadDanhSachHocSinh(hocKy);
        }

        // Constructor cũ để tương thích ngược (nếu cần)
        public TongKetMonDetailDialogViewModel(TongKetLopItem lopItem)
        {
            // Khởi tạo commands
            ExportExcelCommand = new RelayCommand(ExportToExcel);

            // Khởi tạo thông tin từ item được chọn
            TenLop = lopItem.TenLop;
            MonHoc = lopItem.MonHoc;
            NamHoc = lopItem.NamHoc;
            HocKy = lopItem.HocKy;

            // Khởi tạo danh sách học kỳ
            DanhSachHocKy = new ObservableCollection<string> { "Tất cả", "1", "2" };
            
            // Thiết lập học kỳ được chọn
            if (lopItem.HocKy == 0)
                SelectedHocKy = "Tất cả";
            else
                SelectedHocKy = lopItem.HocKy.ToString();

            // Load dữ liệu học sinh
            LoadDanhSachHocSinh(lopItem.HocKy);
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

        private async void LoadDanhSachHocSinh(int? hocKy)
        {
            try
            {
                // Lấy danh sách học sinh trong lớp
                var danhSach = TongKetMonDAL.GetHocSinhTrongLop(TenLop, MonHoc, hocKy, NamHoc);
                DanhSachHocSinh = new ObservableCollection<HocSinhChiTietItem>(danhSach);

                // Tính thống kê
                CalculateStatistics();
            }
            catch (Exception ex)
            {
                await ShowNotificationAsync("Lỗi", $"❌ Lỗi khi tải dữ liệu học sinh: {ex.Message}");
            }
        }

        private void CalculateStatistics()
        {
            TongSoHocSinh = DanhSachHocSinh.Count;
            SoLuongDat = DanhSachHocSinh.Count(hs => hs.DaDat);
            TiLeDat = TongSoHocSinh > 0 ? Math.Round((SoLuongDat * 100.0) / TongSoHocSinh, 2) : 0;
        }

        private void OnHocKyChanged()
        {
            if (string.IsNullOrEmpty(SelectedHocKy)) return;

            int? hocKy = null;
            if (SelectedHocKy != "Tất cả")
            {
                if (int.TryParse(SelectedHocKy, out int parsed))
                    hocKy = parsed;
            }

            HocKy = hocKy ?? 0;
            LoadDanhSachHocSinh(hocKy);
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

                    await ShowNotificationAsync("Thành công", "✅ Xuất Excel thành công!");
                }
            }
            catch (Exception ex)
            {
                await ShowNotificationAsync("Lỗi", $"❌ Lỗi khi xuất Excel: {ex.Message}");
            }
        }
    }
}
