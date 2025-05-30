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

namespace QuanLyHocSinh.ViewModel.BaoCao
{
    public class TongKetMonViewModel : BaseViewModel
    {
        private MainViewModel _mainVM;

        #region Properties for filtering        // Danh sách tổng kết môn
        private ObservableCollection<TongKetMonItem> _danhSachTongKetMon = new();
        public ObservableCollection<TongKetMonItem> DanhSachTongKetMon
        {
            get => _danhSachTongKetMon;
            set { _danhSachTongKetMon = value; OnPropertyChanged(nameof(DanhSachTongKetMon)); }
        }

        private ObservableCollection<TongKetMonItem> _allTongKetMon = new();

        // Danh sách năm học
        private ObservableCollection<string> _danhSachNamHoc = new();
        public ObservableCollection<string> DanhSachNamHoc
        {
            get => _danhSachNamHoc;
            set { _danhSachNamHoc = value; OnPropertyChanged(nameof(DanhSachNamHoc)); }
        }

        // Danh sách lớp
        private ObservableCollection<string> _danhSachLop = new();
        public ObservableCollection<string> DanhSachLop
        {
            get => _danhSachLop;
            set { _danhSachLop = value; OnPropertyChanged(nameof(DanhSachLop)); }
        }

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

        // Selected filters
        private string _selectedNamHoc = "Tất cả";
        public string SelectedNamHoc
        {
            get => _selectedNamHoc;
            set { _selectedNamHoc = value; OnPropertyChanged(nameof(SelectedNamHoc)); Filter(); }
        }

        private string _selectedLop = "Tất cả";
        public string SelectedLop
        {
            get => _selectedLop;
            set { _selectedLop = value; OnPropertyChanged(nameof(SelectedLop)); Filter(); }
        }

        private string _selectedMonHoc = "Tất cả";
        public string SelectedMonHoc
        {
            get => _selectedMonHoc;
            set { _selectedMonHoc = value; OnPropertyChanged(nameof(SelectedMonHoc)); Filter(); }
        }

        private string _selectedHocKy = "Tất cả";
        public string SelectedHocKy
        {
            get => _selectedHocKy;
            set { _selectedHocKy = value; OnPropertyChanged(nameof(SelectedHocKy)); Filter(); }
        }
        #endregion

        #region Statistics Properties
        // Thống kê tổng quan
        private int _tongHocSinh;
        public int TongHocSinh
        {
            get => _tongHocSinh;
            set { _tongHocSinh = value; OnPropertyChanged(nameof(TongHocSinh)); }
        }

        private int _soLuongDat;
        public int SoLuongDat
        {
            get => _soLuongDat;
            set { _soLuongDat = value; OnPropertyChanged(nameof(SoLuongDat)); }
        }

        private int _soLuongKhongDat;
        public int SoLuongKhongDat
        {
            get => _soLuongKhongDat;
            set { _soLuongKhongDat = value; OnPropertyChanged(nameof(SoLuongKhongDat)); }
        }

