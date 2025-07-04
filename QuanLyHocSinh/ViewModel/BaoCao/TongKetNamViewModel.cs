using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows.Input;
using QuanLyHocSinh.Model.Entities;
using System.Windows;
using QuanLyHocSinh.View.Dialogs;
using ClosedXML.Excel;
using System.IO;
using Microsoft.Win32;
using MaterialDesignThemes.Wpf;
using QuanLyHocSinh.View.Dialogs.MessageBox;

namespace QuanLyHocSinh.ViewModel.BaoCao
{
    public class TongKetNamViewModel : BaseViewModel
    {
        private MainViewModel _mainVM;

        #region Properties for filtering
        private ObservableCollection<TongKetLopItem> _danhSachTongKetLop = new();
        public ObservableCollection<TongKetLopItem> DanhSachTongKetLop
        {
            get => _danhSachTongKetLop;
            set { _danhSachTongKetLop = value; OnPropertyChanged(nameof(DanhSachTongKetLop)); }
        }

        private ObservableCollection<string> _danhSachNamHoc = new();
        public ObservableCollection<string> DanhSachNamHoc
        {
            get => _danhSachNamHoc;
            set { _danhSachNamHoc = value; OnPropertyChanged(nameof(DanhSachNamHoc)); }
        }

        private ObservableCollection<string> _danhSachHocKy = new();
        public ObservableCollection<string> DanhSachHocKy
        {
            get => _danhSachHocKy;
            set { _danhSachHocKy = value; OnPropertyChanged(nameof(DanhSachHocKy)); }
        }

        private string _selectedNamHoc;
        public string SelectedNamHoc
        {
            get => _selectedNamHoc;
            set { _selectedNamHoc = value; OnPropertyChanged(nameof(SelectedNamHoc)); LoadData(); }
        }

        private string _selectedHocKy;
        public string SelectedHocKy
        {
            get => _selectedHocKy;
            set { _selectedHocKy = value; OnPropertyChanged(nameof(SelectedHocKy)); LoadData(); }
        }
        #endregion

        #region Statistics Properties
        private int _tongSoLop;
        public int TongSoLop
        {
            get => _tongSoLop;
            set { _tongSoLop = value; OnPropertyChanged(nameof(TongSoLop)); }
        }

        private int _tongSiSo;
        public int TongSiSo
        {
            get => _tongSiSo;
            set { _tongSiSo = value; OnPropertyChanged(nameof(TongSiSo)); }
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

        private string _lopDatTiLeCaoNhat;
        public string LopDatTiLeCaoNhat
        {
            get => _lopDatTiLeCaoNhat;
            set { _lopDatTiLeCaoNhat = value; OnPropertyChanged(nameof(LopDatTiLeCaoNhat)); }
        }

        private double _tiLeCaoNhat;
        public double TiLeCaoNhat
        {
            get => _tiLeCaoNhat;
            set { _tiLeCaoNhat = value; OnPropertyChanged(nameof(TiLeCaoNhat)); }
        }
        #endregion

        #region Commands
        public ICommand ExportExcelCommand { get; }
        #endregion

        public TongKetNamViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM;

            // Load danh sách năm học (loại bỏ option "Tất cả", sắp xếp mới nhất trước)
            var dsNamHoc = TongKetNamDAL.GetAllNamHoc().OrderByDescending(n => n).ToList();
            DanhSachNamHoc = new ObservableCollection<string>(dsNamHoc);

            // Load danh sách học kỳ (không có "Tất cả")
            var dsHocKy = new List<string> { "1", "2" };
            DanhSachHocKy = new ObservableCollection<string>(dsHocKy);

            ExportExcelCommand = new RelayCommand(ExportExcel);

            // Đặt mặc định năm học mới nhất và học kỳ 2
            SelectedNamHoc = DanhSachNamHoc.FirstOrDefault() ?? "";
            SelectedHocKy = "2";
        }

        private void LoadData()
        {
            if (string.IsNullOrEmpty(SelectedHocKy) || string.IsNullOrEmpty(SelectedNamHoc))
                return;

            int hocKy = int.Parse(SelectedHocKy);

            DanhSachTongKetLop.Clear();
            var data = TongKetMonDAL.GetTongKetHocKyTheoLop(SelectedNamHoc, hocKy);
            
            int stt = 1;
            foreach (var item in data)
            {
                item.STT = stt++;
                DanhSachTongKetLop.Add(item);
            }

            CalculateStatistics();
        }

