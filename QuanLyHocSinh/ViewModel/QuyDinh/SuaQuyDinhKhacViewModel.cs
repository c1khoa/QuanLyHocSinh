using QuanLyHocSinh.Model.DAL; // Đảm bảo bạn đã có các lớp DAL
using QuanLyHocSinh.Model.Entities;
using System;
using System.Windows;
using System.Windows.Input;

namespace QuanLyHocSinh.ViewModel.QuyDinh
{
    public class SuaQuyDinhKhacViewModel : BaseViewModel
    {
        // Thuộc tính để lưu trữ chuỗi quy định
        private string _quyDinhKhac;
        public string QuyDinhKhac
        {
            get => _quyDinhKhac;
            set { _quyDinhKhac = value; OnPropertyChanged(); }
        }

        // Tham chiếu đến cửa sổ dialog để có thể đóng nó
        private Window _dialog;

        // Các lệnh (Commands)
        public ICommand SaveCommand { get; set; }
        public ICommand CancelCommand { get; set; }

        public SuaQuyDinhKhacViewModel(Window dialog, string currentQuyDinhKhac)
        {
            _dialog = dialog;
            QuyDinhKhac = currentQuyDinhKhac; // Nhận giá trị hiện tại

            // Khởi tạo các lệnh
            SaveCommand = new RelayCommand(SaveChanges);
            CancelCommand = new RelayCommand(() => _dialog.Close()); // Chỉ cần đóng dialog
        }

        private void SaveChanges()
        {
            try
            {
                // Bước 1: Lấy toàn bộ đối tượng QuyDinh hiện tại từ CSDL
                // Vì hàm UpdateQuyDinh cần một đối tượng QuyDinh hoàn chỉnh
                var currentQuyDinh = QuyDinhDAL.GetQuyDinh();
                if (currentQuyDinh == null)
                {
                    MessageBox.Show("Không tìm thấy dữ liệu quy định gốc.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Bước 2: Cập nhật chỉ thuộc tính QuyDinhKhac
                currentQuyDinh.QuyDinhKhac = this.QuyDinhKhac;

                // Bước 3: Gọi DAL để lưu lại toàn bộ đối tượng đã cập nhật
                QuyDinhDAL.UpdateQuyDinh(currentQuyDinh);

                MessageBox.Show("Cập nhật quy định thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                // Bước 4: Đóng dialog và trả về kết quả true
                _dialog.DialogResult = true;
                _dialog.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Đã xảy ra lỗi khi cập nhật: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}