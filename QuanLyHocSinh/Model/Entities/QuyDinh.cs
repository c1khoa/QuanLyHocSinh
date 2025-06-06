using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace QuanLyHocSinh.Model.Entities
{
    public class QuyDinh : INotifyPropertyChanged
    {
        private int _iD;
        public int ID
        {
            get => _iD;
            set { _iD = value; OnPropertyChanged(); }
        }

        private string _quyDinhID = string.Empty;
        public string QuyDinhID
        {
            get => _quyDinhID;
            set { _quyDinhID = value; OnPropertyChanged(); }
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

        private int _siSoLop;
        public int SiSoLop
        {
            get => _siSoLop;
            set { _siSoLop = value; OnPropertyChanged(); }
        }

        private int _soLuongMonHoc;
        public int SoLuongMonHoc
        {
            get => _soLuongMonHoc;
            set { _soLuongMonHoc = value; OnPropertyChanged(); }
        }

        private double _diemDat;
        public double DiemDat
        {
            get => _diemDat;
            set { _diemDat = value; OnPropertyChanged(); }
        }

        private string _quyDinhKhac = string.Empty;
        public string QuyDinhKhac
        {
            get => _quyDinhKhac;
            set { _quyDinhKhac = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return $"TuoiToiThieu: {TuoiToiThieu}, TuoiToiDa: {TuoiToiDa}\n SiSoLop: {SiSoLop}, SoLuongMonHoc: {SoLuongMonHoc}, DiemDat: {DiemDat}\n QuyDinhKhac: {QuyDinhKhac}";
        }
    }
}