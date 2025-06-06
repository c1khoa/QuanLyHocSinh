using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace QuanLyHocSinh.Model.Entities
{
    public class QuyDinhTuoi : INotifyPropertyChanged
    {
        private string _quyDinhTuoiID = string.Empty;
        public string QuyDinhTuoiID
        {
            get => _quyDinhTuoiID;
            set { _quyDinhTuoiID = value; OnPropertyChanged(); }
        }

        private int _tuoiToiThieu;
        public int TuoiToiThieu
        {
            get => _tuoiToiThieu;
            set { _tuoiToiThieu = value; OnPropertyChanged(); }
        }

        private int _tuoiToiDa;
        public int TuoiToiDa
        {
            get => _tuoiToiDa;
            set { _tuoiToiDa = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}