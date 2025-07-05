using QuanLyHocSinh.Model.DAL; // Đảm bảo bạn đã có các lớp DAL
using QuanLyHocSinh.Model.Entities;
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


        private void Save()
        {
            try
            {
                // Cập nhật các quy định chung (giữ nguyên như cũ)
                var quyDinh = QuyDinhDAL.GetQuyDinh();
                if (quyDinh != null)
                {
                    quyDinh.SoLuongMonHoc = this.SoLuongMonHoc;
                    quyDinh.DiemDat = this.DiemDat;
                    QuyDinhDAL.UpdateQuyDinh(quyDinh);
                }

                // === THÊM DÒNG NÀY ĐỂ LƯU TÊN MÔN HỌC ===
                MonHocDAL.UpdateDanhSachMonHoc(DanhSachMonHoc.ToList());
                // ==========================================
                
                MessageBox.Show("Cập nhật quy định môn học thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                _window.DialogResult = true;
                _window.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Đã có lỗi xảy ra khi cập nhật: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel()
        {
            _window.DialogResult = false;
            _window.Close();
        }
    }
}