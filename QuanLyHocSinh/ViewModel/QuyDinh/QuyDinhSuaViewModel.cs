using System;
using System.ComponentModel;
using System.Configuration;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using MySql.Data.MySqlClient;
using QuanLyHocSinh.Model.Entities;
using QuanLyHocSinh.ViewModel;

public class QuyDinhSuaViewModel : BaseViewModel
{
    private string _quyDinhID;
    private string _quyDinhTuoiID;

    private int _tuoiToiThieu;
    private int _tuoiToiDa;
    private int _siSoLop;
    private int _soLuongMonHoc;
    private float _diemDat;
    private string _quyDinhKhac;

    public string QuyDinhID
    {
        get => _quyDinhID;
        set { _quyDinhID = value; OnPropertyChanged(); }
    }

    public string QuyDinhTuoiID
    {
        get => _quyDinhTuoiID;
        set { _quyDinhTuoiID = value; OnPropertyChanged(); }
    }

    public int TuoiToiThieu
    {
        get => _tuoiToiThieu;
        set { _tuoiToiThieu = value; OnPropertyChanged(); }
    }

    public int TuoiToiDa
    {
        get => _tuoiToiDa;
        set { _tuoiToiDa = value; OnPropertyChanged(); }
    }

    public int SiSoLop
    {
        get => _siSoLop;
        set { _siSoLop = value; OnPropertyChanged(); }
    }

    public int SoLuongMonHoc
    {
        get => _soLuongMonHoc;
        set { _soLuongMonHoc = value; OnPropertyChanged(); }
    }

    public float DiemDat
    {
        get => _diemDat;
        set { _diemDat = value; OnPropertyChanged(); }
    }

    public string QuyDinhKhac
    {
        get => _quyDinhKhac;
        set { _quyDinhKhac = value; OnPropertyChanged(); }
    }

    private readonly QuyDinh _quyDinhGoc;
    public ICommand UpdateCommand { get; }
    public ICommand CancelCommand { get; }

    public event Action CloseRequested;

    public QuyDinhSuaViewModel(QuyDinh quyDinhHienTai)
    {
        _quyDinhGoc = quyDinhHienTai;
        if (quyDinhHienTai == null)
            throw new ArgumentNullException(nameof(quyDinhHienTai));

        QuyDinhID = quyDinhHienTai.QuyDinhID;
        QuyDinhTuoiID = quyDinhHienTai.QuyDinhID; 
        TuoiToiThieu = quyDinhHienTai.TuoiToiThieu;
        TuoiToiDa = quyDinhHienTai.TuoiToiDa;
        SiSoLop = quyDinhHienTai.SiSoLop;
        SoLuongMonHoc = quyDinhHienTai.SoLuongMonHoc;
        DiemDat = (float)quyDinhHienTai.DiemDat;
        QuyDinhKhac = quyDinhHienTai.QuyDinhKhac;

        UpdateCommand = new RelayCommand(CapNhat);
        CancelCommand = new RelayCommand(() => CloseRequested?.Invoke());
    }

    private void CapNhat()
    {
        try
        {
			string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                string queryTuoi = @"
                    UPDATE QUYDINHTUOI SET 
                        TuoiToiThieu = @TuoiToiThieu,
                        TuoiToiDa = @TuoiToiDa
                    WHERE QuyDinhTuoiID = @QuyDinhTuoiID;";

                using (var cmdTuoi = new MySqlCommand(queryTuoi, conn))
                {
                    cmdTuoi.Parameters.AddWithValue("@TuoiToiThieu", TuoiToiThieu);
                    cmdTuoi.Parameters.AddWithValue("@TuoiToiDa", TuoiToiDa);
                    cmdTuoi.Parameters.AddWithValue("@QuyDinhTuoiID", QuyDinhTuoiID);
                    cmdTuoi.ExecuteNonQuery();
                }

                string queryQuyDinh = @"
                    UPDATE QUYDINH SET 
                        SiSoLop = @SiSoLop,
                        SoLuongMonHoc = @SoLuongMonHoc,
                        DiemDat = @DiemDat,
                        QuyDinhKhac = @QuyDinhKhac
                    WHERE QuyDinhID = @QuyDinhID;";

                using (var cmdQuyDinh = new MySqlCommand(queryQuyDinh, conn))
                {
                    cmdQuyDinh.Parameters.AddWithValue("@SiSoLop", SiSoLop);
                    cmdQuyDinh.Parameters.AddWithValue("@SoLuongMonHoc", SoLuongMonHoc);
                    cmdQuyDinh.Parameters.AddWithValue("@DiemDat", DiemDat);
                    cmdQuyDinh.Parameters.AddWithValue("@QuyDinhKhac", QuyDinhKhac);
                    cmdQuyDinh.Parameters.AddWithValue("@QuyDinhID", QuyDinhID);
                    cmdQuyDinh.ExecuteNonQuery();
                }
            }

            _quyDinhGoc.TuoiToiThieu = TuoiToiThieu;
            _quyDinhGoc.TuoiToiDa = TuoiToiDa;
            _quyDinhGoc.SiSoLop = SiSoLop;
            _quyDinhGoc.SoLuongMonHoc = SoLuongMonHoc;
            _quyDinhGoc.DiemDat = DiemDat;
            _quyDinhGoc.QuyDinhKhac = QuyDinhKhac;

            MessageBox.Show("Cập nhật thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            CloseRequested?.Invoke();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Lỗi cập nhật: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
