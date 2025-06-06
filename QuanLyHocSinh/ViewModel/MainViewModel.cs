using QuanLyHocSinh.Model.Entities;
using QuanLyHocSinh.View.Converters;
using QuanLyHocSinh.View.Windows;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

using QuanLyHocSinh.View.Controls;
using QuanLyHocSinh.View.Controls.BaoCao;
using QuanLyHocSinh.View.Controls.QuanLyTaiKhoan;
using QuanLyHocSinh.View.Controls.QuyDinh;
using QuanLyHocSinh.View.Controls.TraCuu;
using QuanLyHocSinh.ViewModel.BaoCao;
using QuanLyHocSinh.ViewModel.QuanLyTaiKhoan;
using QuanLyHocSinh.ViewModel.QuyDinh;
using QuanLyHocSinh.ViewModel.TraCuu;

namespace QuanLyHocSinh.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        #region Fields & Properties


        // Các ViewModel con dùng cho điều hướng
        public TrangChuViewModel TrangChuVM { get; set; }
        public QuanLyTaiKhoanMainViewModel TaiKhoanVM { get; set; }
        public TraCuuHocSinhViewModel HocSinhVM { get; set; }
        public TraCuuGiaoVienViewModel GiaoVienVM { get; set; }
        public TongKetMonViewModel TongKetMonVM { get; set; }
        public QuyDinhMainViewModel QuyDinhVM { get; set; }

        private BaseViewModel _currentView;
        /// <summary>
        /// ViewModel đang được hiển thị hiện tại
        /// </summary>
        public BaseViewModel CurrentView
        {
            get => _currentView;
            set
            {
                if (_currentView != value)
                {
                    _currentView = value;
                    System.Diagnostics.Debug.WriteLine("CurrentView changed to: " + value?.GetType().Name);
                    OnPropertyChanged(nameof(CurrentView));
                }
            }
        }

        // Người dùng hiện tại
        private User? _currentUser;
        public User? CurrentUser
        {
            get => _currentUser;
            set
            {
                if (_currentUser != value)
                {
                    _currentUser = value;
                    OnPropertyChanged(nameof(CurrentUser));

                    var roleName = _currentUser?.VaiTro?.TenVaiTro?.Trim() ?? "null";

                    // Cập nhật UI theo vai trò
                    IsGiaoVuVisible = string.Equals(roleName, "Giáo vụ", StringComparison.OrdinalIgnoreCase);
                    IsNotHocSinhVisible = _currentUser != null && !string.Equals(roleName, "Học sinh", StringComparison.OrdinalIgnoreCase);
                    IsNotGiaoVuVisible = _currentUser != null && !string.Equals(roleName, "Giáo vụ", StringComparison.OrdinalIgnoreCase);
                }
            }
        }

        private string _selectedRole = string.Empty;
        public string SelectedRole
        {
            get => _selectedRole;
            set => SetProperty(ref _selectedRole, value);
        }

        // Các cờ điều khiển hiển thị UI theo vai trò người dùng
        private bool _isGiaoVuVisible;
        public bool IsGiaoVuVisible
        {
            get => _isGiaoVuVisible;
            set => SetProperty(ref _isGiaoVuVisible, value);
        }

        private bool _isNotHocSinhVisible;
        public bool IsNotHocSinhVisible
        {
            get => _isNotHocSinhVisible;
            set => SetProperty(ref _isNotHocSinhVisible, value);
        }

        private bool _isNotGiaoVuVisible;
        public bool IsNotGiaoVuVisible
        {
            get => _isNotGiaoVuVisible;
            set => SetProperty(ref _isNotGiaoVuVisible, value);
        }

        public bool Isloaded { get; set; } = false;


        // Danh sách role dùng để chọn ban đầu
        public ObservableCollection<BeginViewModel> Roles { get; set; }

        public BeginViewModel StudentRole { get; set; }
        public BeginViewModel TeacherRole { get; set; }
        public BeginViewModel AdminRole { get; set; }

        #endregion

        #region Commands

        public ICommand LoadedWindowCommand { get; set; }


        // Commands điều hướng các ViewModel
        public ICommand ShowTrangChuCommand { get; set; }
        public ICommand ShowTaiKhoanCaNhanCommand { get; set; }
        public ICommand ShowQuanLyTaiKhoanCommand { get; set; }
        public ICommand ShowThongTinHocSinhCommand { get; set; }
        public ICommand ShowThongTinGiaoVienCommand { get; set; }
        public ICommand ShowTongKetMonCommand { get; set; }
        public ICommand ShowTongKetNamCommand { get; set; }
        public ICommand ShowQuyDinhCommand { get; set; }

        // Command chọn vai trò mở LoginWindow
        public ICommand SelectRoleCommand { get; set; }

        #endregion

        #region Constructor

        public MainViewModel()
        {
            // Khởi tạo vai trò để hiển thị lựa chọn
            StudentRole = new BeginViewModel("pack://application:,,,/QuanLyHocSinh;component/Images/student_logo.png", "Học sinh");
            TeacherRole = new BeginViewModel("pack://application:,,,/QuanLyHocSinh;component/Images/teacher_logo.png", "Giáo viên");
            AdminRole = new BeginViewModel("pack://application:,,,/QuanLyHocSinh;component/Images/admin_logo.png", "Giáo vụ");

            Roles = new ObservableCollection<BeginViewModel> { StudentRole, TeacherRole, AdminRole };

            // View mặc định
            CurrentView = new TrangChuViewModel(this);

            // Khởi tạo các command điều hướng ViewModel
            ShowTrangChuCommand = new RelayCommand<object>((p) => true, (p) => CurrentView = new TrangChuViewModel(this));
            ShowQuanLyTaiKhoanCommand = new RelayCommand<object>((p) => true, (p) => CurrentView = new QuanLyTaiKhoanMainViewModel(this));
            ShowTaiKhoanCaNhanCommand = new RelayCommand<object>((p) => true, (p) => CurrentView = new QuanLyTaiKhoanCaNhanViewModel(this));
            ShowThongTinHocSinhCommand = new RelayCommand<object>((p) => true, (p) => CurrentView = new TraCuuHocSinhViewModel(this));
            ShowThongTinGiaoVienCommand = new RelayCommand<object>((p) => true, (p) => CurrentView = new TraCuuGiaoVienViewModel(this));
            ShowTongKetMonCommand = new RelayCommand<object>((p) => true, (p) => CurrentView = new TongKetMonViewModel(this));
            ShowTongKetNamCommand = new RelayCommand<object>((p) => true, (p) => CurrentView = new TongKetNamViewModel(this));
            ShowQuyDinhCommand = new RelayCommand<object>((p) => true, (p) => CurrentView = new QuyDinhMainViewModel(this));

            // Command xử lý chọn role để mở LoginWindow
            SelectRoleCommand = new RelayCommand<object>(
                parameter => parameter is BeginViewModel,
                parameter =>
                {
                    if (parameter is BeginViewModel selectedRole)
                        OpenLoginWindow(selectedRole.RoleName);
                });
        }

        #endregion

        #region Navigation Methods

        private Stack<BaseViewModel> _navigationStack = new Stack<BaseViewModel>();

        public void NavigateTo(BaseViewModel viewModel)
        {
            _navigationStack.Push(CurrentView);
            CurrentView = viewModel;
        }

        public void NavigateBack()
        {
            if (_navigationStack.Count > 0)
                CurrentView = _navigationStack.Pop();
            else
                CurrentView = new TrangChuViewModel(this);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Mở cửa sổ đăng nhập tương ứng với role được chọn
        /// </summary>
        private void OpenLoginWindow(string roleName)
        {
            var loginVM = new LoginViewModel { SelectedRole = roleName };
            var loginWindow = new LoginWindow { DataContext = loginVM };

            loginWindow.Loaded += (_, _) => WindowAnimationHelper.FadeIn(loginWindow);
            loginWindow.Show();

            // Đóng BeginWindow (nếu có)
            foreach (Window window in Application.Current.Windows)
            {
                if (window is BeginWindow beginWindow)
                {
                    WindowAnimationHelper.FadeOut(beginWindow);
                    break;
                }
            }

            // Khi đăng nhập thành công
            loginVM.LoginSuccess += (_, user) =>
            {
                if (Application.Current.Resources["MainVM"] is MainViewModel mainVM)
                {
                    mainVM.CurrentUser = user;
                    mainVM.CurrentView = new TrangChuViewModel(mainVM);

                    var mainWindow = new MainWindow();
                    mainWindow.Show();

                    loginWindow.Close();
                }
            };
        }

        /// <summary>
        /// Hỗ trợ cập nhật property và thông báo thay đổi
        /// </summary>
        protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = "")
        {
            if (Equals(field, newValue)) return false;
            field = newValue;
            OnPropertyChanged(propertyName);
            return true;
        }

        #endregion
    }
}
