using QuanLyHocSinh.Model.Entities;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace QuanLyHocSinh.ViewModel.DanhSachLop
{
    public class DanhSachLopViewModel : BaseViewModel
    {
        private readonly LopDAL _lopDAL;
        private readonly MainViewModel _mainVM;

        #region Properties

        // Danh sách các lớp học hiển thị (sau khi lọc)
        private ObservableCollection<Lop> _danhSachLop;
        public ObservableCollection<Lop> DanhSachLop
        {
            get => _danhSachLop;
            set => SetProperty(ref _danhSachLop, value);
        }

        // Danh sách lớp gốc (không lọc)
        private ObservableCollection<Lop> _allLop = new ObservableCollection<Lop>();

        // Lớp đang được chọn
        private Lop _selectedLop;
        public Lop SelectedLop
        {
            get => _selectedLop;
            set
            {
                if (SetProperty(ref _selectedLop, value))
                {
                    // Có thể thực hiện logic khi chọn lớp ở đây nếu cần
                }
            }
        }

        // Cờ cho biết đang tải dữ liệu
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        // Thuộc tính cho thanh tìm kiếm
        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    FilterDanhSachLop();
                }
            }
        }

        #endregion

        #region Commands

        public ICommand LoadLopCommand { get; }
        public ICommand SelectLopCommand { get; }

        #endregion

        public DanhSachLopViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM ?? throw new ArgumentNullException(nameof(mainVM));
            _lopDAL = new LopDAL();
            DanhSachLop = new ObservableCollection<Lop>();

            // Khởi tạo các Command
            LoadLopCommand = new RelayCommand(async () => await LoadAllLopAsync());
            SelectLopCommand = new RelayCommand<Lop>(async (lop) => await ExecuteSelectLop(lop));

            // Tải dữ liệu ban đầu
            LoadLopCommand.Execute(null);
        }

        #region Methods

        /// <summary>
        /// Tải toàn bộ danh sách lớp từ cơ sở dữ liệu và lưu vào _allLop.
        /// </summary>
        private async Task LoadAllLopAsync()
        {
            IsLoading = true;
            try
            {
                var lops = await _lopDAL.GetAllLopAsync();
                _allLop = new ObservableCollection<Lop>(lops);
                FilterDanhSachLop();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách lớp: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Lọc danh sách lớp theo SearchText (tên lớp hoặc mã lớp).
        /// </summary>
        private void FilterDanhSachLop()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                DanhSachLop = new ObservableCollection<Lop>(_allLop);
            }
            else
            {
                var lower = SearchText.ToLower();
                DanhSachLop = new ObservableCollection<Lop>(
                    _allLop.Where(l =>
                        (!string.IsNullOrEmpty(l.TenLop) && l.TenLop.ToLower().Contains(lower)) ||
                        (!string.IsNullOrEmpty(l.LopID) && l.LopID.ToLower().Contains(lower))
                    )
                );
            }
        }

        /// <summary>
        /// Mở window danh sách học sinh của lớp khi bấm nút chức năng.
        /// </summary>
        private async Task ExecuteSelectLop(Lop selectedLop)
        {
            if (selectedLop != null)
            {
                var hocSinhs = await _lopDAL.GetHocSinhsWithDiemByLopIDAsync(selectedLop.LopID);
                var window = new QuanLyHocSinh.View.DanhSachHocSinhWindow(selectedLop, hocSinhs);
                window.ShowDialog();
            }
        }

        #endregion
    }
}