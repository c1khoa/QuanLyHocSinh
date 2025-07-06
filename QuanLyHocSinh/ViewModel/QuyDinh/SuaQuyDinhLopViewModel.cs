using MaterialDesignThemes.Wpf;
using QuanLyHocSinh.Model.DAL;
using QuanLyHocSinh.Model.Entities;
using QuanLyHocSinh.View.Dialogs.MessageBox;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace QuanLyHocSinh.ViewModel.QuyDinh
{
    public class SuaQuyDinhLopViewModel : BaseViewModel
    {
        #region Properties
        private int _siSoLopToiDa;
        public int SiSoLopToiDa
        {
            get => _siSoLopToiDa;
            set { _siSoLopToiDa = value; OnPropertyChanged(); }
        }

        private ObservableCollection<Lop> _danhSachLop;
        public ObservableCollection<Lop> DanhSachLop
        {
            get => _danhSachLop;
            set { _danhSachLop = value; OnPropertyChanged(); }
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        #endregion

        private readonly Window _window;

        public SuaQuyDinhLopViewModel(Window window, int siSoLopToiDa, IList<Lop> danhSachLop)
        {
            _window = window;
            SiSoLopToiDa = siSoLopToiDa;
            DanhSachLop = new ObservableCollection<Lop>(danhSachLop);

            SaveCommand = new RelayCommand(async () => await Save());
            CancelCommand = new RelayCommand(Cancel);
        }

        private async Task Save()
        {
            // Kiểm tra dữ liệu đầu vào
            if (SiSoLopToiDa <= 0)
            {
                await ShowError("Lỗi nhập liệu", "Sĩ số lớp tối đa phải lớn hơn 0.");
                return;
            }

            if (DanhSachLop == null || DanhSachLop.Count == 0)
            {
                await ShowError("Lỗi nhập liệu", "Danh sách lớp không được để trống.");
                return;
            }

            foreach (var lop in DanhSachLop)
            {
                if (string.IsNullOrWhiteSpace(lop.TenLop))
                {
                    await ShowError("Lỗi nhập liệu", "Tên lớp không được để trống.");
                    return;
                }
            }

            // 🔴 Kiểm tra nếu sĩ số lớp hiện tại vượt quá quy định mới
            foreach (var lop in DanhSachLop)
            {
                if (lop.SiSo > SiSoLopToiDa)
                {
                    await ShowError(
                        "Lỗi cập nhật",
                        $"Lớp {lop.TenLop} hiện có sĩ số {lop.SiSo}, vượt quá giới hạn mới {SiSoLopToiDa}."
                    );
                    return;
                }
            }

            try
            {
                // Cập nhật sĩ số tối đa
                var quyDinh = QuyDinhDAL.GetQuyDinh();
                if (quyDinh != null)
                {
                    quyDinh.SiSoLop_ToiDa = this.SiSoLopToiDa;
                    QuyDinhDAL.UpdateQuyDinh(quyDinh);
                }

                // Cập nhật danh sách lớp
                LopDAL.UpdateDanhSachLop(DanhSachLop.ToList());

                await DialogHost.Show(new NotifyDialog("Thông báo", "Cập nhật quy định lớp thành công!"), "RootDialog_SuaLop");
                _window.DialogResult = true;
                _window.Close();
            }
            catch (Exception ex)
            {
                await ShowError("Lỗi hệ thống", "Đã xảy ra lỗi khi cập nhật: " + ex.Message);
            }
        }


        private void Cancel()
        {
            _window.DialogResult = false;
            _window.Close();
        }

        private async Task ShowError(string title, string message)
        {
            try
            {
                await DialogHost.Show(new ErrorDialog(title, message), "RootDialog_SuaLop");
            }
            catch
            {
                MessageBox.Show(message, title, MessageBoxButton.OK,
                    title.Contains("Lỗi") ? MessageBoxImage.Error : MessageBoxImage.Information);
            }
        }
    }
}
