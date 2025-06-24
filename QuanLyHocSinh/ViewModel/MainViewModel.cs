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
    public class MainViewModel : BaseViewModel, INotifyPropertyChanged
    {
        #region Fields & Properties

        private static MainViewModel _instance;
        public static MainViewModel Instance => _instance ??= new MainViewModel();
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
                    OnPropertyChanged(nameof(CurrentView));
                }
            }
        }

        private User _currentUser;
        /// <summary>
        /// Người dùng đang đăng nhập
        /// </summary>
        public User CurrentUser
        {
            get => _currentUser;
            set
            {
                if (_currentUser != value)
                {
                    _currentUser = value;
                    OnPropertyChanged(nameof(CurrentUser));
                    OnPropertyChanged(nameof(VaiTro));

                    var roleName = _currentUser?.VaiTro?.TenVaiTro?.Trim() ?? "null";

                    IsGiaoVuVisible = string.Equals(roleName, "Giáo vụ", StringComparison.OrdinalIgnoreCase);
                    IsNotHocSinhVisible = _currentUser != null && !string.Equals(roleName, "Học sinh", StringComparison.OrdinalIgnoreCase);
                    IsNotGiaoVuVisible = _currentUser != null && !string.Equals(roleName, "Giáo vụ", StringComparison.OrdinalIgnoreCase);

                }
            }
        }

        private string _selectedRole;
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
                _isGiaoVuVisible = value;
                OnPropertyChanged();
            }
        }

        private bool _isNotHocSinhVisible;
        public bool IsNotHocSinhVisible
        {
            get => _isNotHocSinhVisible;
            set
            {
                _isNotHocSinhVisible = value;
                OnPropertyChanged();
            }
        }

        private bool _isNotGiaoVuVisible;
        public bool IsNotGiaoVuVisible
        {
            get => _isNotGiaoVuVisible;
            set
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

        public ICommand LoginExitCommand { get; set; }

        // Commands điều hướng các ViewModel
        public ICommand ShowTrangChuCommand { get; set; }
        public ICommand ShowTaiKhoanCaNhanCommand { get; set; }
        public ICommand ShowQuanLyTaiKhoanCommand { get; set; }
        public ICommand ShowThongTinHocSinhCommand { get; set; }
        public ICommand ShowThongTinGiaoVienCommand { get; set; }
        public ICommand ShowDiemHocSinhCommand { get; set; }
        public ICommand ShowTongKetMonCommand { get; set; }

        public ICommand ShowTongKetNamCommand { get; set; }
        public ICommand ShowQuyDinhCommand { get; set; }

        // Command chọn vai trò mở LoginWindow
        public ICommand SelectRoleCommand { get; set; }

        #endregion

        #region Constructor

        public MainViewModel()
        {
            // Khởi tạo các vai trò cho lựa chọn
            StudentRole = new BeginViewModel("pack://application:,,,/QuanLyHocSinh;component/Images/student_logo.png", "Học sinh");
            TeacherRole = new BeginViewModel("pack://application:,,,/QuanLyHocSinh;component/Images/teacher_logo.png", "Giáo viên");
            AdminRole = new BeginViewModel("pack://application:,,,/QuanLyHocSinh;component/Images/admin_logo.png", "Giáo vụ");

            Roles = new ObservableCollection<BeginViewModel>
            {
                StudentRole,
                TeacherRole,
                AdminRole
            };
            LoginExitCommand = new RelayCommand<object>(
    (p) => true,
    (p) =>
    {
        // 1. Tìm MainWindow hiện tại
        // Cách an toàn nhất là tìm trong Application.Current.Windows
        // Hoặc nếu bạn biết chắc chắn nó là MainWindow.Current (singleton)
        MainWindow mainWindow = null;
        foreach (Window window in Application.Current.Windows)
        {
            if (window is MainWindow mw) // Kiểm tra xem cửa sổ có phải là MainWindow không
            {
                mainWindow = mw;
                break;
            }
        }

        // Đảm bảo MainWindow được tìm thấy
        if (mainWindow != null)
        {
            // 2. Mở BeginWindow
            var beginWindow = new BeginWindow();
            beginWindow.Show(); // HIỂN THỊ CỬA SỔ MỚI TRƯỚC

            _ = WindowAnimationHelper.FadeInAsync(beginWindow);

            // 3. Đóng MainWindow
            mainWindow.Close(); // ĐÓNG CỬA SỔ CŨ SAU KHI CỬA SỔ MỚI ĐÃ HIỂN THỊ
        }
        else
        {
            // Xử lý trường hợp không tìm thấy MainWindow (ví dụ: ghi log)
            Console.WriteLine("Không thể đăng xuất");
        }
    }
);
            // Gán ViewModel mặc định là Trang Chủ
            CurrentView = new TrangChuViewModel(this);

            // Khởi tạo các command điều hướng ViewModel
            ShowTrangChuCommand = new RelayCommand<object>((p) => true, (p) => CurrentView = new TrangChuViewModel(this));
            ShowQuanLyTaiKhoanCommand = new RelayCommand<object>((p) => true, (p) => CurrentView = new QuanLyTaiKhoanMainViewModel(this));
            ShowTaiKhoanCaNhanCommand = new RelayCommand<object>((p) => true, (p) => CurrentView = new QuanLyTaiKhoanCaNhanViewModel(this));
            ShowThongTinHocSinhCommand = new RelayCommand<object>((p) => true, (p) => CurrentView = new TraCuuHocSinhViewModel(this));
            ShowThongTinGiaoVienCommand = new RelayCommand<object>((p) => true, (p) => CurrentView = new TraCuuGiaoVienViewModel(this));
            ShowDiemHocSinhCommand = new RelayCommand<object>((p) => true, (p) => CurrentView = new TraCuuDiemHocSinhViewModel(this));
            ShowTongKetMonCommand = new RelayCommand<object>((p) => true, (p) => CurrentView = new TongKetMonViewModel(this));
            ShowTongKetNamCommand = new RelayCommand<object>((p) => true, (p) => CurrentView = new TongKetNamViewModel(this));
            ShowQuyDinhCommand = new RelayCommand<object>((p) => true, (p) => CurrentView = new QuyDinhMainViewModel(this));

            // Command xử lý chọn role để mở LoginWindow
            SelectRoleCommand = new RelayCommand<object>(
                (parameter) => true,
                (parameter) =>
                {
                    if (parameter is BeginViewModel selectedRole)
                    {
                        OpenLoginWindow(selectedRole.RoleName);
                    }
                });
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
            if (!Equals(field, newValue))
            {
                field = newValue;
                OnPropertyChanged(propertyName);
                return true;
            }
            return false;
        }

        #endregion
    }
}