using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using MySql.Data.MySqlClient;
using QuanLyHocSinh.Model.Entities;
using QuanLyHocSinh.View.Dialogs.MessageBox;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace QuanLyHocSinh.ViewModel.QuanLyTaiKhoan
{
    public class DiemDialogViewModel : BaseViewModel
    {
        public User CurrentUser { get; set; }

        public class DiemTongHopHocSinh
        {
            public string MonHoc { get; set; }
            public string Mieng { get; set; }
            public string Diem15Phut { get; set; }
            public string Diem1Tiet { get; set; }
            public string DiemHocKy { get; set; }
            public int HocKy { get; set; }
            public double TrungBinh { get; set; }
            public string NamHocID { get; set; }
        }

        public ICommand ExportToExcelCommand { get; set; }

        private ObservableCollection<DiemTongHopHocSinh> _allDiem;
        public ObservableCollection<string> dsNamHoc { get; set; }
        public ObservableCollection<int> dsHocKy { get; set; }

        private double _tbHKI;
        public double TBHKI { get => _tbHKI; set { _tbHKI = value; OnPropertyChanged(); } }

        private double _tbHKII;
        public double TBHKII { get => _tbHKII; set { _tbHKII = value; OnPropertyChanged(); } }

        private double _tbCaNam;
        public double TBCaNam { get => _tbCaNam; set { _tbCaNam = value; OnPropertyChanged(); } }

        private string _xepLoai;
        public string XepLoai { get => _xepLoai; set { _xepLoai = value; OnPropertyChanged(); } }

        private string _xepLoaiHKI;
        public string XepLoaiHKI { get => _xepLoaiHKI; set { _xepLoaiHKI = value; OnPropertyChanged(); } }

        private string _xepLoaiHKII;
        public string XepLoaiHKII { get => _xepLoaiHKII; set { _xepLoaiHKII = value; OnPropertyChanged(); } }

        private ObservableCollection<DiemTongHopHocSinh> _filteredDiem;
        public ObservableCollection<DiemTongHopHocSinh> FilteredDiem
        {
            get => _filteredDiem;
            set { _filteredDiem = value; OnPropertyChanged(); TinhTrungBinhVaXepLoai(); }
        }

        private int? _selectedHocKyFilter;
        public int? SelectedHocKyFilter
        {
            get => _selectedHocKyFilter;
            set { _selectedHocKyFilter = value; OnPropertyChanged(); FilterData(); }
        }

        private string _selectedNamHocFilter;
        public string SelectedNamHocFilter
        {
            get => _selectedNamHocFilter;
            set { _selectedNamHocFilter = value; OnPropertyChanged(); FilterData(); }
        }

        public DiemDialogViewModel(User currentUser)
        {
            CurrentUser = currentUser;

            dsNamHoc = new ObservableCollection<string>(HocSinhDAL.GetAllNamHoc());
            dsHocKy = new ObservableCollection<int>(HocSinhDAL.GetAllHocKy());

            _allDiem = new ObservableCollection<DiemTongHopHocSinh>(GetDiemTongHopCuaHocSinh(currentUser.TenDangNhap));

            ExportToExcelCommand = new RelayCommand<object>((p) => true, (p) => ExportToExcel());

            SelectedHocKyFilter = dsHocKy.FirstOrDefault();
            SelectedNamHocFilter = dsNamHoc.FirstOrDefault();
            FilterData();
        }

        private void FilterData()
        {
            if (_allDiem == null) return;

            IEnumerable<DiemTongHopHocSinh> filtered = _allDiem;

            if (SelectedHocKyFilter.HasValue)
                filtered = filtered.Where(d => d.HocKy == SelectedHocKyFilter.Value);

            if (!string.IsNullOrEmpty(SelectedNamHocFilter))
                filtered = filtered.Where(d => d.NamHocID == SelectedNamHocFilter);

            FilteredDiem = new ObservableCollection<DiemTongHopHocSinh>(filtered);
        }

        private void TinhTrungBinhVaXepLoai()
        {
            if (string.IsNullOrEmpty(SelectedNamHocFilter)) return;

            var hki = _allDiem.Where(d => d.HocKy == 1 && d.NamHocID == SelectedNamHocFilter).ToList();
            var hkii = _allDiem.Where(d => d.HocKy == 2 && d.NamHocID == SelectedNamHocFilter).ToList();

            TBHKI = hki.Any() ? Math.Round(hki.Average(d => d.TrungBinh), 2) : 0;
            TBHKII = hkii.Any() ? Math.Round(hkii.Average(d => d.TrungBinh), 2) : 0;

            if (TBHKI == 0 && TBHKII == 0)
                TBCaNam = 0;
            else if (TBHKI == 0)
                TBCaNam = TBHKII;
            else if (TBHKII == 0)
                TBCaNam = TBHKI;
            else
                TBCaNam = Math.Round((TBHKI + TBHKII * 2) / 3, 2);

            XepLoaiHKI = XepLoaiTheoDiem(TBHKI);
            XepLoaiHKII = XepLoaiTheoDiem(TBHKII);
            XepLoai = XepLoaiTheoDiem(TBCaNam);
        }

        private string XepLoaiTheoDiem(double diem)
        {
            if (diem >= 8.0) return "Giỏi";
            if (diem >= 6.5) return "Khá";
            if (diem >= 5.0) return "Trung bình";
            return "Yếu";
        }

        public static List<DiemTongHopHocSinh> GetDiemTongHopCuaHocSinh(string hocSinhID)
        {
            var list = new List<DiemTongHopHocSinh>();

            string query = @"
                SELECT 
                    mh.TenMonHoc AS MonHoc,
                    d.NamHocID,
                    GROUP_CONCAT(CASE WHEN ld.LoaiDiemID = 'LD01' THEN ctd.GiaTri END) AS Mieng,
                    GROUP_CONCAT(CASE WHEN ld.LoaiDiemID = 'LD02' THEN ctd.GiaTri END) AS `15Phut`,
                    GROUP_CONCAT(CASE WHEN ld.LoaiDiemID = 'LD03' THEN ctd.GiaTri END) AS `1Tiet`,
                    GROUP_CONCAT(CASE WHEN ld.LoaiDiemID = 'LD04' THEN ctd.GiaTri END) AS Thi,
                    d.HocKy,
                    ROUND(SUM(ctd.GiaTri * ld.HeSo) / SUM(ld.HeSo), 2) AS TrungBinh
                FROM 
                    CHITIETDIEM ctd
                JOIN DIEM d ON ctd.DiemID = d.DiemID
                JOIN MONHOC mh ON d.MonHocID = mh.MonHocID
                JOIN LOAIDIEM ld ON ctd.LoaiDiemID = ld.LoaiDiemID
                JOIN HOCSINH hs ON d.HocSinhID = hs.HocSinhID
                WHERE hs.HocSinhID = @HocSinhID
                GROUP BY mh.TenMonHoc, d.HocKy, d.NamHocID
                ORDER BY d.HocKy, mh.TenMonHoc;
            ";

            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@HocSinhID", hocSinhID);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new DiemTongHopHocSinh
                            {
                                MonHoc = reader["MonHoc"]?.ToString(),
                                Mieng = reader["Mieng"]?.ToString(),
                                Diem15Phut = reader["15Phut"]?.ToString(),
                                Diem1Tiet = reader["1Tiet"]?.ToString(),
                                DiemHocKy = reader["Thi"]?.ToString(),
                                NamHocID = reader["NamHocID"]?.ToString(),
                                HocKy = Convert.ToInt32(reader["HocKy"]),
                                TrungBinh = reader["TrungBinh"] != DBNull.Value ? Convert.ToDouble(reader["TrungBinh"]) : 0.0
                            });
                        }
                    }
                }
            }

            return list;
        }

        private async void ExportToExcel()
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel Workbook (*.xlsx)|*.xlsx",
                    FileName = $"BangDiem_Nam_{SelectedNamHocFilter}.xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    using (SpreadsheetDocument document = SpreadsheetDocument.Create(saveFileDialog.FileName, SpreadsheetDocumentType.Workbook))
                    {
                        WorkbookPart workbookPart = document.AddWorkbookPart();
                        workbookPart.Workbook = new Workbook();
                        Sheets sheets = workbookPart.Workbook.AppendChild(new Sheets());

                        for (int hk = 1; hk <= 2; hk++)
                        {
                            WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                            SheetData sheetData = new SheetData();
                            worksheetPart.Worksheet = new Worksheet(sheetData);

                            string sheetName = $"Học kỳ {hk}";
                            Sheet sheet = new Sheet()
                            {
                                Id = workbookPart.GetIdOfPart(worksheetPart),
                                SheetId = (uint)hk,
                                Name = sheetName
                            };
                            sheets.Append(sheet);

                            Row titleRow = new Row();
                            titleRow.Append(new Cell
                            {
                                DataType = CellValues.String,
                                CellValue = new CellValue($"BẢNG ĐIỂM - Năm học: {SelectedNamHocFilter} - Học kỳ: {hk}")
                            });
                            sheetData.AppendChild(titleRow);
                            sheetData.AppendChild(new Row());

                            Row headerRow = new Row();
                            string[] headers = new string[] { "Môn học", "Điểm miệng", "Điểm 15 phút", "Điểm 1 tiết", "Điểm thi", "Điểm trung bình" };
                            foreach (string header in headers)
                            {
                                headerRow.AppendChild(new Cell { DataType = CellValues.String, CellValue = new CellValue(header) });
                            }
                            sheetData.AppendChild(headerRow);

                            var diemHocKy = _allDiem.Where(d => d.NamHocID == SelectedNamHocFilter && d.HocKy == hk);
                            foreach (var diem in diemHocKy)
                            {
                                Row newRow = new Row();
                                newRow.Append(
                                    CreateTextCell(diem.MonHoc),
                                    CreateTextCell(diem.Mieng),
                                    CreateTextCell(diem.Diem15Phut),
                                    CreateTextCell(diem.Diem1Tiet),
                                    CreateTextCell(diem.DiemHocKy),
                                    CreateNumberCell(diem.TrungBinh)
                                );
                                sheetData.AppendChild(newRow);
                            }
                        }

                        workbookPart.Workbook.Save();
                    }

                    await ShowNotificationAsync("Thông báo", "Xuất Excel thành công.");
                }
            }
            catch
            {
                await DialogHost.Show(new ErrorDialog("Thất bại", "Xuất Excel không thành công."), "RootDialog_Score");
            }
        }

        private Cell CreateTextCell(string value)
        {
            return new Cell { DataType = CellValues.String, CellValue = new CellValue(value ?? string.Empty) };
        }

        private Cell CreateNumberCell(double value)
        {
            return new Cell { DataType = CellValues.Number, CellValue = new CellValue(value.ToString()) };
        }

        private async Task ShowNotificationAsync(string title, string message)
        {
            try
            {
                await DialogHost.Show(new NotifyDialog(title, message), "RootDialog_Score");
            }
            catch
            {
                MessageBox.Show(message, title, MessageBoxButton.OK,
                    title.Contains("Lỗi") ? MessageBoxImage.Error : MessageBoxImage.Information);
            }
        }
    }
}
