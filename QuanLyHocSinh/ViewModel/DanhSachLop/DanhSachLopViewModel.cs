using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using QuanLyHocSinh.Model.Entities;
using QuanLyHocSinh.Model.DAL;
using QuanLyHocSinh.ViewModel;
using QuanLyHocSinh.View.Dialogs;
using QuanLyHocSinh.View.Dialogs.MessageBox;
using ClosedXML.Excel;
using System.IO;
using Microsoft.Win32;
using MaterialDesignThemes.Wpf;

namespace QuanLyHocSinh.ViewModel.DanhSachLop
{
    public class DanhSachLopViewModel : BaseViewModel
    {
        private ObservableCollection<HocSinhLopItem> _danhSachHocSinhLop;
        public ObservableCollection<HocSinhLopItem> DanhSachHocSinhLop
        {
            get => _danhSachHocSinhLop;
            set { _danhSachHocSinhLop = value; OnPropertyChanged(nameof(DanhSachHocSinhLop)); }
        }

        private ObservableCollection<string> _danhSachLop;
        public ObservableCollection<string> DanhSachLop
        {
            get => _danhSachLop;
            set { _danhSachLop = value; OnPropertyChanged(nameof(DanhSachLop)); }
        }

        private string _selectedLop;
        public string SelectedLop
        {
            get => _selectedLop;
            set 
            { 
                _selectedLop = value; 
                OnPropertyChanged(nameof(SelectedLop)); 
                LoadDanhSachHocSinhLop();
                UpdateSiSoLop();
            }
        }

        private int _siSoLop;
        public int SiSoLop
        {
            get => _siSoLop;
            set { _siSoLop = value; OnPropertyChanged(nameof(SiSoLop)); }
        }

        private HocSinhLopItem _selectedHocSinh;
        public HocSinhLopItem SelectedHocSinh
        {
            get => _selectedHocSinh;
            set { _selectedHocSinh = value; OnPropertyChanged(nameof(SelectedHocSinh)); }
        }

        public ICommand SuaHocSinhCommand { get; }
        public ICommand ExportExcelCommand { get; }

        private MainViewModel _mainVM;
        public DanhSachLopViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM;
            
            SuaHocSinhCommand = new RelayCommand<HocSinhLopItem>(SuaHocSinh);
            ExportExcelCommand = new RelayCommand<object>((p) => !string.IsNullOrEmpty(SelectedLop), (p) => ExportExcel());
            
            LoadDanhSachLop();
        }

        private void LoadDanhSachLop()
        {
            List<string> danhSachLop;

            if (_mainVM.CurrentUser.VaiTro.VaiTroID == "VT02")
            {   
                danhSachLop = GiaoVienDAL.GetLopDayCuaUser(_mainVM.CurrentUser.UserID);
            }
            else if (_mainVM.CurrentUser.VaiTro.VaiTroID == "VT01")
            {
                danhSachLop = HocSinhDAL.GetLopHocCuaUser(_mainVM.CurrentUser.UserID);
            }
            else
            {
                danhSachLop = HocSinhDAL.GetAllLop();
            }

            DanhSachLop = new ObservableCollection<string>(danhSachLop.OrderBy(l => l));
            
            if (DanhSachLop.Count > 0)
            {
                SelectedLop = DanhSachLop.First();
            }
        }

        private void LoadDanhSachHocSinhLop()
        {
            if (string.IsNullOrEmpty(SelectedLop))
            {
                DanhSachHocSinhLop = new ObservableCollection<HocSinhLopItem>();
                return;
            }

            var danhSach = HocSinhDAL.GetDanhSachHocSinhTheoLop(SelectedLop);
            DanhSachHocSinhLop = new ObservableCollection<HocSinhLopItem>(danhSach);
        }

        private void UpdateSiSoLop()
        {
            if (string.IsNullOrEmpty(SelectedLop))
            {
                SiSoLop = 0;
                return;
            }

            SiSoLop = HocSinhDAL.GetSiSoLop(SelectedLop);
        }

