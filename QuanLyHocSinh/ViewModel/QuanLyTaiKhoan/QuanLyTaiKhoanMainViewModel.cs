// QuanLyTaiKhoanMainViewModel.cs
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
    public class QuanLyTaiKhoanMainViewModel : BaseViewModel
    {
        private readonly MainViewModel _mainVM;

        public QuanLyTaiKhoanMainViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM ?? throw new ArgumentNullException(nameof(mainVM));

            ShowThemTaiKhoanCommand = new RelayCommand(ShowThemTaiKhoan);
            ReloadCommand = new RelayCommand(LoadDanhSachTaiKhoan);

            // ⚠️  Đổi sang RelayCommand<User> để nhận tham số từ nút Xóa trong XAML
            SuaTaiKhoanCommand = new RelayCommand<User>(user => SuaTaiKhoan(user));
            XoaTaiKhoanCommand = new RelayCommand<User>(user => XoaTaiKhoan(user));

            LoadDanhSachTaiKhoan();
        }

        #region Thuộc tính & Trạng thái
        public ObservableCollection<User> Users { get; } = new();

        private ObservableCollection<User> _filteredUsers = new();
        public ObservableCollection<User> FilteredUsers
        {
            get => _filteredUsers;
            set { _filteredUsers = value; OnPropertyChanged(); }
        }

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
                    OnPropertyChanged(nameof(CanDelete));
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

        public bool CanDelete =>
            SelectedUser != null &&
            _mainVM.CurrentUser?.UserID != SelectedUser.UserID;
        #endregion

        #region Lệnh (Commands)
        public ICommand ShowThemTaiKhoanCommand { get; }
        public ICommand ReloadCommand { get; }
        public ICommand SuaTaiKhoanCommand { get; }
        public ICommand XoaTaiKhoanCommand { get; }
        #endregion

        #region Logic lọc, tải, thêm, sửa, xóa
        private void FilterUsers()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
                FilteredUsers = new(Users);
            else
            {
                string keyword = SearchText.Trim().ToLower();
                FilteredUsers = new(
                    Users.Where(u =>
                        (u.UserID ?? string.Empty).ToLower().Contains(keyword) ||
                        (u.TenDangNhap ?? string.Empty).ToLower().Contains(keyword) ||
                        (u.HoTen ?? string.Empty).ToLower().Contains(keyword)));
            }
        }

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
                    user.VaiTro ??= new VaiTro();
                    Users.Add(user);
                }
                FilterUsers();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}");
            }
        }

        private void ShowThemTaiKhoan()
        {
            var themVM = new QuanLyTaiKhoanThemViewModel(_mainVM);

            themVM.AccountAddedSuccessfully += userMoi =>
            {
                Users.Insert(0, userMoi);
                LoadDanhSachTaiKhoan();
                _mainVM.CurrentView = this;
            };
            themVM.CancelRequested += () => _mainVM.CurrentView = this;

            _mainVM.CurrentView = themVM;
        }

        private void SuaTaiKhoan(User userToEdit)
        {
            if (userToEdit == null)
            {
                MessageBox.Show("Không tìm thấy tài khoản để sửa.");
                return;
            }

            var suaVM = new QuanLyTaiKhoanSuaViewModel(userToEdit, _mainVM);
            var dialog = new SuaTaiKhoanDiaLog { DataContext = suaVM };

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
                    user.MatKhau = editedUser.MatKhau;
                }
                dialog.Close();
            };
            dialog.ShowDialog();
        }

        private void XoaTaiKhoan(User user)
        {
            if (user == null)
            {
                MessageBox.Show("Không tìm thấy tài khoản để xóa");
                return;
            }

            if (_mainVM.CurrentUser?.UserID == user.UserID)
            {
                MessageBox.Show("Không thể xóa tài khoản đang đăng nhập!");
                return;
            }

            bool hasRel = UserService.HasRelatedStudents(user.UserID);
            string msg = hasRel
                ? "Tài khoản này có dữ liệu liên quan (điểm, hồ sơ ...).\nBạn có chắc muốn xóa tất cả?"
                : $"Bạn chắc muốn xóa tài khoản \"{user.TenDangNhap}\"?";

            if (MessageBox.Show(msg, "Xác nhận", MessageBoxButton.YesNo,
                hasRel ? MessageBoxImage.Warning : MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            try
            {
                UserService.XoaTaiKhoanVaHocSinh(user.UserID);
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
