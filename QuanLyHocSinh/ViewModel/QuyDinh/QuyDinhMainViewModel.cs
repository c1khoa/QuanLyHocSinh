using QuanLyHocSinh.Model.Entities;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows;

namespace QuanLyHocSinh.ViewModel.QuyDinh
{
    public class QuyDinhMainViewModel : BaseViewModel
    {
        // Các thuộc tính chứa dữ liệu quy định
        private QuyDinhEntities _quyDinhChung;
        public QuyDinhEntities QuyDinhChung { get => _quyDinhChung; set { _quyDinhChung = value; OnPropertyChanged(); } }

        private QuyDinhTuoiEntities _quyDinhHocSinh;
        public QuyDinhTuoiEntities QuyDinhHocSinh { get => _quyDinhHocSinh; set { _quyDinhHocSinh = value; OnPropertyChanged(); } }

        private QuyDinhTuoiEntities _quyDinhGiaoVien;
        public QuyDinhTuoiEntities QuyDinhGiaoVien { get => _quyDinhGiaoVien; set { _quyDinhGiaoVien = value; OnPropertyChanged(); } }

        // Thuộc tính kiểm soát trạng thái chỉnh sửa chung
        private bool _isEditing;
        public bool IsEditing { get => _isEditing; set { _isEditing = value; OnPropertyChanged(); } }

        // Các lệnh (Commands)
        public ICommand LoadDataCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        public ICommand CancelCommand { get; set; }
        private MainViewModel _mainVM;

        public QuyDinhMainViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM;
            // Gán các lệnh với phương thức xử lý
            LoadDataCommand = new RelayCommand(LoadData);
            EditCommand = new RelayCommand(() => IsEditing = true);
            SaveCommand = new RelayCommand(SaveData);
            CancelCommand = new RelayCommand(() => {
                IsEditing = true;
                LoadData(); // Tải lại dữ liệu gốc khi hủy
            });

            LoadData(); // Tải dữ liệu ban đầu
        }

        // Tải dữ liệu từ DAL
        private void LoadData()
        {
            try
            {
                IsEditing = false;
                // Gọi các phương thức static từ DAL mà bạn đã cung cấp
                QuyDinhChung = QuyDinhDAL.GetQuyDinh();
                QuyDinhHocSinh = QuyDinhTuoiDAL.GetQuyDinhTuoi("QDHS");
                QuyDinhGiaoVien = QuyDinhTuoiDAL.GetQuyDinhTuoi("QDGV");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải dữ liệu quy định: " + ex.Message);
            }
        }

        // Lưu dữ liệu thông qua DAL
        private void SaveData()
        {
            try
            {
                // Gọi các phương thức static từ DAL để cập nhật
                QuyDinhDAL.UpdateQuyDinh(QuyDinhChung);
                QuyDinhTuoiDAL.UpdateQuyDinhTuoi(QuyDinhHocSinh);
                QuyDinhTuoiDAL.UpdateQuyDinhTuoi(QuyDinhGiaoVien);

                IsEditing = false;
                MessageBox.Show("Cập nhật quy định thành công!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi cập nhật quy định: " + ex.Message);
            }
        }
    }
}
