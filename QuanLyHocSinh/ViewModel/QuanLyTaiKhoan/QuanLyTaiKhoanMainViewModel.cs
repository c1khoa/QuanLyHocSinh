using QuanLyHocSinh.Model.Entities;
using QuanLyHocSinh.Service;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace QuanLyHocSinh.ViewModel.QuanLyTaiKhoan
{
    public class QuanLyTaiKhoanMainViewModel : BaseViewModel
    {
        public readonly MainViewModel _mainVM;

        public ObservableCollection<User> Users { get; } = new ObservableCollection<User>();
        public ICommand ShowThemTaiKhoanCommand { get; }
        public ICommand ReloadCommand { get; }

        public QuanLyTaiKhoanMainViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM ?? throw new ArgumentNullException(nameof(mainVM));

            LoadDanhSachTaiKhoan();

            ShowThemTaiKhoanCommand = new RelayCommand(ShowThemTaiKhoan);
            ReloadCommand = new RelayCommand(LoadDanhSachTaiKhoan);
        }

        public void ShowThemTaiKhoan()
        {
            // Trong phương thức ShowThemTaiKhoan()
            var themVM = new QuanLyTaiKhoanThemViewModel(_mainVM);

            themVM.AccountAddedSuccessfully += (userMoi) =>
            {
                Users.Insert(0, userMoi);
                _mainVM.CurrentView = this; // Quay lại view chính
            };

            themVM.CancelRequested += () =>
            {
                _mainVM.CurrentView = this; // Quay lại view chính
            };

            _mainVM.CurrentView = themVM;
        }

        public void LoadDanhSachTaiKhoan()
        {
            try
            {
                Users.Clear();
                var danhSach = UserService.LayDanhSachTaiKhoan();

                if (danhSach == null)
                {
                    MessageBox.Show("Không thể tải danh sách tài khoản (null).");
                    return;
                }

                foreach (var user in danhSach)
                {
                    if (user == null) continue;

                    user.VaiTro ??= new VaiTro();
                    Users.Add(user);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách tài khoản: {ex.Message}");
            }
        }
    }
}
