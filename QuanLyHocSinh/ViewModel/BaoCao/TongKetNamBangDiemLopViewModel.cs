using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using QuanLyHocSinh.Model.Entities;
using System.Windows;
using ClosedXML.Excel;
using System.IO;
using Microsoft.Win32;
using System.Linq;

namespace QuanLyHocSinh.ViewModel.BaoCao
{
    public class TongKetNamBangDiemLopViewModel : BaseViewModel
    {
        #region Properties
        private ObservableCollection<string> _danhSachLop = new();
        public ObservableCollection<string> DanhSachLop
        {
            get => _danhSachLop;
            set { _danhSachLop = value; OnPropertyChanged(nameof(DanhSachLop)); }
        }

        private string _selectedLop;
        public string SelectedLop
        {
            get => _selectedLop;
            set { _selectedLop = value; OnPropertyChanged(nameof(SelectedLop)); LoadData(); }
        }

        private ObservableCollection<BangDiemLopItem> _bangDiemLop = new();
        public ObservableCollection<BangDiemLopItem> BangDiemLop
        {
            get => _bangDiemLop;
            set { _bangDiemLop = value; OnPropertyChanged(nameof(BangDiemLop)); }
        }

        private string _namHoc;
        public string NamHoc
        {
            get => _namHoc;
            set { _namHoc = value; OnPropertyChanged(nameof(NamHoc)); }
        }

        private ObservableCollection<string> _danhSachHocKy = new() { "Cả năm", "1", "2" };
        public ObservableCollection<string> DanhSachHocKy
        {
            get => _danhSachHocKy;
            set { _danhSachHocKy = value; OnPropertyChanged(nameof(DanhSachHocKy)); }
        }
        private string _selectedHocKy = "Cả năm";
        public string SelectedHocKy
        {
            get => _selectedHocKy;
            set { _selectedHocKy = value; OnPropertyChanged(nameof(SelectedHocKy)); LoadData(); }
        }

        private ObservableCollection<string> _danhSachMonHoc = new();
        public ObservableCollection<string> DanhSachMonHoc
        {
            get => _danhSachMonHoc;
            set { _danhSachMonHoc = value; OnPropertyChanged(nameof(DanhSachMonHoc)); }
        }

        private double _diemTrungBinhLop;
        public double DiemTrungBinhLop
        {
            get => _diemTrungBinhLop;
            set { _diemTrungBinhLop = value; OnPropertyChanged(nameof(DiemTrungBinhLop)); }
        }
        private double _tiLeDat;
        public double TiLeDat
        {
            get => _tiLeDat;
            set { _tiLeDat = value; OnPropertyChanged(nameof(TiLeDat)); }
        }
        private int _soLuongGioi;
        public int SoLuongGioi
        {
            get => _soLuongGioi;
            set { _soLuongGioi = value; OnPropertyChanged(nameof(SoLuongGioi)); }
        }
        private int _soLuongKha;
        public int SoLuongKha
        {
            get => _soLuongKha;
            set { _soLuongKha = value; OnPropertyChanged(nameof(SoLuongKha)); }
        }
        private int _soLuongTrungBinh;
        public int SoLuongTrungBinh
        {
            get => _soLuongTrungBinh;
            set { _soLuongTrungBinh = value; OnPropertyChanged(nameof(SoLuongTrungBinh)); }
        }
        private int _soLuongYeu;
        public int SoLuongYeu
        {
            get => _soLuongYeu;
            set { _soLuongYeu = value; OnPropertyChanged(nameof(SoLuongYeu)); }
        }
        private int _soLuongKem;
        public int SoLuongKem
        {
            get => _soLuongKem;
            set { _soLuongKem = value; OnPropertyChanged(nameof(SoLuongKem)); }
        }
        private ObservableCollection<(string MonHoc, double DiemTB)> _diemTrungBinhTungMon = new();
        public ObservableCollection<(string MonHoc, double DiemTB)> DiemTrungBinhTungMon
        {
            get => _diemTrungBinhTungMon;
            set { _diemTrungBinhTungMon = value; OnPropertyChanged(nameof(DiemTrungBinhTungMon)); }
        }
        #endregion

        #region Commands
        public ICommand ExportExcelCommand { get; }
        #endregion

        public TongKetNamBangDiemLopViewModel()
        {
            ExportExcelCommand = new RelayCommand(ExportExcel);
        }

        private void LoadData()
        {
            if (string.IsNullOrEmpty(SelectedLop) || string.IsNullOrEmpty(NamHoc) || string.IsNullOrEmpty(SelectedHocKy))
                return;
            BangDiemLop.Clear();
            int hocKy = SelectedHocKy == "Cả năm" ? 0 : int.Parse(SelectedHocKy);
            var result = TongKetNamDAL.GetBangDiemLopVaThongKe(NamHoc, SelectedLop, hocKy);
            foreach (var item in result.BangDiem)
                BangDiemLop.Add(item);
            DiemTrungBinhLop = result.DiemTrungBinhLop;
            TiLeDat = result.TiLeDat;
            SoLuongGioi = result.SoLuongGioi;
            SoLuongKha = result.SoLuongKha;
            SoLuongTrungBinh = result.SoLuongTrungBinh;
            SoLuongYeu = result.SoLuongYeu;
            SoLuongKem = result.SoLuongKem;
            DiemTrungBinhTungMon = new ObservableCollection<(string MonHoc, double DiemTB)>(result.DiemTrungBinhTungMon);
        }

