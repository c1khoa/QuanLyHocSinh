using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using QuanLyHocSinh.Model.Entities;
using System.Windows;
using ClosedXML.Excel;

namespace QuanLyHocSinh.ViewModel.BaoCao
{
    public class ChiTietHocSinhViewModel : BaseViewModel
    {
        public HocSinhThongTin ThongTinChung { get; set; }
        public ObservableCollection<BangDiemMonHocItem> BangDiemMonHoc { get; set; }
        public double? DiemTrungBinhChung { get; set; }
        public string XepLoai { get; set; }
        public string KetQuaCuoiNam { get; set; }
        public ICommand ExportExcelCommand { get; set; }

        public ChiTietHocSinhViewModel(string hocSinhID, string namHoc, string lop)
        {
            // Lấy thông tin chung
            ThongTinChung = TongKetNamDAL.GetThongTinHocSinh(hocSinhID, namHoc, lop) ?? new HocSinhThongTin();
            // Lấy bảng điểm các môn
            var diemMon = TongKetNamDAL.GetBangDiemMonHoc(hocSinhID, namHoc);
            // Tính điểm TB cả năm từng môn
            foreach (var item in diemMon)
            {
                // Tính điểm TB cả năm nếu đủ dữ liệu
                if (item.DiemTBHK1.HasValue && item.DiemTBHK2.HasValue)
                    item.DiemTBCaNam = Math.Round((item.DiemTBHK1.Value + item.DiemTBHK2.Value * 2) / 3, 2);
                else if (item.DiemTBHK1.HasValue)
                    item.DiemTBCaNam = item.DiemTBHK1.Value;
                else if (item.DiemTBHK2.HasValue)
                    item.DiemTBCaNam = item.DiemTBHK2.Value;
                else
                    item.DiemTBCaNam = null;
            }
            BangDiemMonHoc = new ObservableCollection<BangDiemMonHocItem>(diemMon);
            // Tính điểm TB chung
            var diemTBCaNamList = diemMon.Where(x => x.DiemTBCaNam.HasValue).Select(x => x.DiemTBCaNam.Value).ToList();
            DiemTrungBinhChung = diemTBCaNamList.Count > 0 ? Math.Round(diemTBCaNamList.Average(), 2) : (double?)null;
            // Xếp loại học lực
            XepLoai = XepLoaiHocLuc(BangDiemMonHoc, DiemTrungBinhChung);
            // Kết quả cuối năm
            KetQuaCuoiNam = XacDinhKetQua(BangDiemMonHoc);
            ExportExcelCommand = new RelayCommand(ExportExcel);
        }

        // Thêm property cho điểm TB cả năm vào BangDiemMonHocItem
        // (nếu chưa có, bạn cần bổ sung vào model)

        private string XepLoaiHocLuc(ObservableCollection<BangDiemMonHocItem> bangDiem, double? tbChung)
        {
            if (!tbChung.HasValue) return "-";
            if (tbChung >= 8.5) return "Giỏi";
            if (tbChung >= 6.5) return "Khá";
            if (tbChung >= 5.0) return "Trung bình";
            if (tbChung >= 3.5) return "Yếu";
            return "Kém";
        }
        private string XacDinhKetQua(ObservableCollection<BangDiemMonHocItem> bangDiem)
        {
            if (bangDiem.Any(x => x.DiemTBCaNam.HasValue && x.DiemTBCaNam.Value < 3.5))
                return "Ở lại lớp";
            if (bangDiem.All(x => x.DiemTBCaNam.HasValue))
                return "Được lên lớp";
            return "-";
        }

        private void ExportExcel()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx",
                FileName = $"ChiTietHocSinh_{ThongTinChung.HoTen}_{ThongTinChung.NamHoc}.xlsx"
            };
            if (dialog.ShowDialog() == true)
            {
                using (var workbook = new XLWorkbook())
                {
                    var ws = workbook.Worksheets.Add("Chi tiết học sinh");
                    int row = 1;
                    ws.Cell(row++, 1).Value = "CHI TIẾT HỌC SINH";
                    ws.Range(1, 1, 1, 6).Merge().Style.Font.Bold = true;
                    ws.Cell(row++, 1).Value = $"Họ tên: {ThongTinChung.HoTen}";
                    ws.Cell(row++, 1).Value = $"Ngày sinh: {ThongTinChung.NgaySinh:dd/MM/yyyy}";
                    ws.Cell(row++, 1).Value = $"Lớp: {ThongTinChung.TenLop}";
                    ws.Cell(row++, 1).Value = $"Năm học: {ThongTinChung.NamHoc}";
                    row++;
                    ws.Cell(row++, 1).Value = "BẢNG ĐIỂM CÁC MÔN";
                    ws.Range(row-1, 1, row-1, 6).Merge().Style.Font.Bold = true;
                    // Header bảng điểm
                    ws.Cell(row, 1).Value = "Môn học";
                    ws.Cell(row, 2).Value = "Điểm TB HK1";
                    ws.Cell(row, 3).Value = "Điểm TB HK2";
                    ws.Cell(row, 4).Value = "Điểm TB cả năm";
                    ws.Range(row, 1, row, 4).Style.Font.Bold = true;
                    int firstDataRow = ++row;
                    foreach (var item in BangDiemMonHoc)
                    {
                        ws.Cell(row, 1).Value = item.MonHoc;
                        ws.Cell(row, 2).Value = item.DiemTBHK1;
                        ws.Cell(row, 3).Value = item.DiemTBHK2;
                        ws.Cell(row, 4).Value = item.DiemTBCaNam;
                        row++;
                    }
                    row++;
                    ws.Cell(row++, 1).Value = "TỔNG HỢP";
                    ws.Range(row-1, 1, row-1, 6).Merge().Style.Font.Bold = true;
                    ws.Cell(row, 1).Value = "Điểm trung bình chung:";
                    ws.Cell(row, 2).Value = DiemTrungBinhChung;
                    ws.Cell(row, 3).Value = "Xếp loại:";
                    ws.Cell(row, 4).Value = XepLoai;
                    row++;
                    ws.Cell(row, 1).Value = "Kết quả cuối năm:";
                    ws.Cell(row, 2).Value = KetQuaCuoiNam;
                    ws.Columns().AdjustToContents();
                    workbook.SaveAs(dialog.FileName);
                }
                MessageBox.Show("Xuất Excel thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
