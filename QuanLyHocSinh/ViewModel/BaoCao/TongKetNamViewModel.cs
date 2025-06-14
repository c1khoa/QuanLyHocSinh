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

namespace QuanLyHocSinh.ViewModel.BaoCao
{
    public class TongKetNamViewModel : BaseViewModel
    {
        private MainViewModel _mainVM;

        #region Properties for filtering
        private ObservableCollection<TongKetNamHocItem> _danhSachTongKetNamHoc = new();
        public ObservableCollection<TongKetNamHocItem> DanhSachTongKetNamHoc
        {
            get => _danhSachTongKetNamHoc;
            set { _danhSachTongKetNamHoc = value; OnPropertyChanged(nameof(DanhSachTongKetNamHoc)); }
        }

        private ObservableCollection<string> _danhSachNamHoc = new();
        public ObservableCollection<string> DanhSachNamHoc
        {
            get => _danhSachNamHoc;
            set { _danhSachNamHoc = value; OnPropertyChanged(nameof(DanhSachNamHoc)); }
        }

        private ObservableCollection<string> _danhSachLop = new();
        public ObservableCollection<string> DanhSachLop
        {
            get => _danhSachLop;
            set { _danhSachLop = value; OnPropertyChanged(nameof(DanhSachLop)); }
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

        private string _selectedLop;
        public string SelectedLop
        {
            get => _selectedLop;
            set { _selectedLop = value; OnPropertyChanged(nameof(SelectedLop)); LoadData(); }
        }

        private string _selectedHocKy;
        public string SelectedHocKy
        {
            get => _selectedHocKy;
            set { _selectedHocKy = value; OnPropertyChanged(nameof(SelectedHocKy)); LoadData(); }
        }
        #endregion

        #region Statistics Properties
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
        #endregion

        #region Commands
        public ICommand OpenBangDiemLopCommand { get; }
        public ICommand OpenBangDiemHocSinhCommand { get; }
        public ICommand ExportExcelCommand { get; }
        #endregion

        public TongKetNamViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM;
            
            // Khởi tạo danh sách
            var dsNamHoc = TongKetNamDAL.GetAllNamHoc();
            dsNamHoc.Insert(0, "Tất cả");
            DanhSachNamHoc = new ObservableCollection<string>(dsNamHoc);

            var dsLop = TongKetNamDAL.GetAllLop();
            dsLop.Insert(0, "Tất cả");
            DanhSachLop = new ObservableCollection<string>(dsLop);

            var dsHocKy = TongKetNamDAL.GetAllHocKy().Select(h => h.ToString()).ToList();
            dsHocKy.Insert(0, "Cả năm");
            DanhSachHocKy = new ObservableCollection<string>(dsHocKy);

            // Khởi tạo commands
            OpenBangDiemLopCommand = new RelayCommand(OpenBangDiemLop);
            OpenBangDiemHocSinhCommand = new RelayCommand<TongKetNamHocItem>(null, OpenBangDiemHocSinh);
            ExportExcelCommand = new RelayCommand(ExportExcel);

            // Mặc định chọn 'Tất cả'
            SelectedNamHoc = "Tất cả";
            SelectedLop = "Tất cả";
            SelectedHocKy = "Tất cả";
        }

        private void LoadData()
        {
            // Nếu tất cả filter đều là 'Tất cả' thì lấy toàn bộ dữ liệu
            string? namHoc = SelectedNamHoc == "Tất cả" ? null : SelectedNamHoc;
            string? lop = SelectedLop == "Tất cả" ? null : SelectedLop;
            int? hocKy = (SelectedHocKy == "Tất cả" || SelectedHocKy == "Cả năm" || string.IsNullOrEmpty(SelectedHocKy)) ? null : int.Parse(SelectedHocKy);

            DanhSachTongKetNamHoc.Clear();
            var data = TongKetNamDAL.GetTongKetNamHoc(namHoc, lop, hocKy);
            int stt = 1;
            foreach (var item in data)
            {
                item.STT = stt++;
                // Lấy điểm TB từng học kỳ
                var diemMon = TongKetNamDAL.GetBangDiemMonHoc(item.HocSinhID, item.NamHoc);
                // Tính điểm TB học kỳ 1
                var tb1 = diemMon.Where(x => x.DiemTBHK1.HasValue).Select(x => x.DiemTBHK1.Value).ToList();
                item.DiemTBHK1 = tb1.Count > 0 ? Math.Round(tb1.Average(), 2) : (double?)null;
                // Tính điểm TB học kỳ 2
                var tb2 = diemMon.Where(x => x.DiemTBHK2.HasValue).Select(x => x.DiemTBHK2.Value).ToList();
                item.DiemTBHK2 = tb2.Count > 0 ? Math.Round(tb2.Average(), 2) : (double?)null;
                // Tính điểm TB cả năm
                if (item.DiemTBHK1.HasValue && item.DiemTBHK2.HasValue)
                    item.DiemTBCaNam = Math.Round((item.DiemTBHK1.Value + item.DiemTBHK2.Value * 2) / 3, 2);
                else if (item.DiemTBHK1.HasValue)
                    item.DiemTBCaNam = item.DiemTBHK1;
                else if (item.DiemTBHK2.HasValue)
                    item.DiemTBCaNam = item.DiemTBHK2;
                else
                    item.DiemTBCaNam = null;
                // Xếp loại
                if (!item.DiemTBCaNam.HasValue) item.XepLoai = "-";
                else if (item.DiemTBCaNam >= 8.5) item.XepLoai = "Giỏi";
                else if (item.DiemTBCaNam >= 6.5) item.XepLoai = "Khá";
                else if (item.DiemTBCaNam >= 5.0) item.XepLoai = "Trung bình";
                else if (item.DiemTBCaNam >= 3.5) item.XepLoai = "Yếu";
                else item.XepLoai = "Kém";
                // Kết quả
                item.KetQua = (item.DiemTBCaNam.HasValue && item.DiemTBCaNam >= 5.0) ? "Đạt" : "Không đạt";
                DanhSachTongKetNamHoc.Add(item);
            }

            // Cập nhật thống kê
            TongHocSinh = DanhSachTongKetNamHoc.Count;
            SoLuongDat = DanhSachTongKetNamHoc.Count(x => x.GhiChu == "Đạt");
            SoLuongKhongDat = DanhSachTongKetNamHoc.Count(x => x.GhiChu == "Không đạt");
            TiLeDat = TongHocSinh > 0 ? Math.Round(100.0 * SoLuongDat / TongHocSinh, 2) : 0;
            SoLuongGioi = DanhSachTongKetNamHoc.Count(x => x.XepLoai == "Giỏi");
            SoLuongKha = DanhSachTongKetNamHoc.Count(x => x.XepLoai == "Khá");
            SoLuongTrungBinh = DanhSachTongKetNamHoc.Count(x => x.XepLoai == "Trung bình");
            SoLuongYeu = DanhSachTongKetNamHoc.Count(x => x.XepLoai == "Yếu");
            SoLuongKem = DanhSachTongKetNamHoc.Count(x => x.XepLoai == "Kém");
        }

