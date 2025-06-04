using QuanLyHocSinh.Model.Entities;
using QuanLyHocSinh.View.Converters;
using QuanLyHocSinh.View.Windows;
using QuanLyHocSinh.ViewModel.BaoCao;
using QuanLyHocSinh.ViewModel.QuanLyTaiKhoan;
using QuanLyHocSinh.ViewModel.QuyDinh;
using QuanLyHocSinh.ViewModel.TraCuu;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace QuanLyHocSinh.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        #region Fields & Properties

        private BaseViewModel _currentView = null!;
        public BaseViewModel CurrentView
        {
            get => _currentView;
            set
            {
                if (_currentView != value)
                {
                    _currentView = value;
                    System.Diagnostics.Debug.WriteLine($"CurrentView changed to: {value?.GetType().Name}");
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
        }

        public bool IsLoaded { get; set; }

        public ObservableCollection<BeginViewModel> Roles { get; }

        public BeginViewModel StudentRole { get; }
        public BeginViewModel TeacherRole { get; }
        public BeginViewModel AdminRole { get; }

        #endregion

        #region Commands

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
            // Initialize roles
            StudentRole = new BeginViewModel(
                "pack://application:,,,/QuanLyHocSinh;component/Images/student_logo.png",
                "Học sinh");

            TeacherRole = new BeginViewModel(
                "pack://application:,,,/QuanLyHocSinh;component/Images/teacher_logo.png",
                "Giáo viên");

            AdminRole = new BeginViewModel(
                "pack://application:,,,/QuanLyHocSinh;component/Images/admin_logo.png",
                "Giáo Vụ");

            Roles = new ObservableCollection<BeginViewModel>
            {
                StudentRole,
                TeacherRole,
                AdminRole
            };

            // Initialize CurrentView
            CurrentView = new TrangChuViewModel(this);

            // Initialize commands
            ShowTrangChuCommand = new RelayCommand<object>(
                _ => true,
                _ => CurrentView = new TrangChuViewModel(this));

            ShowQuanLyTaiKhoanCommand = new RelayCommand<object>(
                _ => true,
                _ => CurrentView = new QuanLyTaiKhoanMainViewModel(this));

            ShowTaiKhoanCaNhanCommand = new RelayCommand<object>(
                _ => true,
                _ => CurrentView = new QuanLyTaiKhoanCaNhanViewModel(this));

            ShowThongTinHocSinhCommand = new RelayCommand<object>(
                _ => true,
                _ => CurrentView = new TraCuuHocSinhViewModel(this));

            ShowThongTinGiaoVienCommand = new RelayCommand<object>(
                _ => true,
                _ => CurrentView = new TraCuuGiaoVienViewModel(this));

            ShowDiemHocSinhCommand = new RelayCommand<object>(
                _ => true,
                _ => CurrentView = new TraCuuDiemHocSinhViewModel(this));

            ShowTongKetMonCommand = new RelayCommand<object>(
                _ => true,
                _ => CurrentView = new TongKetMonViewModel(this));

            ShowTongKetNamCommand = new RelayCommand<object>(
                _ => true,
                _ => CurrentView = new TongKetNamViewModel(this));

            ShowQuyDinhCommand = new RelayCommand<object>(
                _ => true,
                _ => CurrentView = new QuyDinhMainViewModel(this));

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

        // Bổ sung phương thức điều hướng quay lại
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
