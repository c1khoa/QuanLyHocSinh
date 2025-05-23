using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using QuanLyHocSinh.Model.Entities;
using MySql.Data.MySqlClient;

namespace QuanLyHocSinh.ViewModel.TraCuu
{
    public class SuaDiemViewModel : INotifyPropertyChanged
    {
        private string _maHS;
        public string MaHS
        {
            get => _maHS;
            set { _maHS = value; OnPropertyChanged(); }
        }

        private string _hoTen;
        public string HoTen
        {
            get => _hoTen;
            set { _hoTen = value; OnPropertyChanged(); }
        }

        private string _lop;    
        public string Lop
        {
            get => _lop;
            set { _lop = value; OnPropertyChanged(); }
        }

        private string _monHoc;
        public string MonHoc
        {
            get => _monHoc;
            set { _monHoc = value; OnPropertyChanged(); }
        }

        private float _diemMieng;
        public float DiemMieng
        {
            get => _diemMieng;
            set 
            { 
                if (value >= 0 && value <= 10)
                {
                    _diemMieng = value;
                    OnPropertyChanged();
                    TinhDiemTrungBinh();
                }
            }
        }

        private float _diem15p;
        public float Diem15p
        {
            get => _diem15p;
            set 
            { 
                if (value >= 0 && value <= 10)
                {
                    _diem15p = value;
                    OnPropertyChanged();
                    TinhDiemTrungBinh();
                }
            }
        }

        private float _diem1Tiet;
        public float Diem1Tiet
        {
            get => _diem1Tiet;
            set 
            { 
                if (value >= 0 && value <= 10)
                {
                    _diem1Tiet = value;
                    OnPropertyChanged();
                    TinhDiemTrungBinh();
                }
            }
        }

        private float _diemThi;
        public float DiemThi
        {
            get => _diemThi;
            set 
            { 
                if (value >= 0 && value <= 10)
                {
                    _diemThi = value;
                    OnPropertyChanged();
                    TinhDiemTrungBinh();
                }
            }
        }

        private float _diemTB;
        public float DiemTB
        {
            get => _diemTB;
            set { _diemTB = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> DanhSachMonHoc { get; } = 
            new ObservableCollection<string>(DiemDAL.GetAllMonHoc());

        public ObservableCollection<string> DanhSachLop { get; } = 
            new ObservableCollection<string>(DiemDAL.GetAllLop());

        public ICommand SaveCommandDiem { get; }
        public ICommand CancelCommandDiem { get; }

        public event Action<bool> CloseDialog;

        private readonly Diem _diemGoc;
        
        //Phần sửa điểm này dù không báo lỗi nhưng mà điểm không sửa được, tui sẽ tìm cách sau.
        public SuaDiemViewModel(Diem diem)
        {
            _diemGoc = diem;
            MaHS = diem.MaHS;
            HoTen = diem.HoTen;
            Lop = diem.Lop;
            MonHoc = diem.MonHoc;
            DiemMieng = diem.DiemMieng;
            Diem15p = diem.Diem15p;
            Diem1Tiet = diem.Diem1Tiet;
            DiemThi = diem.DiemThi;
            DiemTB = diem.DiemTB;

            SaveCommandDiem = new RelayCommand(Save);
            CancelCommandDiem = new RelayCommand(Cancel);
        }

        private void TinhDiemTrungBinh()
        {
            try
            {
                string connectionString = "Server=localhost;Database=quanlyhocsinh;Uid=khanghy1102;Pwd=khanghy1102;SslMode=none;";
                string query = @"
                    SELECT 
                        SUM(CASE WHEN LoaiDiemID = 'LD001' THEN HeSo ELSE 0 END) as HeSoMieng,
                        SUM(CASE WHEN LoaiDiemID = 'LD002' THEN HeSo ELSE 0 END) as HeSo15p,
                        SUM(CASE WHEN LoaiDiemID = 'LD003' THEN HeSo ELSE 0 END) as HeSo1Tiet,
                        SUM(CASE WHEN LoaiDiemID = 'LD004' THEN HeSo ELSE 0 END) as HeSoThi
                    FROM LOAIDIEM
                    WHERE LoaiDiemID IN ('LD001', 'LD002', 'LD003', 'LD004')";

                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                float heSoMieng = Convert.ToSingle(reader["HeSoMieng"]);
                                float heSo15p = Convert.ToSingle(reader["HeSo15p"]);
                                float heSo1Tiet = Convert.ToSingle(reader["HeSo1Tiet"]);
                                float heSoThi = Convert.ToSingle(reader["HeSoThi"]);

                                float tongHeSo = heSoMieng + heSo15p + heSo1Tiet + heSoThi;
                                float tongDiem = (DiemMieng * heSoMieng) + 
                                               (Diem15p * heSo15p) + 
                                               (Diem1Tiet * heSo1Tiet) + 
                                               (DiemThi * heSoThi);

                                DiemTB = (float)Math.Round(tongDiem / tongHeSo, 2);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tính điểm trung bình: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateDiem()
        {
            if (DiemMieng < 0 || DiemMieng > 10 ||
                Diem15p < 0 || Diem15p > 10 ||
                Diem1Tiet < 0 || Diem1Tiet > 10 ||
                DiemThi < 0 || DiemThi > 10)
            {
                MessageBox.Show("Điểm phải nằm trong khoảng từ 0 đến 10!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        private void Save()
        {
            if (!ValidateDiem())
                return;

            try
            {
                _diemGoc.MaHS = MaHS;
                _diemGoc.HoTen = HoTen;
                _diemGoc.Lop = Lop;
                _diemGoc.MonHoc = MonHoc;
                _diemGoc.DiemMieng = DiemMieng;
                _diemGoc.Diem15p = Diem15p;
                _diemGoc.Diem1Tiet = Diem1Tiet;
                _diemGoc.DiemThi = DiemThi;
                _diemGoc.DiemTB = DiemTB;

                DiemDAL.UpdateDiem(_diemGoc);
                MessageBox.Show("Cập nhật điểm thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                CloseDialog?.Invoke(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật điểm: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel()
        {
            CloseDialog?.Invoke(false);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}