        private void CalculateStatistics()
        {
            var data = DanhSachTongKetLop.ToList();
            
            TongSoLop = data.Count;
            TongSiSo = data.Sum(x => x.SiSo);
            TongSoLuongDat = data.Sum(x => x.SoLuongDat);
            TiLeDatChung = TongSiSo > 0 ? Math.Round((double)TongSoLuongDat * 100 / TongSiSo, 2) : 0;

            var lopCaoNhat = data.OrderByDescending(x => x.TiLeDat).FirstOrDefault();
            if (lopCaoNhat != null)
            {
                LopDatTiLeCaoNhat = lopCaoNhat.TenLop;
                TiLeCaoNhat = lopCaoNhat.TiLeDat;
            }
            else
            {
                LopDatTiLeCaoNhat = "-";
                TiLeCaoNhat = 0;
            }
        }

        private async void ExportExcel()
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx",
                FileName = $"TongKetHocKy_{SelectedNamHoc}_HK{SelectedHocKy}"
            };

            if (dialog.ShowDialog() == true)
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Tổng kết học kỳ");

                    worksheet.Cell("A1").Value = "BÁO CÁO TỔNG KẾT HỌC KỲ";
                    worksheet.Range("A1:E1").Merge();
                    worksheet.Cell("A1").Style.Font.Bold = true;
                    worksheet.Cell("A1").Style.Font.FontSize = 16;
                    worksheet.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    var headers = new[] { "STT", "Lớp", "Sỉ số", "Số lượng đạt", "Tỉ lệ (%)" };
                    for (int i = 0; i < headers.Length; i++)
                    {
                        worksheet.Cell(3, i + 1).Value = headers[i];
                        worksheet.Cell(3, i + 1).Style.Font.Bold = true;
                    }

                    //Vẽ dữ liệu
                    int row = 4;
                    foreach (var item in DanhSachTongKetLop)
                    {
                        worksheet.Cell(row, 1).Value = item.STT;
                        worksheet.Cell(row, 2).Value = item.TenLop;
                        worksheet.Cell(row, 3).Value = item.SiSo;
                        worksheet.Cell(row, 4).Value = item.SoLuongDat;
                        worksheet.Cell(row, 5).Value = $"{item.TiLeDat:F2}";
                        row++;
                    }

                    //Thống kê
                    row += 2;
                    worksheet.Cell(row, 1).Value = "THỐNG KÊ TỔNG KẾT HỌC KỲ";
                    worksheet.Cell(row, 1).Style.Font.Bold = true;
                    row++;
                    worksheet.Cell(row, 1).Value = "Tổng số lớp:";
                    worksheet.Cell(row, 2).Value = TongSoLop;
                    row++;
                    worksheet.Cell(row, 1).Value = "Tổng sỉ số:";
                    worksheet.Cell(row, 2).Value = TongSiSo;
                    row++;
                    worksheet.Cell(row, 1).Value = "Tổng số lượng đạt:";
                    worksheet.Cell(row, 2).Value = TongSoLuongDat;
                    row++;
                    worksheet.Cell(row, 1).Value = "Tỉ lệ đạt chung:";
                    worksheet.Cell(row, 2).Value = $"{TiLeDatChung:F2}%";
                    row++;
                    worksheet.Cell(row, 1).Value = "Lớp đạt tỉ lệ cao nhất:";
                    worksheet.Cell(row, 2).Value = LopDatTiLeCaoNhat;
                    row++;
                    worksheet.Cell(row, 1).Value = "Tỉ lệ đạt cao nhất:";
                    worksheet.Cell(row, 2).Value = $"{TiLeCaoNhat:F2}%";
                    
                    //Tự động chỉnh cột
                    worksheet.ColumnsUsed().AdjustToContents();

                    workbook.SaveAs(dialog.FileName);
                }

                await ShowNotificationAsync("Thông báo", "✅ Xuất Excel thành công!");
            }
        }

        private async Task ShowNotificationAsync(string title, string message)
        {
            try
            {
                await DialogHost.Show(new NotifyDialog(title, message), "RootDialog_Main");
            }
            catch
            {
                MessageBox.Show(message, title, MessageBoxButton.OK, 
                    title.Contains("Lỗi") ? MessageBoxImage.Error : MessageBoxImage.Information);
            }
        }
    }
}
