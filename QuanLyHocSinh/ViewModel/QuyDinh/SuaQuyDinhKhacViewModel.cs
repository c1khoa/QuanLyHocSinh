using MaterialDesignThemes.Wpf;
using QuanLyHocSinh.Model.DAL;
using QuanLyHocSinh.Model.Entities;
using QuanLyHocSinh.View.Dialogs.MessageBox;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace QuanLyHocSinh.ViewModel.QuyDinh
{
    public class SuaQuyDinhKhacViewModel : BaseViewModel
    {
        #region Properties
        private string _quyDinhKhac;
        public string QuyDinhKhac
        {
            get => _quyDinhKhac;
            set { _quyDinhKhac = value; OnPropertyChanged(); }
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        #endregion

        private readonly Window _dialog;

        public SuaQuyDinhKhacViewModel(Window dialog, string currentQuyDinhKhac)
        {
            _dialog = dialog;
            QuyDinhKhac = currentQuyDinhKhac;

            SaveCommand = new RelayCommand(async () => await SaveChanges());
            CancelCommand = new RelayCommand(() =>
            {
                _dialog.DialogResult = false;
                _dialog.Close();
            });
        }

        private async Task SaveChanges()
        {
            // Kiểm tra dữ liệu đầu vào
            if (string.IsNullOrWhiteSpace(QuyDinhKhac))
            {
                await ShowError("Lỗi nhập liệu", "Quy định khác không được để trống.");
                return;
            }

            try
            {
                var currentQuyDinh = QuyDinhDAL.GetQuyDinh();
                if (currentQuyDinh == null)
                {
                    await ShowError("Lỗi hệ thống", "Không tìm thấy dữ liệu quy định gốc.");
                    return;
                }

                currentQuyDinh.QuyDinhKhac = this.QuyDinhKhac;
                QuyDinhDAL.UpdateQuyDinh(currentQuyDinh);

                await DialogHost.Show(new NotifyDialog("Thông báo", "Cập nhật quy định khác thành công!"), "RootDialog_SuaKhac");
                _dialog.DialogResult = true;
                _dialog.Close();
            }
            catch (Exception ex)
            {
                await ShowError("Lỗi hệ thống", "Đã xảy ra lỗi khi cập nhật: " + ex.Message);
            }
        }

        private async Task ShowError(string title, string message)
        {
            try
            {
                await DialogHost.Show(new ErrorDialog(title, message), "RootDialog_SuaKhac");
            }
            catch
            {
                MessageBox.Show(message, title, MessageBoxButton.OK,
                    title.Contains("Lỗi") ? MessageBoxImage.Error : MessageBoxImage.Information);
            }
        }
    }
}
