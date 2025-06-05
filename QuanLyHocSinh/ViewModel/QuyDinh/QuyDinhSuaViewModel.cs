using QuanLyHocSinh.Model.Entities; // Đảm bảo dòng này có
using System.Windows.Input;
using QuanLyHocSinh.ViewModel; // Thêm dòng này để sử dụng BaseViewModel và RelayCommand
using System.Windows; // Thêm để sử dụng MessageBox

namespace QuanLyHocSinh.ViewModel
{
    public class QuyDinhSuaViewModel : BaseViewModel
    {
        private MainViewModel _mainVM;

        // ĐÃ SỬA: Chỉ rõ namespace đầy đủ cho _currentQuyDinh
        private QuanLyHocSinh.Model.Entities.QuyDinh _currentQuyDinh; // Giữ tham chiếu đến đối tượng gốc

        // ĐÃ SỬA: Chỉ rõ namespace đầy đủ cho tham số quyDinh trong constructor
        public QuyDinhSuaViewModel(MainViewModel mainVM, QuanLyHocSinh.Model.Entities.QuyDinh quyDinh)
        {
            _mainVM = mainVM;
            _currentQuyDinh = quyDinh;

            // Khởi tạo các thuộc tính ViewModel từ đối tượng QuyDinh
            Title = quyDinh.Title;
            Description = quyDinh.Description;

            SaveCommand = new RelayCommand(Save);
            BackCommand = new RelayCommand(Back);
        }

        private string _title = string.Empty;
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }

        private string _description = string.Empty;
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged();
            }
        }

        public ICommand SaveCommand { get; }
        public ICommand BackCommand { get; }

        private void Save()
        {
            // Cập nhật trực tiếp vào đối tượng gốc
            _currentQuyDinh.Title = Title;
            _currentQuyDinh.Description = Description;

            // TODO: Nếu có gọi lưu về DB thì làm ở đây
            MessageBox.Show("Lưu thay đổi thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            Back(); // Gọi Back sau khi lưu
        }

        private void Back()
        {
            _mainVM.NavigateBack();
        }
    }
}