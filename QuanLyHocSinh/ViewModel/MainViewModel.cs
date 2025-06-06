using QuanLyHocSinh.Model.Entities;
using QuanLyHocSinh.View.Converters;
using QuanLyHocSinh.View.Windows;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

using QuanLyHocSinh.Model.Entities;
using QuanLyHocSinh.View.Controls;
using QuanLyHocSinh.View.Controls.BaoCao;
using QuanLyHocSinh.View.Controls.QuanLyTaiKhoan;
using QuanLyHocSinh.View.Controls.QuyDinh;
using QuanLyHocSinh.View.Controls.TraCuu;
using QuanLyHocSinh.View.Converters;
using QuanLyHocSinh.View.Windows;
using QuanLyHocSinh.ViewModel.BaoCao;
using QuanLyHocSinh.ViewModel.QuanLyTaiKhoan;
using QuanLyHocSinh.ViewModel.QuyDinh;
using QuanLyHocSinh.ViewModel.TraCuu;


namespace QuanLyHocSinh.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        #region Fields & Properties


        private BaseViewModel _currentView = null!;

        // Các ViewModel con dùng cho điều hướng
        public TrangChuViewModel TrangChuVM { get; set; }
        public QuanLyTaiKhoanMainViewModel TaiKhoanVM { get; set; }
        public TraCuuHocSinhViewModel HocSinhVM { get; set; }
        public TraCuuGiaoVienViewModel GiaoVienVM { get; set; }
        public TraCuuDiemHocSinhViewModel DiemHocSinhVM { get; set; }
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

                    IsGiaoVuVisible = string.Equals(roleName, "Giáo vụ", StringComparison.OrdinalIgnoreCase);
                    IsNotHocSinhVisible = _currentUser != null &&
                        !string.Equals(roleName, "Học sinh", StringComparison.OrdinalIgnoreCase);
                    IsNotGiaoVuVisible = _currentUser != null &&
                        !string.Equals(roleName, "Giáo vụ", StringComparison.OrdinalIgnoreCase);
                }
            }
        }

        private string _selectedRole = string.Empty;

        public string SelectedRole
        {
            get => _selectedRole;
            set
            {
                _selectedRole = value;
                OnPropertyChanged();
            }
        }


        // Các cờ điều khiển hiển thị UI theo vai trò người dùng

        private bool _isGiaoVuVisible;
        public bool IsGiaoVuVisible
        {
            get => _isGiaoVuVisible;
            set
            {
                if (_isGiaoVuVisible != value)
                {
                    _isGiaoVuVisible = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isNotHocSinhVisible;
        public bool IsNotHocSinhVisible
        {
            get => _isNotHocSinhVisible;
            set
            {

                if (_isNotHocSinhVisible != value)
                {
                    _isNotHocSinhVisible = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isNotGiaoVuVisible;
        public bool IsNotGiaoVuVisible
        {
            get => _isNotGiaoVuVisible;
            set
            {
                if (_isNotGiaoVuVisible != value)
                {
                    _isNotGiaoVuVisible = value;
                    OnPropertyChanged();
                }
            }

        public bool Isloaded { get; set; } = false;

        // Bộ sưu tập các vai trò để hiển thị lựa chọn
        public ObservableCollection<BeginViewModel> Roles { get; set; }

        // Các role model để chọn
        public BeginViewModel StudentRole { get; set; }
        public BeginViewModel TeacherRole { get; set; }
        public BeginViewModel AdminRole { get; set; }
        #endregion

        #region Commands
        public ICommand LoadedWindowCommand { get; set; }

        public ICommand ShowTrangChuCommand { get; }
        public ICommand ShowTaiKhoanCaNhanCommand { get; }
        public ICommand ShowQuanLyTaiKhoanCommand { get; }
        public ICommand ShowThongTinHocSinhCommand { get; }
        public ICommand ShowThongTinGiaoVienCommand { get; }
        public ICommand ShowDiemHocSinhCommand { get; }
        public ICommand ShowTongKetMonCommand { get; }
        public ICommand ShowTongKetNamCommand { get; }
        public ICommand ShowQuyDinhCommand { get; }
        public ICommand SelectRoleCommand { get; }

        #endregion

        #region Constructor

        public MainViewModel()
        {
            // Khởi tạo vai trò
            StudentRole = new BeginViewModel("pack://application:,,,/QuanLyHocSinh;component/Images/student_logo.png", "Học sinh");
            TeacherRole = new BeginViewModel("pack://application:,,,/QuanLyHocSinh;component/Images/teacher_logo.png", "Giáo viên");
            AdminRole = new BeginViewModel("pack://application:,,,/QuanLyHocSinh;component/Images/admin_logo.png", "Giáo Vụ");

            Roles = new ObservableCollection<BeginViewModel>
            {
                StudentRole,
                TeacherRole,
                AdminRole
            };

            // View mặc định
            CurrentView = new TrangChuViewModel(this);

            // Command điều hướng
            ShowTrangChuCommand = new RelayCommand<object>(_ => true, _ => CurrentView = new TrangChuViewModel(this));
            ShowQuanLyTaiKhoanCommand = new RelayCommand<object>(_ => true, _ => CurrentView = new QuanLyTaiKhoanMainViewModel(this));
            ShowTaiKhoanCaNhanCommand = new RelayCommand<object>(_ => true, _ => CurrentView = new QuanLyTaiKhoanCaNhanViewModel(this));
            ShowThongTinHocSinhCommand = new RelayCommand<object>(_ => true, _ => CurrentView = new TraCuuHocSinhViewModel(this));
            ShowThongTinGiaoVienCommand = new RelayCommand<object>(_ => true, _ => CurrentView = new TraCuuGiaoVienViewModel(this));
            ShowDiemHocSinhCommand = new RelayCommand<object>(_ => true, _ => CurrentView = new TraCuuDiemHocSinhViewModel(this));
            ShowTongKetMonCommand = new RelayCommand<object>(_ => true, _ => CurrentView = new TongKetMonViewModel(this));
            ShowTongKetNamCommand = new RelayCommand<object>(_ => true, _ => CurrentView = new TongKetNamViewModel(this));
            ShowQuyDinhCommand = new RelayCommand<object>(_ => true, _ => CurrentView = new QuyDinhMainViewModel(this));

            SelectRoleCommand = new RelayCommand<object>(
                parameter => parameter is BeginViewModel,
                parameter =>
                {
                    if (parameter is BeginViewModel selectedRole)
                    {
                        OpenLoginWindow(selectedRole.RoleName);
                    }
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
            {
                CurrentView = _navigationStack.Pop();
            }
            else
            {
                CurrentView = new TrangChuViewModel(this);
            }
        }

        #endregion


        #region Methods

        /// <summary>
        /// Mở cửa sổ đăng nhập tương ứng với role được chọn
        /// </summary>
        /// <param name="roleName">Tên vai trò (Học sinh, Giáo viên, Giáo vụ)</param>
        private void OpenLoginWindow(string roleName)
        {
            var loginVM = new LoginViewModel
            {
                SelectedRole = roleName
            };

            var loginWindow = new LoginWindow
            {
                DataContext = loginVM
            };

            // Hiệu ứng FadeIn khi loginWindow được load
            loginWindow.Loaded += (s, e) =>
            {
                WindowAnimationHelper.FadeIn(loginWindow);
            };

            loginWindow.Show();

            // Thu nhỏ và đóng BeginWindow (nếu có)
            foreach (Window window in Application.Current.Windows)
            {
                if (window is BeginWindow beginWindow)
                {
                    WindowAnimationHelper.FadeOut(beginWindow);
                    break;
                }
            }

            // Xử lý sự kiện đăng nhập thành công
            loginVM.LoginSuccess += (s, user) =>
            {
                // Lấy MainVM từ Resources của ứng dụng
                if (Application.Current.Resources["MainVM"] is MainViewModel mainVM)
                {
                    mainVM.CurrentUser = user;
                    mainVM.CurrentView = new TrangChuViewModel(mainVM);

                    // Mở MainWindow
                    var mainWindow = new MainWindow();
                    mainWindow.Show();

                    // Đóng loginWindow
                    loginWindow.Close();
                }
            };
        }

        /// <summary>
        /// Hỗ trợ gọi OnPropertyChanged
        /// </summary>
        protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            var loginVM = new LoginViewModel
            {
                SelectedRole = roleName
            };

            var loginWindow = new LoginWindow
            {
                DataContext = loginVM
            };

            loginWindow.Loaded += (_, _) => WindowAnimationHelper.FadeIn(loginWindow);
            loginWindow.Show();

            // Close BeginWindow if exists
            foreach (Window window in Application.Current.Windows)
            {
                if (window is BeginWindow beginWindow)
                {
                    WindowAnimationHelper.FadeOut(beginWindow);
                    break;
                }
            }
            // Handle login success
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
