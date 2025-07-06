using MaterialDesignThemes.Wpf;
using QuanLyHocSinh.Model.DAL; // Đảm bảo bạn đã có các lớp DAL
using QuanLyHocSinh.Model.Entities;
using QuanLyHocSinh.View.Dialogs.MessageBox;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace QuanLyHocSinh.ViewModel.QuyDinh
{
    public class SuaQuyDinhMonHocViewModel : BaseViewModel
    {
        #region Properties
        // Các thuộc tính cần gọi OnPropertyChanged() để binding hoạt động
        private int _soLuongMonHoc;
        public int SoLuongMonHoc
        {
            get => _soLuongMonHoc;
            set { _soLuongMonHoc = value; OnPropertyChanged(); }
        }

        private float _diemDat;
        public float DiemDat
        {
            get => _diemDat;
            set { _diemDat = value; OnPropertyChanged(); }
        }

        private ObservableCollection<MonHoc> _danhSachMonHoc;
        public ObservableCollection<MonHoc> DanhSachMonHoc
        {
            get => _danhSachMonHoc;
            set { _danhSachMonHoc = value; OnPropertyChanged(); }
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        #endregion

        private Window _window;

        public SuaQuyDinhMonHocViewModel(Window window, int soLuongMonHoc, float diemDat, List<MonHoc> danhSachMonHoc)
        {
            _window = window;
            
            // Gán giá trị ban đầu
            SoLuongMonHoc = soLuongMonHoc;
            DiemDat = diemDat;
            DanhSachMonHoc = new ObservableCollection<MonHoc>(danhSachMonHoc);

            // Khởi tạo Command
            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(Cancel);
        }


        private async void Save()
        {
            if (SoLuongMonHoc <= 0)
            {
                await ShowError("Lỗi nhập liệu", "Số lượng môn học phải lớn hơn 0.");
                return;
            }

            if (DiemDat < 0 || DiemDat > 10)
            {
                await ShowError("Lỗi nhập liệu", "Điểm đạt phải nằm trong khoảng từ 0 đến 10.");
                return;
            }

            if (DanhSachMonHoc == null || DanhSachMonHoc.Count == 0)
            {
                await ShowError("Lỗi nhập liệu", "Danh sách môn học không được để trống.");
                return;
            }

            foreach (var monHoc in DanhSachMonHoc)
            {
                if (string.IsNullOrWhiteSpace(monHoc.TenMonHoc))
                {
                    await ShowError("Lỗi nhập liệu", "Tên môn học không được để trống.");
                    return;
                }
            }

            try
            {
                // Cập nhật các quy định chung
                var quyDinh = QuyDinhDAL.GetQuyDinh();
                if (quyDinh != null)
                {
                    quyDinh.SoLuongMonHoc = this.SoLuongMonHoc;
                    quyDinh.DiemDat = this.DiemDat;
                    QuyDinhDAL.UpdateQuyDinh(quyDinh);
                }

                // Cập nhật danh sách môn học
                MonHocDAL.UpdateDanhSachMonHoc(DanhSachMonHoc.ToList());
               
                await DialogHost.Show(new NotifyDialog("Thông báo", "Cập nhật quy định môn học thành công!"), "RootDialog_SuaMonHoc");
                _window.DialogResult = true;
                _window.Close();
            }
            catch (Exception ex)
            {
                await ShowError("Lỗi hệ thống", "Đã xảy ra lỗi khi cập nhật: " + ex.Message);
            }
        }

        private void Cancel()
        {
            _window.DialogResult = false;
            _window.Close();
        }
        private async Task ShowError(string title, string message)
        {
            try
            {
                await DialogHost.Show(new ErrorDialog(title, message), "RootDialog_SuaMonHoc");
            }
            catch
            {
                MessageBox.Show(message, title, MessageBoxButton.OK,
                    title.Contains("Lỗi") ? MessageBoxImage.Error : MessageBoxImage.Information);
            }
        }
    }
}