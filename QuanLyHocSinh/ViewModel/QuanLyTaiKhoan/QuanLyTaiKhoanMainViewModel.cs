using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using QuanLyHocSinh.Model.Entities;
using QuanLyHocSinh.Service;
using QuanLyHocSinh.View.Dialogs;

namespace QuanLyHocSinh.ViewModel.QuanLyTaiKhoan
{
    /// <summary>
    /// ViewModel cho màn quản lý tài khoản (chỉ admin mới vào được).
    /// Gồm logic: tải danh sách, thêm, sửa, xoá tài khoản.
    /// </summary>
    public class QuanLyTaiKhoanMainViewModel : BaseViewModel
    {
        private readonly MainViewModel _mainVM;

        #region Constructor
        public QuanLyTaiKhoanMainViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM ?? throw new ArgumentNullException(nameof(mainVM));

            // Khởi tạo các command
            ShowThemTaiKhoanCommand = new RelayCommand(ShowThemTaiKhoan);
            ReloadCommand = new RelayCommand(LoadDanhSachTaiKhoan);
            SuaTaiKhoanCommand = new RelayCommand<User>(user => SuaTaiKhoan(user));
            XoaTaiKhoanCommand = new RelayCommand(() => XoaTaiKhoan(SelectedUser));

            // Tải dữ liệu ban đầu
            LoadDanhSachTaiKhoan();
        }
        #endregion

        #region Danh sách tài khoản

        public ObservableCollection<User> Users { get; } = new();

        private ObservableCollection<User> _filteredUsers = new();
        public ObservableCollection<User> FilteredUsers
        {
            get => _filteredUsers;
            set
            {
                _filteredUsers = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region SelectedUser & Search

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
                }
            }
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged();
                    FilterUsers();
                }
            }
        }

        #endregion

        #region Command

        public ICommand ShowThemTaiKhoanCommand { get; }
        public ICommand ReloadCommand { get; }
        public ICommand SuaTaiKhoanCommand { get; }
        public ICommand XoaTaiKhoanCommand { get; }

        #endregion

        #region Lọc người dùng

        private void FilterUsers()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                FilteredUsers = new ObservableCollection<User>(Users);
            }
            else
            {
                string keyword = SearchText.Trim().ToLower();
                FilteredUsers = new ObservableCollection<User>(
                    Users.Where(u => u.UserID?.ToLower().Contains(keyword) == true));
            }
        }

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
                    user.VaiTro ??= new VaiTro(); // Đảm bảo không null
                    Users.Add(user);
                }

                FilterUsers(); // Cập nhật danh sách lọc
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}");
            }
        }

        #endregion

        #region Thêm tài khoản

        private void ShowThemTaiKhoan()
        {
            var themVM = new QuanLyTaiKhoanThemViewModel(_mainVM);

            themVM.AccountAddedSuccessfully += userMoi =>
            {
                Users.Insert(0, userMoi);
                _mainVM.CurrentView = this;
            };

            themVM.CancelRequested += () => _mainVM.CurrentView = this;

            _mainVM.CurrentView = themVM;
        }

        #endregion

        #region Sửa tài khoản

        private void SuaTaiKhoan(User userToEdit)
        {
            if (userToEdit == null)
            {
                MessageBox.Show("Không tìm thấy tài khoản để sửa.");
                return;
            }

            var suaVM = new QuanLyTaiKhoanSuaViewModel(userToEdit, _mainVM);
            var dialog = new SuaTaiKhoanDiaLog
            {
                DataContext = suaVM
            };

            suaVM.CancelRequested += () => dialog.Close();

            suaVM.AccountEditedSuccessfully += editedUser =>
            {
                var user = Users.FirstOrDefault(u => u.UserID == editedUser.UserID);
                if (user != null)
                {
                    user.HoTen = editedUser.HoTen;
                    user.TenDangNhap = editedUser.TenDangNhap;
                    user.VaiTroID = editedUser.VaiTroID;
                    user.VaiTro = editedUser.VaiTro;

                    // Nếu có cập nhật mật khẩu, bạn có thể thêm dòng:
                     user.MatKhau = editedUser.MatKhau;
                }

                dialog.Close();
            };


            dialog.ShowDialog();
        }

        #endregion

        #region Xóa tài khoản

        private void XoaTaiKhoan(User user)
        {
            if (user == null)
            {
                MessageBox.Show("Không tìm thấy tài khoản để xóa.");
                return;
            }

            try
            {
                bool hasRelatedStudents = UserService.HasRelatedStudents(user.UserID);

                if (hasRelatedStudents)
                {
                    var confirm = MessageBox.Show(
                        "Tài khoản này có học sinh liên quan.\nXóa cả học sinh liên quan?",
                        "Cảnh báo", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (confirm != MessageBoxResult.Yes) return;

                    UserService.XoaTaiKhoanVaHocSinh(user.UserID);
                }
                else
                {
                    var confirm = MessageBox.Show(
                        $"Bạn chắc muốn xóa tài khoản \"{user.HoTen}\"?",
                        "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (confirm != MessageBoxResult.Yes) return;

                    UserService.XoaTaiKhoan(user.UserID);
                }

                Users.Remove(user);
                MessageBox.Show("Đã xóa tài khoản thành công!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xóa tài khoản: {ex.Message}");
            }
        }

        #endregion
    }
}
