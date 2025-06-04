using QuanLyHocSinh.Model.Entities;
using QuanLyHocSinh.Service;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace QuanLyHocSinh.ViewModel.QuanLyTaiKhoan
{
    public class QuanLyTaiKhoanMainViewModel : BaseViewModel
    {
        private readonly MainViewModel _mainVM;

        public ObservableCollection<User> DanhSachTaiKhoan { get; }
        public ICommand ShowThemTaiKhoanCommand { get; }
        public ICommand ReloadCommand { get; }

        public QuanLyTaiKhoanMainViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM ?? throw new ArgumentNullException(nameof(mainVM));
            DanhSachTaiKhoan = new ObservableCollection<User>();

            LoadDanhSachTaiKhoan();

            ShowThemTaiKhoanCommand = new RelayCommand(
                execute: () => ShowThemTaiKhoan()
            );

            ReloadCommand = new RelayCommand(
                execute: () => LoadDanhSachTaiKhoan()
            );
        }

        private void ShowThemTaiKhoan()
        {
            try
            {
                // Sửa: Truyền mainVM vào constructor
                var themVM = new QuanLyTaiKhoanThemViewModel(_mainVM);

                themVM.AccountAddedSuccessfully += (userMoi) =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        DanhSachTaiKhoan.Insert(0, userMoi);
                    });
                };

                // Thêm xử lý khi hủy bỏ
                themVM.CancelRequested += () =>
                {
                    _mainVM.CurrentView = this; // Quay lại view hiện tại
                };

                _mainVM.CurrentView = themVM;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi mở form thêm tài khoản: {ex.Message}");
            }
        }

        private void LoadDanhSachTaiKhoan()
        {
            try
            {
                DanhSachTaiKhoan.Clear();
                var danhSach = UserService.LayDanhSachTaiKhoan();

                if (danhSach != null)
                {
                    foreach (var user in danhSach)
                    {
                        if (user != null)
                        {
                            DanhSachTaiKhoan.Add(user);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách tài khoản: {ex.Message}");
            }
        }
    }
}
