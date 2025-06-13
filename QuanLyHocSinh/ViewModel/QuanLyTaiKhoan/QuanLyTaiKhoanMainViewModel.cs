using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using QuanLyHocSinh.Model.Entities;
using QuanLyHocSinh.Service;

namespace QuanLyHocSinh.ViewModel.QuanLyTaiKhoan
{
    /// <summary>
    /// ViewModel cho màn quản lý tài khoản.
    /// Bao gồm logic tải danh sách, thêm, sửa, xoá và quyền cho nút thao tác.
    /// </summary>
    public class QuanLyTaiKhoanMainViewModel : BaseViewModel
    {
        private readonly MainViewModel _mainVM;

        #region Thuộc tính bind sang View
        public ObservableCollection<User> Users { get; } = new();

        private User _selectedUser;
        public User SelectedUser
        {
            get => _selectedUser;
            set
            {
                if (_selectedUser != value)
                {
                    _selectedUser = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CanEditCurrentUser));
                    OnPropertyChanged(nameof(CanDeleteCurrentUser));
                  
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        /// <summary>
        /// Xác định người đăng nhập có quyền Giáo Vụ/Admin hay không.
        /// </summary>
        private bool IsGiaoVu =>
            string.Equals(_mainVM.CurrentUser?.VaiTro?.TenVaiTro, "GiaoVu", StringComparison.OrdinalIgnoreCase)
         || string.Equals(_mainVM.CurrentUser?.VaiTroID, "GVU", StringComparison.OrdinalIgnoreCase)
         || string.Equals(_mainVM.CurrentUser?.VaiTroID, "GIAOVU", StringComparison.OrdinalIgnoreCase)
         || string.Equals(_mainVM.CurrentUser?.VaiTro?.TenVaiTro, "Admin", StringComparison.OrdinalIgnoreCase);

        public bool CanEditCurrentUser => CanEditUser(SelectedUser);
        public bool CanDeleteCurrentUser => CanDeleteUser(SelectedUser);
        #endregion

        #region Command
        public ICommand ShowThemTaiKhoanCommand { get; }
        public ICommand ReloadCommand { get; }
        public ICommand SuaTaiKhoanCommand { get; }
        public ICommand XoaTaiKhoanCommand { get; }
        #endregion

        public QuanLyTaiKhoanMainViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM ?? throw new ArgumentNullException(nameof(mainVM));

            // Khởi tạo command
            ShowThemTaiKhoanCommand = new RelayCommand(ShowThemTaiKhoan);
            ReloadCommand = new RelayCommand(LoadDanhSachTaiKhoan);
            SuaTaiKhoanCommand = new RelayCommand(
                                        execute: () => SuaTaiKhoan(SelectedUser),
                                        canExecute: () => CanEditCurrentUser);
            XoaTaiKhoanCommand = new RelayCommand(
                                        execute: () => XoaTaiKhoan(SelectedUser),
                                        canExecute: () => CanDeleteCurrentUser);

            // Tải dữ liệu ban đầu
            LoadDanhSachTaiKhoan();
        }

        #region Logic quyền
        private bool CanEditUser(User user) =>
            user != null && (IsGiaoVu || user.UserID == _mainVM.CurrentUser?.UserID);

        private bool CanDeleteUser(User user) =>
            user != null && IsGiaoVu && user.UserID != _mainVM.CurrentUser?.UserID;
        #endregion

        #region Tải danh sách tài khoản
        private void LoadDanhSachTaiKhoan()
        {
            try
            {
                Users.Clear();
                var danhSach = UserService.LayDanhSachTaiKhoan();

                if (danhSach == null)
                {
                    MessageBox.Show("Không thể tải danh sách tài khoản!");
                    return;
                }

                foreach (var user in danhSach)
                {
                    user.VaiTro ??= new VaiTro(); // Bảo đảm không null
                    Users.Add(user);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}");
            }
        }
        #endregion

        #region Thêm – Sửa – Xóa
        private void ShowThemTaiKhoan()
        {
            var themVM = new QuanLyTaiKhoanThemViewModel(_mainVM);

            themVM.AccountAddedSuccessfully += userMoi =>
            {
                Users.Insert(0, userMoi);     // Thêm vào đầu danh sách
                _mainVM.CurrentView = this;   // Quay về màn danh sách
            };

            themVM.CancelRequested += () => _mainVM.CurrentView = this;

            _mainVM.CurrentView = themVM;
        }

        private void SuaTaiKhoan(User userToEdit)
        {
            if (userToEdit == null) return;

            var suaVM = new QuanLyTaiKhoanSuaViewModel(userToEdit, _mainVM);

            suaVM.AccountEditedSuccessfully += editedUser =>
            {
                // Cập nhật lại item trong ObservableCollection
                var index = Users.IndexOf(userToEdit);
                if (index >= 0) Users[index] = editedUser;
                _mainVM.CurrentView = this;
            };

            suaVM.CancelRequested += () => _mainVM.CurrentView = this;

            _mainVM.CurrentView = suaVM;
        }

        private void XoaTaiKhoan(User user)
        {
            if (user == null) return;

            try
            {
                // Kiểm tra liên quan học sinh
                bool hasRelatedStudents = UserService.HasRelatedStudents(user.UserID);

                if (hasRelatedStudents)
                {
                    var yes = MessageBox.Show(
                        "Tài khoản này có học sinh liên quan.\nXóa cả học sinh liên quan?",
                        "Cảnh báo", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (yes != MessageBoxResult.Yes) return;

                    UserService.XoaTaiKhoanVaHocSinh(user.UserID);
                }
                else
                {
                    var yes = MessageBox.Show(
                        $"Bạn chắc muốn xóa tài khoản \"{user.HoTen}\"?",
                        "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (yes != MessageBoxResult.Yes) return;

                    UserService.XoaTaiKhoan(user.UserID);
                }

                Users.Remove(user);
                MessageBox.Show("Đã xóa tài khoản thành công!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xóa: {ex.Message}");
            }
        }
        #endregion
    }
}
