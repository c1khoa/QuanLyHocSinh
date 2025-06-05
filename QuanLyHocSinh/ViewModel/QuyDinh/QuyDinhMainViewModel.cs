using QuanLyHocSinh.Model.Entities;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using QuanLyHocSinh.ViewModel; // Thêm dòng này để sử dụng BaseViewModel và RelayCommand
using System.Windows; // Thêm để sử dụng MessageBox

// Đặt alias để tránh nhầm giữa namespace và class
using QuyDinhModel = QuanLyHocSinh.Model.Entities.QuyDinh;

namespace QuanLyHocSinh.ViewModel.QuyDinh
{
    public class QuyDinhMainViewModel : BaseViewModel
    {
        private MainViewModel _mainVM;

        public QuyDinhMainViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM;
            ListQuyDinh = new ObservableCollection<QuyDinhModel>();

            // Khởi tạo commands với RelayCommand chuẩn (đã sửa đổi)
            AddCommand = new RelayCommand(ThemQuyDinh); // canExecute mặc định là true
            UpdateCommand = new RelayCommand(CapNhatQuyDinh, () => SelectedQuyDinh != null);
            EditCommand = new RelayCommand(XemChiTiet, () => SelectedQuyDinh != null);
            ClearCommand = new RelayCommand(LamMoi); // canExecute mặc định là true

            // Load dữ liệu ban đầu (ví dụ)
            LoadQuyDinhData();
        }

        private string _title = string.Empty; // Khởi tạo với string.Empty để tránh lỗi null
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }

        private string _description = string.Empty; // Khởi tạo với string.Empty để tránh lỗi null
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged();
            }
        }

        private QuyDinhModel? _selectedQuyDinh; // Cho phép null
        public QuyDinhModel? SelectedQuyDinh
        {
            get => _selectedQuyDinh;
            set
            {
                _selectedQuyDinh = value;
                OnPropertyChanged();
                // Cập nhật các trường Title và Description khi SelectedQuyDinh thay đổi
                if (value != null)
                {
                    Title = value.Title;
                    Description = value.Description;
                }
                else
                {
                    // Xóa các trường khi không có QuyDinh nào được chọn
                    Title = string.Empty;
                    Description = string.Empty;
                }
                // Yêu cầu CommandManager đánh giá lại trạng thái CanExecute của các command liên quan
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public ObservableCollection<QuyDinhModel> ListQuyDinh { get; set; }

        public ICommand AddCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand ClearCommand { get; }

        private void LoadQuyDinhData()
        {
            // Đây là nơi bạn sẽ tải dữ liệu quy định từ cơ sở dữ liệu
            // Hiện tại, chỉ thêm dữ liệu giả định
            ListQuyDinh.Add(new QuyDinhModel { QuyDinhID = "QD000001", Title = "Quy định tuổi học sinh", Description = "Quy định về độ tuổi tối thiểu và tối đa của học sinh." });
            ListQuyDinh.Add(new QuyDinhModel { QuyDinhID = "QD000002", Title = "Quy định sĩ số lớp", Description = "Quy định số lượng học sinh tối đa trong một lớp học." });
            ListQuyDinh.Add(new QuyDinhModel { QuyDinhID = "QD000003", Title = "Quy định điểm đạt", Description = "Điểm trung bình tối thiểu để đạt môn học." });
        }

        private void ThemQuyDinh()
        {
            if (string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(Description))
            {
                // Có thể hiển thị thông báo lỗi cho người dùng
                MessageBox.Show("Vui lòng nhập đầy đủ Tiêu đề và Mô tả.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var newQuyDinh = new QuyDinhModel
            {
                QuyDinhID = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper(), // Tạo ID duy nhất
                Title = Title,
                Description = Description
            };
            ListQuyDinh.Add(newQuyDinh);
            // TODO: Gọi hàm lưu vào cơ sở dữ liệu ở đây
            LamMoi();
            MessageBox.Show("Thêm quy định thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CapNhatQuyDinh()
        {
            if (SelectedQuyDinh != null) // Đảm bảo SelectedQuyDinh không null
            {
                if (string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(Description))
                {
                    MessageBox.Show("Vui lòng nhập đầy đủ Tiêu đề và Mô tả.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Cập nhật các thuộc tính của đối tượng được chọn
                SelectedQuyDinh.Title = Title;
                SelectedQuyDinh.Description = Description;

                // Tìm vị trí của đối tượng trong collection
                // Sử dụng toán tử ! (null-forgiving) để báo cho trình biên dịch rằng _selectedQuyDinh không null ở đây
                var index = ListQuyDinh.IndexOf(_selectedQuyDinh!);
                if (index != -1)
                {
                    // Gán lại đối tượng để ObservableCollection kích hoạt sự kiện thay đổi, làm mới UI
                    ListQuyDinh[index] = SelectedQuyDinh;
                }

                // TODO: Gọi hàm lưu cập nhật vào cơ sở dữ liệu ở đây
                MessageBox.Show("Cập nhật quy định thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void XemChiTiet()
        {
            if (SelectedQuyDinh != null)
            {
                // NavigateTo sẽ đẩy CurrentView hiện tại vào stack và chuyển đến QuyDinhSuaViewModel
                _mainVM.NavigateTo(new QuyDinhSuaViewModel(_mainVM, SelectedQuyDinh));
            }
        }

        private void LamMoi()
        {
            Title = string.Empty;
            Description = string.Empty;
            SelectedQuyDinh = null; // Bỏ chọn QuyDinh
        }
    }
}