        private void ExportExcel()
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx",
                FileName = $"BangDiemLop_{SelectedLop}_{NamHoc}_HK{SelectedHocKy}"
            };

            if (dialog.ShowDialog() == true)
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Bảng điểm lớp");

                    //Vẽ tiêu đề
                    worksheet.Cell("A1").Value = "BÁO CÁO TỔNG KẾT BẢNG ĐIỂM LỚP";
                    worksheet.Range("A1:N1").Merge();
                    worksheet.Cell("A1").Style.Font.Bold = true;
                    worksheet.Cell("A1").Style.Font.FontSize = 16;
                    worksheet.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    //Tạo header
                    var headers = new[] { "STT", "Mã HS", "Họ tên", "Điểm miệng", "Điểm 15p", "Điểm 1 tiết", "Điểm thi", "Điểm TB", "Xếp loại" };
                    for (int i = 0; i < headers.Length; i++)
                    {
                        worksheet.Cell(3, i + 1).Value = headers[i];
                        worksheet.Cell(3, i + 1).Style.Font.Bold = true;
                    }

                    // Thêm header cho từng môn học
                    int col = 4;
                    foreach (var mon in DanhSachMonHoc)
                    {
                        worksheet.Cell(1, col++).Value = mon;
                    }

                    // Thêm header cho điểm TB và xếp loại
                    worksheet.Cell(1, col).Value = "Điểm TB";
                    worksheet.Cell(1, col + 1).Value = "Xếp loại";

                    // Data
                    int row = 4;
                    foreach (var item in BangDiemLop)
                    {
                        worksheet.Cell(row, 1).Value = item.STT;
                        worksheet.Cell(row, 2).Value = item.HocSinhID;
                        worksheet.Cell(row, 3).Value = item.HoTen;

                        // Điểm từng môn
                        col = 4;
                        foreach (var mon in DanhSachMonHoc)
                        {
                            if (item.DiemTungMon.ContainsKey(mon))
                                worksheet.Cell(row, col).Value = item.DiemTungMon[mon];
                            col++;
                        }

                        // Điểm TB và xếp loại
                        worksheet.Cell(row, col).Value = item.DiemTrungBinh;
                        worksheet.Cell(row, col + 1).Value = item.XepLoai;
                        row++;
                    }

                    // Thêm thống kê dưới bảng điểm
                    row += 2;
                    worksheet.Cell(row, 1).Value = "THỐNG KÊ";
                    worksheet.Cell(row, 1).Style.Font.Bold = true;
                    row++;
                    worksheet.Cell(row, 1).Value = "Điểm trung bình của lớp:";
                    worksheet.Cell(row, 2).Value = DiemTrungBinhLop;
                    row++;
                    worksheet.Cell(row, 1).Value = "Tỉ lệ đạt:";
                    worksheet.Cell(row, 2).Value = TiLeDat + "%";
                    row++;
                    worksheet.Cell(row, 1).Value = "Số lượng giỏi:";
                    worksheet.Cell(row, 2).Value = SoLuongGioi;
                    row++;
                    worksheet.Cell(row, 1).Value = "Số lượng khá:";
                    worksheet.Cell(row, 2).Value = SoLuongKha;
                    row++;
                    worksheet.Cell(row, 1).Value = "Số lượng trung bình:";
                    worksheet.Cell(row, 2).Value = SoLuongTrungBinh;
                    row++;
                    worksheet.Cell(row, 1).Value = "Số lượng yếu:";
                    worksheet.Cell(row, 2).Value = SoLuongYeu;
                    row++;
                    worksheet.Cell(row, 1).Value = "Số lượng kém:";
                    worksheet.Cell(row, 2).Value = SoLuongKem;
                    row++;
                    row++;
                    worksheet.Cell(row, 1).Value = "Điểm trung bình từng môn:";
                    worksheet.Cell(row, 1).Style.Font.Bold = true;
                    worksheet.Cell(row, 1).Value = "Môn học";
                    worksheet.Cell(row, 2).Value = "Điểm TB";
                    row++;
                    foreach (var mon in DiemTrungBinhTungMon)
                    {
                        worksheet.Cell(row, 1).Value = mon.MonHoc;
                        worksheet.Cell(row, 2).Value = mon.DiemTB;
                        row++;
                    }

                    //Tự động chỉnh cột
                    worksheet.Columns().AdjustToContents();

                    workbook.SaveAs(dialog.FileName);
                }

                MessageBox.Show("Xuất Excel thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
