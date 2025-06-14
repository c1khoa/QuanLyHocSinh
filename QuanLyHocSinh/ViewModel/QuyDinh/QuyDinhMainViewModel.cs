using QuanLyHocSinh.Model.Entities;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows; 

namespace QuanLyHocSinh.ViewModel.QuyDinh
{
    public class QuyDinhMainViewModel : BaseViewModel
    {
        private MainViewModel _mainVM;

        public QuyDinhMainViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM;

            Regulations = new ObservableCollection<Model.Entities.QuyDinh>();
            NewQuyDinh = new Model.Entities.QuyDinh();

            AddRegulationCommand = new RelayCommand(AddRegulation, CanAddRegulation);
            EditRegulationCommand = new RelayCommand(EditRegulation, CanEditRegulation);
            DeleteRegulationCommand = new RelayCommand<Model.Entities.QuyDinh>(DeleteRegulation);
        }

        private ObservableCollection<Model.Entities.QuyDinh> _regulations;
        public ObservableCollection<Model.Entities.QuyDinh> Regulations
        {
            get => _regulations;
            set
            {
                _regulations = value;
                OnPropertyChanged();
            }
        }

        private Model.Entities.QuyDinh _newQuyDinh;
        public Model.Entities.QuyDinh NewQuyDinh
        {
            get => _newQuyDinh;
            set
            {
                _newQuyDinh = value;
                OnPropertyChanged();
            }
        }

        private Model.Entities.QuyDinh _selectedRegulation;
        public Model.Entities.QuyDinh SelectedRegulation
        {
            get => _selectedRegulation;
            set
            {
                _selectedRegulation = value;
                OnPropertyChanged();

                // Cập nhật lại trạng thái nút Sửa
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public ICommand AddRegulationCommand { get; set; }
        public ICommand EditRegulationCommand { get; set; }
        public ICommand DeleteRegulationCommand { get; set; }
        private bool CanAddRegulation()
        {
            return NewQuyDinh != null && NewQuyDinh.TuoiToiThieu > 0;
        }

        private void AddRegulation()
        {
            var newItem = new Model.Entities.QuyDinh()
            {
                QuyDinhID = (Regulations.Count + 1).ToString(),
                TuoiToiThieu = NewQuyDinh.TuoiToiThieu,
                TuoiToiDa = NewQuyDinh.TuoiToiDa,
                SiSoLop = NewQuyDinh.SiSoLop,
                SoLuongMonHoc = NewQuyDinh.SoLuongMonHoc,
                DiemDat = NewQuyDinh.DiemDat,
                QuyDinhKhac = NewQuyDinh.QuyDinhKhac
            };

            Regulations.Add(newItem);

            NewQuyDinh = new Model.Entities.QuyDinh();

            CommandManager.InvalidateRequerySuggested();
        }

        private void DeleteRegulation(Model.Entities.QuyDinh quyDinh)
        {
            if (quyDinh == null) return;

            Regulations.Remove(quyDinh);
            CommandManager.InvalidateRequerySuggested();
        }
        private bool CanEditRegulation()
        {
            return SelectedRegulation != null;
        }

        public event Action<Model.Entities.QuyDinh> OpenEditViewRequested;
        private void EditRegulation()
        {
            if (SelectedRegulation == null) return;

            OpenEditViewRequested?.Invoke(SelectedRegulation);
        }
    }
}
