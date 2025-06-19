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

        public ObservableCollection<string> VaiTroOptions { get; } = new();

        private string _selectedVaiTroFilter;
        public string SelectedVaiTroFilter
        {
            get => _selectedVaiTroFilter;
            set
            {
                if (_selectedVaiTroFilter != value)
                {
                    _selectedVaiTroFilter = value;
                    OnPropertyChanged();
                    FilterAll();
                }
            }
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

        private string _searchTextID;
        public string SearchTextID
        {
            get => _searchTextID;
            set
            {
                if (_searchTextID != value)
                {
                    _searchTextID = value;
                    OnPropertyChanged();
                    FilterAll();
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
                    FilterAll();
                }
            }
        }

        private string _searchLoginText;
        public string SearchLoginText
        {
            get => _searchLoginText;
            set
            {
                if (_searchLoginText != value)
                {
                    _searchLoginText = value;
                    OnPropertyChanged();
                    FilterAll();
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

        #region Logic Lọc, Tải, Sửa, Xóa
        private void FilterAll()
        {
            var query = Users.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchTextID))
                query = query.Where(u => (u.UserID ?? "").ToLower().Contains(SearchTextID.Trim().ToLower()));

            if (!string.IsNullOrWhiteSpace(SearchText))
                query = query.Where(u => (u.HoTen ?? "").ToLower().Contains(SearchText.Trim().ToLower()));

            if (!string.IsNullOrWhiteSpace(SearchLoginText))
                query = query.Where(u => (u.TenDangNhap ?? "").ToLower().Contains(SearchLoginText.Trim().ToLower()));

            if (!string.IsNullOrWhiteSpace(SelectedVaiTroFilter) && SelectedVaiTroFilter != "Tất cả")
                query = query.Where(u => u.VaiTro?.TenVaiTro == SelectedVaiTroFilter);

            FilteredUsers = new ObservableCollection<User>(query);
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

                // Load vai trò vào danh sách lọc
                VaiTroOptions.Clear();
                VaiTroOptions.Add("Tất cả");
                foreach (var tenVaiTro in Users
                    .Where(u => u.VaiTro != null)
                    .Select(u => u.VaiTro.TenVaiTro)
                    .Distinct()
                    .OrderBy(v => v))
                {
                    VaiTroOptions.Add(tenVaiTro);
                }

                SelectedVaiTroFilter = "Tất cả";
                FilterAll();
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

            UserService.XoaTaiKhoanVaLienQuan(user.UserID);
            Users.Remove(user);
            LoadDanhSachTaiKhoan();
        }
        #endregion
    }
}