        private double _tiLeDat;
        public double TiLeDat
        {
            get => _tiLeDat;
            set { _tiLeDat = value; OnPropertyChanged(nameof(TiLeDat)); }
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

        private void LoadData()
        {
            try
            {
                //Tải tất cả dữ liệu tổng kết môn
                _allTongKetMon = new ObservableCollection<TongKetMonItem>(TongKetMonDAL.GetAllTongKetMon());
                DanhSachTongKetMon = new ObservableCollection<TongKetMonItem>(_allTongKetMon);

                //Tải dữ liệu lọc
                LoadFilterData();

                //Tính toán thống kê
                CalculateStatistics();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void LoadFilterData()
        {
            // Load năm học
            var dsNamHoc = TongKetMonDAL.GetAllNamHoc().OrderBy(n => n).ToList();
            dsNamHoc.Insert(0, "Tất cả");
            DanhSachNamHoc = new ObservableCollection<string>(dsNamHoc);

            // Load lớp
            var dsLop = TongKetMonDAL.GetAllLop().OrderBy(l => l).ToList();
            dsLop.Insert(0, "Tất cả");
            DanhSachLop = new ObservableCollection<string>(dsLop);

            // Load môn học
            var dsMonHoc = TongKetMonDAL.GetAllMonHoc().OrderBy(m => m).ToList();
            dsMonHoc.Insert(0, "Tất cả");
            DanhSachMonHoc = new ObservableCollection<string>(dsMonHoc);

            // Load học kỳ
            var dsHocKy = TongKetMonDAL.GetAllHocKy().Select(h => h.ToString()).OrderBy(h => h).ToList();
            dsHocKy.Insert(0, "Tất cả");
            DanhSachHocKy = new ObservableCollection<string>(dsHocKy);
        }

        private void Filter()
        {
            try
            {
                var filtered = _allTongKetMon.Where(item =>
                    (SelectedNamHoc == "Tất cả" || string.IsNullOrEmpty(SelectedNamHoc) || item.NamHoc == SelectedNamHoc) &&
                    (SelectedLop == "Tất cả" || string.IsNullOrEmpty(SelectedLop) || item.TenLop == SelectedLop) &&
                    (SelectedMonHoc == "Tất cả" || string.IsNullOrEmpty(SelectedMonHoc) || item.MonHoc == SelectedMonHoc) &&
                    (SelectedHocKy == "Tất cả" || string.IsNullOrEmpty(SelectedHocKy) || item.HocKy.ToString() == SelectedHocKy)
                ).ToList();

                // Cập nhật STT và GhiChu
                for (int i = 0; i < filtered.Count; i++)
                {
                    filtered[i].STT = i + 1;
                    filtered[i].GhiChu = filtered[i].DiemTrungBinh >= 5 ? "Đạt" : "Không đạt";
                }

                DanhSachTongKetMon = new ObservableCollection<TongKetMonItem>(filtered);
                CalculateStatistics();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Lỗi khi lọc dữ liệu: {ex.Message}", "Lỗi", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        //Tính toán thống kê
        private void CalculateStatistics()
        {
            if (DanhSachTongKetMon == null || DanhSachTongKetMon.Count == 0)
            {
                TongHocSinh = 0;
                SoLuongDat = 0;
                SoLuongKhongDat = 0;
                TiLeDat = 0;
                return;
            }

            TongHocSinh = DanhSachTongKetMon.Count;
            SoLuongDat = DanhSachTongKetMon.Count(item => item.GhiChu == "Đạt");
            SoLuongKhongDat = TongHocSinh - SoLuongDat;
            TiLeDat = TongHocSinh > 0 ? (double)SoLuongDat / TongHocSinh * 100 : 0;
        }

        //Xuất Excel
        private void ExportToExcel()
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel files (*.xlsx)|*.xlsx",
                    FileName = $"BaoCaoTongKetMon_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("Tổng kết môn");

                        //Vẽ tiêu đề
                        worksheet.Cell("A1").Value = "BÁO CÁO TỔNG KẾT MÔN";
                        worksheet.Range("A1:N1").Merge();
                        worksheet.Cell("A1").Style.Font.Bold = true;
                        worksheet.Cell("A1").Style.Font.FontSize = 14;
                        worksheet.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        //Tạo tiêu đề
                        var headers = new[] { "STT", "Mã HS", "Họ tên", "Lớp", "Môn học", "Năm học", "Học kỳ", "Điểm miệng", "Điểm 15p", "Điểm 1 tiết", "Điểm thi", "Điểm TB", "Xếp loại", "Ghi chú" };
                        for (int i = 0; i < headers.Length; i++)
                        {
                            worksheet.Cell(3, i + 1).Value = headers[i];
                            worksheet.Cell(3, i + 1).Style.Font.Bold = true;
                        }

                        //Vẽ dữ liệu
                        int row = 4;
                        foreach (var item in DanhSachTongKetMon)
                        {
                            worksheet.Cell(row, 1).Value = item.STT;
                            worksheet.Cell(row, 2).Value = item.HocSinhID;
                            worksheet.Cell(row, 3).Value = item.HoTen;
                            worksheet.Cell(row, 4).Value = item.TenLop;
                            worksheet.Cell(row, 5).Value = item.MonHoc;
                            worksheet.Cell(row, 6).Value = item.NamHoc;
                            worksheet.Cell(row, 7).Value = item.HocKy;
                            worksheet.Cell(row, 8).Value = item.DiemMiengStr;
                            worksheet.Cell(row, 9).Value = item.Diem15PhutStr;
                            worksheet.Cell(row, 10).Value = item.Diem1TietStr;
                            worksheet.Cell(row, 11).Value = item.DiemThiStr;
                            worksheet.Cell(row, 12).Value = item.DiemTrungBinh;
                            worksheet.Cell(row, 13).Value = item.XepLoai;
                            worksheet.Cell(row, 14).Value = item.GhiChu;
                            row++;
                        }

                        //Thống kê
                        row += 2;
                        worksheet.Cell(row, 1).Value = "THỐNG KÊ";
                        worksheet.Cell(row, 1).Style.Font.Bold = true;
                        row++;
                        worksheet.Cell(row, 1).Value = "Tổng số học sinh:";
                        worksheet.Cell(row, 2).Value = TongHocSinh;
                        row++;
                        worksheet.Cell(row, 1).Value = "Số lượng đạt:";
                        worksheet.Cell(row, 2).Value = SoLuongDat;
                        row++;
                        worksheet.Cell(row, 1).Value = "Số lượng không đạt:";
                        worksheet.Cell(row, 2).Value = SoLuongKhongDat;
                        row++;
                        worksheet.Cell(row, 1).Value = "Tỉ lệ đạt:";
                        worksheet.Cell(row, 2).Value = $"{TiLeDat:F2}%";

                        //Tự động chỉnh cột
                        worksheet.Columns().AdjustToContents();

                        workbook.SaveAs(saveFileDialog.FileName);
                    }

                    MessageBox.Show("Xuất Excel thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xuất Excel: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}