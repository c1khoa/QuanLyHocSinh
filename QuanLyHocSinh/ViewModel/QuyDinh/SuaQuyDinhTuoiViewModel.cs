using MaterialDesignThemes.Wpf;
using QuanLyHocSinh.Model.Entities;
using QuanLyHocSinh.View.Dialogs.MessageBox;
using System.Windows;
using System.Windows.Input;

namespace QuanLyHocSinh.ViewModel.QuyDinh
{
    public class SuaQuyDinhTuoiViewModel : BaseViewModel
    {
        public int TuoiToiThieuHS { get; set; }
        public int TuoiToiDaHS { get; set; }
        public int TuoiToiThieuGV { get; set; }
        public int TuoiToiDaGV { get; set; }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        private Window _window;
        private QuyDinhTuoiEntities _quyDinhHS;
        private QuyDinhTuoiEntities _quyDinhGV;

        public SuaQuyDinhTuoiViewModel(Window window, QuyDinhTuoiEntities quyDinhHS, QuyDinhTuoiEntities quyDinhGV)
        {
            _window = window;
            _quyDinhHS = quyDinhHS;
            _quyDinhGV = quyDinhGV;

            TuoiToiThieuHS = quyDinhHS.TuoiToiThieu;
            TuoiToiDaHS = quyDinhHS.TuoiToiDa;
            TuoiToiThieuGV = quyDinhGV.TuoiToiThieu;
            TuoiToiDaGV = quyDinhGV.TuoiToiDa;

            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(Cancel);
        }

        private async void Save()
        {
            // Kiểm tra các giá trị nhập vào
            if (TuoiToiThieuHS <= 0 || TuoiToiDaHS <= 0 || TuoiToiThieuGV <= 0 || TuoiToiDaGV <= 0)
            {
                await ShowError("Lỗi nhập liệu", "Tất cả các giá trị tuổi phải lớn hơn 0.");
                return;
            }

            if (TuoiToiThieuHS >= TuoiToiDaHS)
            {
                await ShowError("Lỗi nhập liệu", "Tuổi tối thiểu của học sinh phải nhỏ hơn tuổi tối đa.");
                return;
            }

            if (TuoiToiThieuGV >= TuoiToiDaGV)
            {
                await ShowError("Lỗi nhập liệu", "Tuổi tối thiểu của giáo viên phải nhỏ hơn tuổi tối đa.");
                return;
            }

            // Lưu dữ liệu nếu hợp lệ
            _quyDinhHS.TuoiToiThieu = TuoiToiThieuHS;
            _quyDinhHS.TuoiToiDa = TuoiToiDaHS;
            _quyDinhGV.TuoiToiThieu = TuoiToiThieuGV;
            _quyDinhGV.TuoiToiDa = TuoiToiDaGV;

            QuyDinhTuoiDAL.UpdateQuyDinhTuoi(_quyDinhHS);
            QuyDinhTuoiDAL.UpdateQuyDinhTuoi(_quyDinhGV);

            
            await DialogHost.Show(new NotifyDialog("Thành công", "Đã cập nhật quy định tuổi."), "RootDialog_SuaTuoi");
            _window.DialogResult = true;
            _window.Close();
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
                await DialogHost.Show(new ErrorDialog(title, message), "RootDialog_SuaTuoi");
            }
            catch
            {
                MessageBox.Show(message, title, MessageBoxButton.OK,
                    title.Contains("Lỗi") ? MessageBoxImage.Error : MessageBoxImage.Information);
            }
        }
    }
}