        private void SuaHocSinh(HocSinhLopItem hocSinhItem)
        {
            if (hocSinhItem == null) return;

            var hocSinh = new HocSinh
            {
                HocSinhID = hocSinhItem.HocSinhID,
                HoTen = hocSinhItem.HoTen,
                GioiTinh = hocSinhItem.GioiTinh,
                NgaySinh = hocSinhItem.NgaySinh,
                Email = hocSinhItem.Email,
                DiaChi = hocSinhItem.DiaChi,
                TenLop = SelectedLop,
                NienKhoa = hocSinhItem.NienKhoa
            };

            var suaHSDialog = new SuaHocSinhDialog(hocSinh);
            var result = suaHSDialog.ShowDialog();

            if (result == true)
            {
                LoadDanhSachHocSinhLop();
            }
        }

        private async void ExportExcel()
        {
            try
            {
                if (DanhSachHocSinhLop == null || DanhSachHocSinhLop.Count == 0)
                {
                    await ShowNotificationAsync("Thông báo", "Không có dữ liệu để xuất!");
                    return;
                }

                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel files (*.xlsx)|*.xlsx",
                    Title = "Lưu danh sách học sinh",
                    FileName = $"DanhSach_Lop_{SelectedLop}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("Danh sách học sinh");
    
                        worksheet.Range("A1:G1").Merge().Value = $"DANH SÁCH HỌC SINH LỚP {SelectedLop}";
                        worksheet.Range("A1:G1").Style.Font.FontSize = 16;
                        worksheet.Range("A1:G1").Style.Font.Bold = true;
                        worksheet.Range("A1:G1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        worksheet.Range("A2:G2").Merge().Value = $"Sỉ số: {SiSoLop} học sinh - Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm}";
                        worksheet.Range("A2:G2").Style.Font.FontSize = 11;
                        worksheet.Range("A2:G2").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        worksheet.Cell(4, 1).Value = "STT";
                        worksheet.Cell(4, 2).Value = "Họ và tên";
                        worksheet.Cell(4, 3).Value = "Giới tính";
                        worksheet.Cell(4, 4).Value = "Ngày sinh";
                        worksheet.Cell(4, 5).Value = "Email";
                        worksheet.Cell(4, 6).Value = "Địa chỉ";
                        worksheet.Cell(4, 7).Value = "Niên khóa";

                        var headerRange = worksheet.Range("A4:G4");
                        headerRange.Style.Font.Bold = true;
                        headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
                        headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thick;
                        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        int row = 5;
                        foreach (var hocSinh in DanhSachHocSinhLop)
                        {
                            worksheet.Cell(row, 1).Value = hocSinh.STT;
                            worksheet.Cell(row, 2).Value = hocSinh.HoTen;
                            worksheet.Cell(row, 3).Value = hocSinh.GioiTinh;
                            worksheet.Cell(row, 4).Value = hocSinh.NgaySinh.ToString("dd/MM/yyyy");
                            worksheet.Cell(row, 5).Value = hocSinh.Email;
                            worksheet.Cell(row, 6).Value = hocSinh.DiaChi;
                            worksheet.Cell(row, 7).Value = hocSinh.NienKhoa;

                            if (row % 2 == 0)
                            {
                                worksheet.Range(row, 1, row, 7).Style.Fill.BackgroundColor = XLColor.LightGray;
                            }

                            row++;
                        }

                        var dataRange = worksheet.Range(4, 1, row - 1, 7);
                        dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                        dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Columns().AdjustToContents();

                        worksheet.Range(5, 1, row - 1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center; // STT
                        worksheet.Range(5, 3, row - 1, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center; // Giới tính
                        worksheet.Range(5, 4, row - 1, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center; // Ngày sinh
                        worksheet.Range(5, 7, row - 1, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center; // Niên khóa

                        workbook.SaveAs(saveFileDialog.FileName);
                    }

                    await ShowNotificationAsync("Thành công", $"Xuất Excel thành công!");
                }
            }
            catch (Exception ex)
            {
                await ShowNotificationAsync("Lỗi", $"Lỗi khi xuất Excel: {ex.Message}");
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
                System.Windows.MessageBox.Show(message, title);
            }
        }
    }
} 