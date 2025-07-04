using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using QuanLyHocSinh.Model.Entities;
using ClosedXML.Excel;
using System.IO;
using Microsoft.Win32;
using System.Windows;
using MaterialDesignThemes.Wpf;
using QuanLyHocSinh.View.Dialogs.MessageBox;

namespace QuanLyHocSinh.ViewModel.BaoCao
{
    public class TongKetMonViewModel : BaseViewModel
    {
        private MainViewModel _mainVM;

        #region Properties for filtering        
        // Danh sách tổng kết theo lớp (thay đổi từ TongKetMonItem thành TongKetLopItem)
        private ObservableCollection<TongKetLopItem> _danhSachTongKetLop = new();
        public ObservableCollection<TongKetLopItem> DanhSachTongKetLop
        {
            get => _danhSachTongKetLop;
            set { _danhSachTongKetLop = value; OnPropertyChanged(nameof(DanhSachTongKetLop)); }
        }

        private ObservableCollection<TongKetLopItem> _allTongKetLop = new();

        // Danh sách môn học
        private ObservableCollection<string> _danhSachMonHoc = new();
        public ObservableCollection<string> DanhSachMonHoc
        {
            get => _danhSachMonHoc;
            set { _danhSachMonHoc = value; OnPropertyChanged(nameof(DanhSachMonHoc)); }
        }

        // Danh sách học kỳ
        private ObservableCollection<string> _danhSachHocKy = new();
        public ObservableCollection<string> DanhSachHocKy
        {
            get => _danhSachHocKy;
            set { _danhSachHocKy = value; OnPropertyChanged(nameof(DanhSachHocKy)); }
        }

        // Selected filters - chỉ còn môn học
        private string _selectedMonHoc;
        public string SelectedMonHoc
        {
            get => _selectedMonHoc;
            set { _selectedMonHoc = value; OnPropertyChanged(nameof(SelectedMonHoc)); Filter(); }
        }
        #endregion

        #region Statistics Properties - cập nhật theo biểu mẫu mới
        // Thống kê tổng quan
        private int _tongSoLop;
        public int TongSoLop
        {
            get => _tongSoLop;
            set { _tongSoLop = value; OnPropertyChanged(nameof(TongSoLop)); }
        }

        private int _tongHocSinh;
        public int TongHocSinh
        {
            get => _tongHocSinh;
            set { _tongHocSinh = value; OnPropertyChanged(nameof(TongHocSinh)); }
        }

        private int _tongSoLuongDat;
        public int TongSoLuongDat
        {
            get => _tongSoLuongDat;
            set { _tongSoLuongDat = value; OnPropertyChanged(nameof(TongSoLuongDat)); }
        }

        private double _tiLeDatChung;
        public double TiLeDatChung
        {
            get => _tiLeDatChung;
            set { _tiLeDatChung = value; OnPropertyChanged(nameof(TiLeDatChung)); }
        }
        #endregion

        public ICommand FilterCommand { get; }
        public ICommand ExportExcelCommand { get; }

        public TongKetMonViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM;
            
            // Khởi tạo commands
            FilterCommand = new RelayCommand(Filter);
            ExportExcelCommand = new RelayCommand(ExportToExcel);

            // Load dữ liệu
            LoadData();
            
            // Mặc định chọn "Tất cả" đã được set trong field initialization
        }

        // Helper method để hiển thị thông báo
        private async Task ShowNotificationAsync(string title, string message)
        {
            try
            {
                await DialogHost.Show(new NotifyDialog(title, message), "RootDialog_Main");
            }
            catch
                {
                // Fallback về MessageBox nếu DialogHost không khả dụng
                MessageBox.Show(message, title, MessageBoxButton.OK, 
                    title.Contains("Lỗi") ? MessageBoxImage.Error : MessageBoxImage.Information);
            }
        }

        private async void LoadData()
        {
            try
            {
                // Load dữ liệu tổng kết theo lớp
                _allTongKetLop = new ObservableCollection<TongKetLopItem>(TongKetMonDAL.GetTongKetTheoLop());
                DanhSachTongKetLop = new ObservableCollection<TongKetLopItem>(_allTongKetLop);

                // Load dữ liệu filter
                LoadFilterData();

                // Tính toán thống kê
                CalculateStatistics();
            }
            catch (Exception ex)
            {
                await ShowNotificationAsync("Lỗi", $"❌ Lỗi khi tải dữ liệu: {ex.Message}");
            }
        }

