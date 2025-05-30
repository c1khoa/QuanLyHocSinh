using System.Collections.ObjectModel;
using System.Windows.Input;
using QuanLyHocSinh.Model.Entities;
using Microsoft.Win32;
using System.IO;
using ClosedXML.Excel;
using System;

namespace QuanLyHocSinh.ViewModel.TraCuu
{
    public class TraCuuBangDiemHocSinhViewModel : BaseViewModel
    {
        public string HoTen { get; set; }
        public string TenLop { get; set; }
        public string NamHoc { get; set; }
        public int HocKy { get; set; }
        public ObservableCollection<TongKetMonItem> BangDiemMon { get; set; }
        public ICommand ExportExcelCommand { get; set; }

        public ObservableCollection<string> DanhSachNamHoc { get; set; }
        public ObservableCollection<int> DanhSachHocKy { get; set; }

        private string _selectedNamHoc;
        public string SelectedNamHoc
        {
            get => _selectedNamHoc;
            set { _selectedNamHoc = value; OnPropertyChanged(); OnFilterChanged(); }
        }
        private int? _selectedHocKy;
        public int? SelectedHocKy
        {
            get => _selectedHocKy;
            set { _selectedHocKy = value; OnPropertyChanged(); OnFilterChanged(); }
        }

        private bool _isShowBangDiem;
        public bool IsShowBangDiem
        {
            get => _isShowBangDiem;
            set { _isShowBangDiem = value; OnPropertyChanged(); }
        }

        private HocSinh _hocSinh;
        public TraCuuBangDiemHocSinhViewModel(HocSinh hs, string namHoc = null, int? hocKy = null)
        {
            _hocSinh = hs;
            HoTen = hs.HoTen;
            TenLop = hs.TenLop;
            // Lấy danh sách năm học và học kỳ
            DanhSachNamHoc = new ObservableCollection<string>(HocSinhDAL.GetAllNamHoc());
            DanhSachNamHoc.Insert(0, "Chọn");
            DanhSachHocKy = new ObservableCollection<int>(HocSinhDAL.GetAllHocKy());
            DanhSachHocKy.Insert(0, 0); // 0 đại diện cho "Chọn"
            SelectedNamHoc = "Chọn";
            SelectedHocKy = 0;
            BangDiemMon = null;
            IsShowBangDiem = false;
            ExportExcelCommand = new RelayCommand(ExportExcel);
        }
        private void OnFilterChanged()
        {
            if (SelectedNamHoc != null && SelectedNamHoc != "Chọn" && SelectedHocKy.HasValue && SelectedHocKy.Value != 0)
            {
                NamHoc = SelectedNamHoc;
                HocKy = SelectedHocKy.Value;
                var list = HocSinhDAL.GetBangDiemHocSinh(_hocSinh.HocSinhID, NamHoc, HocKy);
                BangDiemMon = new ObservableCollection<TongKetMonItem>(list);
                IsShowBangDiem = BangDiemMon != null && BangDiemMon.Count > 0;
                OnPropertyChanged(nameof(BangDiemMon));
            }
            else
            {
                BangDiemMon = null;
                IsShowBangDiem = false;
                OnPropertyChanged(nameof(BangDiemMon));
            }
        }
        private void ExportExcel()
        {
            if (!IsShowBangDiem) return;
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel files (*.xlsx)|*.xlsx",
                    FileName = $"BangDiem_{HoTen}_{NamHoc}_HK{HocKy}"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("Bảng điểm");

                        // Thêm thông tin học sinh
                        worksheet.Cell("A1").Value = "Họ tên:";
                        worksheet.Cell("B1").Value = HoTen;
                        worksheet.Cell("A2").Value = "Lớp:";
                        worksheet.Cell("B2").Value = TenLop;
                        worksheet.Cell("A3").Value = "Năm học:";
                        worksheet.Cell("B3").Value = NamHoc;
                        worksheet.Cell("A4").Value = "Học kỳ:";
                        worksheet.Cell("B4").Value = HocKy;

                        // Thêm header
                        var headers = new[] { "Môn học", "Điểm miệng", "Điểm 15p", "Điểm 1 tiết", "Điểm thi", "Điểm TB", "Xếp loại" };
                        for (int i = 0; i < headers.Length; i++)
                        {
                            worksheet.Cell(6, i + 1).Value = headers[i];
                            worksheet.Cell(6, i + 1).Style.Font.Bold = true;
                        }

                        // Thêm dữ liệu
                        int row = 7;
                        foreach (var item in BangDiemMon)
                        {
                            worksheet.Cell(row, 1).Value = item.MonHoc;
                            worksheet.Cell(row, 2).Value = item.DiemMiengStr;
                            worksheet.Cell(row, 3).Value = item.Diem15PhutStr;
                            worksheet.Cell(row, 4).Value = item.Diem1TietStr;
                            worksheet.Cell(row, 5).Value = item.DiemThiStr;
                            worksheet.Cell(row, 6).Value = item.DiemTrungBinh;
                            worksheet.Cell(row, 7).Value = item.XepLoai;
                            row++;
                        }

                        // Format
                        worksheet.Range(1, 1, 4, 2).Style.Font.Bold = true;
                        worksheet.Range(6, 1, 6, 7).Style.Fill.BackgroundColor = XLColor.LightGray;

                        // Tự động điều chỉnh độ rộng cột
                        worksheet.Columns().AdjustToContents();

                        workbook.SaveAs(saveFileDialog.FileName);
                    }

                    System.Windows.MessageBox.Show("Xuất Excel thành công!", "Thông báo", 
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Lỗi khi xuất Excel: {ex.Message}", "Lỗi", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }
}
