using QuanLyHocSinh.Model.Entities;
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

        private void Save()
        {
            _quyDinhHS.TuoiToiThieu = TuoiToiThieuHS;
            _quyDinhHS.TuoiToiDa = TuoiToiDaHS;
            _quyDinhGV.TuoiToiThieu = TuoiToiThieuGV;
            _quyDinhGV.TuoiToiDa = TuoiToiDaGV;

            QuyDinhTuoiDAL.UpdateQuyDinhTuoi(_quyDinhHS);
            QuyDinhTuoiDAL.UpdateQuyDinhTuoi(_quyDinhGV);

            _window.DialogResult = true;
            _window.Close();
        }

        private void Cancel()
        {
            _window.DialogResult = false;
            _window.Close();
        }
    }
}