        private void LoadFilterData()
        {
            // Load môn học
            var dsMonHoc = TongKetMonDAL.GetAllMonHoc().OrderBy(m => m).ToList();
            DanhSachMonHoc = new ObservableCollection<string>(dsMonHoc);
            
            // Đặt mặc định môn học đầu tiên
            if (DanhSachMonHoc.Count > 0)
            {
                SelectedMonHoc = DanhSachMonHoc[0];
            }
        }

        private async void Filter()
        {
            try
            {
                // Lấy dữ liệu đã lọc theo môn học đã chọn, học kỳ mặc định là 2
                var filteredData = TongKetMonDAL.GetTongKetTheoLop(SelectedMonHoc, 2);
                DanhSachTongKetLop = new ObservableCollection<TongKetLopItem>(filteredData);

                // Cập nhật thống kê
                CalculateStatistics();
            }
            catch (Exception ex)
            {
                await ShowNotificationAsync("Lỗi", $"❌ Lỗi khi lọc dữ liệu: {ex.Message}");
            }
        }

        private void CalculateStatistics()
        {
            var (TongSoLop, TongSoHocSinh, TongSoDat, TiLeDatChung) = 
                TongKetMonDAL.GetThongKeTongHopTheoLop(SelectedMonHoc, 2);

            this.TongSoLop = TongSoLop;
            this.TongHocSinh = TongSoHocSinh;
            this.TongSoLuongDat = TongSoDat;
            this.TiLeDatChung = TiLeDatChung;
        }

        private async void ExportToExcel()
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*",
                    FileName = $"TongKetMon_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("Tổng kết môn");

                        // Tiêu đề
                        worksheet.Cell(1, 1).Value = "BÁO CÁO TỔNG KẾT MÔN";
                        worksheet.Range("A1:F1").Merge();
                        worksheet.Cell(1, 1).Style.Font.Bold = true;
                        worksheet.Cell(1, 1).Style.Font.FontSize = 16;
                        worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        // Thông tin bộ lọc
                        worksheet.Cell(3, 1).Value = $"Môn học: {SelectedMonHoc}";
                        worksheet.Cell(4, 1).Value = $"Học kỳ: 2";

                        // Header
                        int row = 6;
                        worksheet.Cell(row, 1).Value = "STT";
                        worksheet.Cell(row, 2).Value = "Lớp";
                        worksheet.Cell(row, 3).Value = "Sỉ số";
                        worksheet.Cell(row, 4).Value = "Số lượng đạt";
                        worksheet.Cell(row, 5).Value = "Tỉ lệ đạt (%)";

                        // Định dạng header
                        var headerRange = worksheet.Range($"A{row}:E{row}");
                        headerRange.Style.Font.Bold = true;
                        headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
                        headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        // Dữ liệu
                        row++;
                        foreach (var item in DanhSachTongKetLop)
                        {
                            worksheet.Cell(row, 1).Value = item.STT;
                            worksheet.Cell(row, 2).Value = item.TenLop;
                            worksheet.Cell(row, 3).Value = item.SiSo;
                            worksheet.Cell(row, 4).Value = item.SoLuongDat;
                            worksheet.Cell(row, 5).Value = item.TiLeDat;
                            row++;
                        }

                        // Thống kê tổng
                        row += 2;
                        worksheet.Cell(row, 1).Value = "THỐNG KÊ TỔNG";
                        worksheet.Cell(row, 1).Style.Font.Bold = true;
                        row++;
                        worksheet.Cell(row, 1).Value = $"Tổng số lớp: {TongSoLop}";
                        row++;
                        worksheet.Cell(row, 1).Value = $"Tổng học sinh: {TongHocSinh}";
                        row++;
                        worksheet.Cell(row, 1).Value = $"Tổng số đạt: {TongSoLuongDat}";
                        row++;
                        worksheet.Cell(row, 1).Value = $"Tỉ lệ đạt chung: {TiLeDatChung:F2}%";

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