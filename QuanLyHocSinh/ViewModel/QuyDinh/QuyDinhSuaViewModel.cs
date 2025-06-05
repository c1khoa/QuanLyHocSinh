using QuanLyHocSinh.Model.Entities; 
using System.Windows.Input;
using QuanLyHocSinh.ViewModel; 
using System.Windows; 

namespace QuanLyHocSinh.ViewModel
{
    public class QuyDinhSuaViewModel : BaseViewModel
    {
        private MainViewModel _mainVM;

        private QuanLyHocSinh.Model.Entities.QuyDinh _currentQuyDinh; 

        public QuyDinhSuaViewModel(MainViewModel mainVM, QuanLyHocSinh.Model.Entities.QuyDinh quyDinh)
        {
            _mainVM = mainVM;
            _currentQuyDinh = quyDinh;

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

            MessageBox.Show("Lưu thay đổi thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            Back(); // Gọi Back sau khi lưu
        }

        private void Back()
        {
            _mainVM.NavigateBack();
        }
    }
}
