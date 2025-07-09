using System.Collections.ObjectModel;
using System.Windows.Input;
using QuanLyHocSinh.Model.Entities;
using QuanLyHocSinh.Model.DAL;
using Microsoft.Win32;
using System.IO;
using ClosedXML.Excel;
using System;
using System.Windows;
using MaterialDesignThemes.Wpf;
using QuanLyHocSinh.View.Dialogs.MessageBox;
using System.Linq;
using QuanLyHocSinh.View.RoleControls;

namespace QuanLyHocSinh.ViewModel.TraCuu
{
    public class TraCuuBangDiemHocSinhViewModel : BaseViewModel
    {
        public string HoTen { get; set; }
        public string TenLop { get; set; }
        public string NamHoc { get; set; }
        public int HocKy { get; set; }
        public string HocSinhID { get; set; }
        public ObservableCollection<TongKetMonItem> BangDiemMon { get; set; }
        public ICommand ExportExcelCommand { get; set; }
        
        public event System.Action RequestMainWindowRefresh;
        public event System.Action RequestDialogActivation;

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
        public bool IsGiaoVienVisible => _mainVM.IsGiaoVienVisible;

        private HocSinh _hocSinh;

        private MainViewModel _mainVM;
        public TraCuuBangDiemHocSinhViewModel(HocSinh hs, MainViewModel mainVM, string namHoc = null, int? hocKy = null)
        {
            _mainVM = mainVM;
            _hocSinh = hs;
            HoTen = hs.HoTen;
            TenLop = hs.TenLop;
            HocSinhID = hs.HocSinhID;
            
            var allNamHoc = HocSinhDAL.GetAllNamHoc();
            DanhSachNamHoc = new ObservableCollection<string>(allNamHoc);
            
            var allHocKy = HocSinhDAL.GetAllHocKy().Where(hk => hk > 0).ToList(); 
            DanhSachHocKy = new ObservableCollection<int>(allHocKy);
            
            if (DanhSachNamHoc.Count > 0)
            {
                SelectedNamHoc = DanhSachNamHoc.LastOrDefault(); 
            }
            
            if (DanhSachHocKy.Contains(2))
            {
                SelectedHocKy = 2; 
            }
            else if (DanhSachHocKy.Count > 0)
            {
                SelectedHocKy = DanhSachHocKy.FirstOrDefault(); 
            }
            
            BangDiemMon = null;
            IsShowBangDiem = false;
            ExportExcelCommand = new RelayCommand(ExportExcel);
            
            OnFilterChanged();
        }
        private void OnFilterChanged()
        {
            if (!string.IsNullOrEmpty(SelectedNamHoc) && SelectedHocKy.HasValue && SelectedHocKy.Value > 0)
            {
                NamHoc = SelectedNamHoc;
                HocKy = SelectedHocKy.Value;
                List<TongKetMonItem> list;
                if (_mainVM.CurrentUser.VaiTro.VaiTroID == "VT02")
                {
                    list = HocSinhDAL.GetBangDiemHocSinh_GiaoVien(HocSinhID, _mainVM.CurrentUser.GiaoVienID, NamHoc, HocKy);
                }
                else
                {
                    list = HocSinhDAL.GetBangDiemHocSinh(HocSinhID, NamHoc, HocKy);
                }
                    BangDiemMon = new ObservableCollection<TongKetMonItem>(list);
                IsShowBangDiem = BangDiemMon != null && BangDiemMon.Count > 0;
                OnPropertyChanged(nameof(BangDiemMon));
                OnPropertyChanged(nameof(NamHoc));
                OnPropertyChanged(nameof(HocKy));
            }
            else
            {
                BangDiemMon = null;
                IsShowBangDiem = false;
                OnPropertyChanged(nameof(BangDiemMon));
            }
        }
        private async void ExportExcel()
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

                        worksheet.Cell("A1").Value = "Họ tên:";
                        worksheet.Cell("B1").Value = HoTen;
                        worksheet.Cell("A2").Value = "Lớp:";
                        worksheet.Cell("B2").Value = TenLop;
                        worksheet.Cell("A3").Value = "Năm học:";
                        worksheet.Cell("B3").Value = NamHoc;
                        worksheet.Cell("A4").Value = "Học kỳ:";
                        worksheet.Cell("B4").Value = HocKy;

                        var headers = new[] { "Môn học", "Điểm miệng", "Điểm 15p", "Điểm 1 tiết", "Điểm thi", "Điểm TB", "Xếp loại" };
                        for (int i = 0; i < headers.Length; i++)
                        {
                            worksheet.Cell(6, i + 1).Value = headers[i];
                            worksheet.Cell(6, i + 1).Style.Font.Bold = true;
                        }

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

                        worksheet.Columns().AdjustToContents();

                        workbook.SaveAs(saveFileDialog.FileName);
                    }

                    await ShowNotificationAsync("Thông báo", "✅ Xuất Excel thành công!");
                    
                    // Activate dialog sau khi export để không bị ẩn đằng sau main window
                    RequestDialogActivation?.Invoke();
                }
            }
            catch (Exception ex)
            {
                await ShowNotificationAsync("Lỗi", $"❌ Lỗi khi xuất Excel: {ex.Message}");
                
                // Activate dialog sau khi hiển thị lỗi để không bị ẩn đằng sau main window
                RequestDialogActivation?.Invoke();
            }
        }
        private async void SaveChanges()
        {
            if (!IsShowBangDiem || BangDiemMon == null) return;

            try
            {
                foreach (var item in BangDiemMon)
                {
                    Diem diem = new Diem
                    {
                        MaHS = HocSinhID,
                        MonHoc = item.MonHoc,
                        NamHocID = NamHoc,
                        HocKy = HocKy,
                        DiemMieng = double.IsNaN(item.DiemMieng) ? -1f : (float)item.DiemMieng,
                        Diem15p = double.IsNaN(item.Diem15Phut) ? -1f : (float)item.Diem15Phut,
                        Diem1Tiet = double.IsNaN(item.Diem1Tiet) ? -1f : (float)item.Diem1Tiet,
                        DiemThi = double.IsNaN(item.DiemThi) ? -1f : (float)item.DiemThi
                    };
                    string errorMsg;
                    bool result = DiemDAL.UpdateDiem(diem, out errorMsg);
                }

                var list = HocSinhDAL.GetBangDiemHocSinh(HocSinhID, NamHoc, HocKy);
                BangDiemMon = new ObservableCollection<TongKetMonItem>(list);
                OnPropertyChanged(nameof(BangDiemMon));

                await ShowNotificationAsync("Thông báo", "✅ Lưu thay đổi thành công!");
            }
            catch (Exception ex)
            {
                await ShowNotificationAsync("Lỗi", $"❌ Lỗi khi lưu thay đổi: {ex.Message}");
            }
        }
        public void RefreshBangDiem()
        {
            if (!string.IsNullOrEmpty(NamHoc) && HocKy > 0)
            {
                var list = HocSinhDAL.GetBangDiemHocSinh(HocSinhID, NamHoc, HocKy);
                BangDiemMon = new ObservableCollection<TongKetMonItem>(list);
                IsShowBangDiem = BangDiemMon != null && BangDiemMon.Count > 0;
                OnPropertyChanged(nameof(BangDiemMon));
            }
        }
        
        public void NotifyMainWindowRefresh()
        {
            RequestMainWindowRefresh?.Invoke();
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
