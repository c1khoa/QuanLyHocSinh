using QuanLyHocSinh.Model.DAL; // Đảm bảo đã using DAL
using QuanLyHocSinh.Model.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq; // Cần cho .ToList()
using System.Windows;
using System.Windows.Input;

namespace QuanLyHocSinh.ViewModel.QuyDinh
{
    public class SuaQuyDinhLopViewModel : BaseViewModel
    {
        // Thuộc tính cần OnPropertyChanged để binding
        private int _siSoLopToiDa;
        public int SiSoLopToiDa 
        {
            get => _siSoLopToiDa;
            set { _siSoLopToiDa = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Lop> DanhSachLop { get; set; }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        private Window _window;

        public SuaQuyDinhLopViewModel(Window window, int siSoLopToiDa, IList<Lop> danhSachLop)
        {
            _window = window;
            SiSoLopToiDa = siSoLopToiDa;
            DanhSachLop = new ObservableCollection<Lop>(danhSachLop);

            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(Cancel);
        }

        private void Save()
        {
            try
            {
                // Cập nhật sĩ số tối đa (giữ nguyên)
                var quyDinh = QuyDinhDAL.GetQuyDinh();
                if (quyDinh != null)
                {
                    quyDinh.SiSoLop_ToiDa = this.SiSoLopToiDa;
                    QuyDinhDAL.UpdateQuyDinh(quyDinh);
                }

                // Cập nhật danh sách tên lớp (giờ đây là danh sách các bản sao)
                // Hàm này sẽ cập nhật các bản sao này xuống CSDL
                LopDAL.UpdateDanhSachLop(DanhSachLop.ToList());
                
                MessageBox.Show("Cập nhật quy định lớp thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

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