        private void OpenBangDiemLop()
        {
            if (SelectedNamHoc == "Tất cả" || SelectedHocKy == "Tất cả" || SelectedLop == "Tất cả")
            {
                MessageBox.Show("Vui lòng chọn đầy đủ Năm học, Học kỳ và Lớp!", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var dialog = new TongKetNamBangDiemLop(SelectedNamHoc, int.Parse(SelectedHocKy), SelectedLop);
            dialog.ShowDialog();
        }

        private void OpenBangDiemHocSinh(TongKetNamHocItem item)
        {
            if (item == null) return;
            var dialog = new QuanLyHocSinh.View.Dialogs.ChiTietHocSinhDialog(item.HocSinhID, item.NamHoc, item.TenLop);
            dialog.ShowDialog();
        }

        private void ExportExcel()
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx",
                FileName = SelectedNamHoc == "Tất cả" ? $"TongKetNam_{SelectedLop}_HK{SelectedHocKy}" : SelectedLop == "Tất cả" ? $"TongKetNam_{SelectedNamHoc}_HK{SelectedHocKy}" : SelectedHocKy == "Tất cả" ? $"TongKetNam_{SelectedNamHoc}_{SelectedLop}" : $"TongKetNam_{SelectedNamHoc}_{SelectedLop}_HK{SelectedHocKy}"
            };

            if (dialog.ShowDialog() == true)
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Tổng kết năm học");

                    //Vẽ tiêu đề
                    worksheet.Cell("A1").Value = "BÁO CÁO TỔNG KẾT NĂM HỌC";
                    worksheet.Range("A1:N1").Merge();
                    worksheet.Cell("A1").Style.Font.Bold = true;
                    worksheet.Cell("A1").Style.Font.FontSize = 16;
                    worksheet.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    //Tạo header
                    var headers = new[] { "STT", "Mã HS", "Họ tên", "Lớp", "Năm học", "Học kỳ","Điểm TB HK1", "Điểm TB HK2", "Điểm TB Cả năm", "Xếp loại", "Kết quả" };
                    for (int i = 0; i < headers.Length; i++)
                    {
                        worksheet.Cell(3, i + 1).Value = headers[i];
                        worksheet.Cell(3, i + 1).Style.Font.Bold = true;
                    }

                    //Vẽ dữ liệu
                    int row = 4;
                    foreach (var item in DanhSachTongKetNamHoc)
                    {
                        worksheet.Cell(row, 1).Value = item.STT;
                        worksheet.Cell(row, 2).Value = item.HocSinhID;
                        worksheet.Cell(row, 3).Value = item.HoTen;
                        worksheet.Cell(row, 4).Value = item.TenLop;
                        worksheet.Cell(row, 5).Value = item.NamHoc;
                        worksheet.Cell(row, 6).Value = item.HocKy;
                        worksheet.Cell(row, 7).Value = item.DiemTBHK1;
                        worksheet.Cell(row, 8).Value = item.DiemTBHK2;
                        worksheet.Cell(row, 9).Value = item.DiemTBCaNam;
                        worksheet.Cell(row, 10).Value = item.XepLoai;
                        worksheet.Cell(row, 11).Value = item.KetQua;
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
                    
                    //Tự động chỉnh cột
                    worksheet.Columns().AdjustToContents();

                    workbook.SaveAs(dialog.FileName);
                }

                MessageBox.Show("Xuất Excel thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
