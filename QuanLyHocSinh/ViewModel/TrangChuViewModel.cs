using LiveCharts.Wpf;
using LiveCharts;
using MySql.Data.MySqlClient;
using QuanLyHocSinh.Model.Entities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyHocSinh.ViewModel
{
    public class TrangChuViewModel : BaseViewModel
    {
        private MainViewModel _mainVM;
        public static string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
        public SeriesCollection GioiTinhSeries { get; set; }
        public SeriesCollection XepLoaiPieSeries { get; set; }
        public SeriesCollection MonHocSeries { get; set; }
        public Func<double, string> Formatter { get; set; } = value => value.ToString("0.00");
        public List<string> MonHocLabels { get; set; }

        private int _soLuongHocSinh;
        public int SoLuongHocSinh
        {
            get => _soLuongHocSinh;
            set
            {
                if (_soLuongHocSinh != value)
                {
                    _soLuongHocSinh = value;
                    OnPropertyChanged(nameof(SoLuongHocSinh));
                }
            }
        }
        private int _soLuongLop;
        public int SoLuongLop
        {
            get => _soLuongLop;
            set
            {
                if (_soLuongLop != value)
                {
                    _soLuongLop = value;
                    OnPropertyChanged(nameof(SoLuongLop));
                }
            }
        }

        private int _soLuongGiaoVien;
        public int SoLuongGiaoVien
        {
            get => _soLuongGiaoVien;
            set
            {
                if (_soLuongGiaoVien != value)
                {
                    _soLuongGiaoVien = value;
                    OnPropertyChanged(nameof(SoLuongGiaoVien));
                }
            }
        }
        public void LoadThongtin()
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();
            using var cmd1 = new MySqlCommand("SELECT COUNT(*) FROM HOCSINH", conn);
            var countHs = Convert.ToInt32(cmd1.ExecuteScalar());
            SoLuongHocSinh = countHs;
            using var cmd2 = new MySqlCommand("SELECT COUNT(*) FROM LOP", conn);
            var countLop = Convert.ToInt32(cmd2.ExecuteScalar());
            SoLuongLop = countLop;
            using var cmd3 = new MySqlCommand("SELECT COUNT(*) FROM GIAOVIEN", conn);
            var countGv = Convert.ToInt32(cmd3.ExecuteScalar());
            SoLuongGiaoVien = countGv;
        }

        private void LoadGioiTinh()
        {
            var (soNam, soNu) = HocSinhDAL.GetThongKeGioiTinh();

            GioiTinhSeries = new SeriesCollection
            {
                new PieSeries
                {
                    Title = "Nam",
                    Values = new ChartValues<int> { soNam },
                    DataLabels = true
                },
                new PieSeries
                {
                    Title = "Nữ",
                    Values = new ChartValues<int> { soNu },
                    DataLabels = true
                }
            };

            OnPropertyChanged(nameof(GioiTinhSeries));
        }

        private void LoadThongKeXepLoai()
        {
            var data = DiemDAL.GetThongKeXepLoai();

            XepLoaiPieSeries = new SeriesCollection();

            foreach (var item in data)
            {
                XepLoaiPieSeries.Add(new PieSeries
                {
                    Title = item.Key,
                    Values = new ChartValues<int> { item.Value },
                    DataLabels = true
                });
            }

            OnPropertyChanged(nameof(XepLoaiPieSeries));
        }

        private void LoadDiemTheoKhoi()
        {
            var data = DiemDAL.GetDiemTrungBinhTheoKhoi(); // DAL trả về Dictionary<string, Dictionary<string, float>>
            MonHocSeries = new SeriesCollection();
            MonHocLabels = new List<string>();

            // Tập hợp toàn bộ tên môn học
            var allMon = data.SelectMany(kv => kv.Value.Keys).Distinct().OrderBy(m => m).ToList();
            MonHocLabels = allMon;

            foreach (var khoi in data.Keys.OrderBy(k => k))
            {
                var values = new ChartValues<float>();
                foreach (var mon in allMon)
                {
                    float diem = data[khoi].ContainsKey(mon) ? data[khoi][mon] : 0;
                    values.Add((float)Math.Round(diem, 2)); // Làm tròn 2 chữ số

                }

                MonHocSeries.Add(new LineSeries
                {
                    Title = $"Khối {khoi}",
                    Values = values,
                    PointGeometry = DefaultGeometries.Circle,
                    PointGeometrySize = 8
                });
            }

            OnPropertyChanged(nameof(MonHocSeries));
            OnPropertyChanged(nameof(MonHocLabels));
        }



        public TrangChuViewModel(MainViewModel _mainVM)
        {
            this._mainVM = _mainVM;
            LoadThongtin();
            LoadGioiTinh();
            LoadThongKeXepLoai();
            LoadDiemTheoKhoi();
        }
    }
}
