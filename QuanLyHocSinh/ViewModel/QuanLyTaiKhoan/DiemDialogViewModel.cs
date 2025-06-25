using MySql.Data.MySqlClient;
using QuanLyHocSinh.Model.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace QuanLyHocSinh.ViewModel.QuanLyTaiKhoan
{
    public class DiemDialogViewModel : BaseViewModel
    {
        public User CurrentUser { get; set; }

        public class DiemTongHopHocSinh
        {
            public string MonHoc { get; set; }
            public string Mieng { get; set; }
            public string Diem15Phut { get; set; }
            public string Diem1Tiet { get; set; }
            public string DiemHocKy { get; set; }
            public int HocKy { get; set; }
            public double TrungBinh { get; set; }
            public string NamHocID { get; set; }
        }

        private ObservableCollection<DiemTongHopHocSinh> _allDiem;
        public ObservableCollection<string> dsNamHoc { get; set; }
        public ObservableCollection<int> dsHocKy { get; set; }

        private ObservableCollection<DiemTongHopHocSinh> _filteredDiem;
        public ObservableCollection<DiemTongHopHocSinh> FilteredDiem
        {
            get => _filteredDiem;
            set { _filteredDiem = value; OnPropertyChanged(); }
        }

        private int? _selectedHocKyFilter;
        public int? SelectedHocKyFilter
        {
            get => _selectedHocKyFilter;
            set
            {
                _selectedHocKyFilter = value;
                OnPropertyChanged();
                FilterData();
            }
        }

        private string _selectedNamHocFilter;
        public string SelectedNamHocFilter
        {
            get => _selectedNamHocFilter;
            set
            {
                _selectedNamHocFilter = value;
                OnPropertyChanged();
                FilterData();
            }
        }

        private DiemTongHopHocSinh _selectedItem;
        public DiemTongHopHocSinh SelectedItem
        {
            get => _selectedItem;
            set { _selectedItem = value; OnPropertyChanged(); }
        }

        public DiemDialogViewModel(User currentUser)
        {
            CurrentUser = currentUser;

            // Lấy dữ liệu từ CSDL
            dsNamHoc = new ObservableCollection<string>(HocSinhDAL.GetAllNamHoc());
            dsHocKy = new ObservableCollection<int>(HocSinhDAL.GetAllHocKy());

            // Lấy điểm tổng hợp
            _allDiem = new ObservableCollection<DiemTongHopHocSinh>(
                GetDiemTongHopCuaHocSinh(currentUser.TenDangNhap)
            );

            // Gán mặc định bộ lọc
            SelectedHocKyFilter = dsHocKy.FirstOrDefault();         // ✅ kiểu int
            SelectedNamHocFilter = dsNamHoc.FirstOrDefault();       // ✅ kiểu string

            // Gán ban đầu
            FilteredDiem = new ObservableCollection<DiemTongHopHocSinh>(_allDiem);
        }

        private void FilterData()
        {
            if (_allDiem == null) return;

            var filtered = _allDiem.Where(d =>
                (!SelectedHocKyFilter.HasValue || d.HocKy == SelectedHocKyFilter.Value) &&
                (string.IsNullOrEmpty(SelectedNamHocFilter) || d.NamHocID == SelectedNamHocFilter)
            );

            FilteredDiem = new ObservableCollection<DiemTongHopHocSinh>(filtered);
        }

        public static List<DiemTongHopHocSinh> GetDiemTongHopCuaHocSinh(string hocSinhID)
        {
            var list = new List<DiemTongHopHocSinh>();

            string query = @"
                SELECT 
                    mh.TenMonHoc AS MonHoc,
                    d.NamHocID,
                    GROUP_CONCAT(CASE WHEN ld.LoaiDiemID = 'LD01' THEN ctd.GiaTri END) AS Mieng,
                    GROUP_CONCAT(CASE WHEN ld.LoaiDiemID = 'LD02' THEN ctd.GiaTri END) AS `15Phut`,
                    GROUP_CONCAT(CASE WHEN ld.LoaiDiemID = 'LD03' THEN ctd.GiaTri END) AS `1Tiet`,
                    GROUP_CONCAT(CASE WHEN ld.LoaiDiemID = 'LD04' THEN ctd.GiaTri END) AS Thi,
                    d.HocKy,
                    ROUND(SUM(ctd.GiaTri * ld.HeSo) / SUM(ld.HeSo), 2) AS TrungBinh
                FROM 
                    CHITIETDIEM ctd
                JOIN DIEM d ON ctd.DiemID = d.DiemID
                JOIN MONHOC mh ON d.MonHocID = mh.MonHocID
                JOIN LOAIDIEM ld ON ctd.LoaiDiemID = ld.LoaiDiemID
                JOIN HOCSINH hs ON d.HocSinhID = hs.HocSinhID
                WHERE hs.HocSinhID = @HocSinhID
                GROUP BY mh.TenMonHoc, d.HocKy, d.NamHocID
                ORDER BY d.HocKy, mh.TenMonHoc;
            ";

            string connectionString = System.Configuration.ConfigurationManager
                .ConnectionStrings["MySqlConnection"].ConnectionString;

            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@HocSinhID", hocSinhID);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new DiemTongHopHocSinh
                            {
                                MonHoc = reader["MonHoc"]?.ToString(),
                                Mieng = reader["Mieng"]?.ToString(),
                                Diem15Phut = reader["15Phut"]?.ToString(),
                                Diem1Tiet = reader["1Tiet"]?.ToString(),
                                DiemHocKy = reader["Thi"]?.ToString(),
                                NamHocID = reader["NamHocID"]?.ToString(),
                                HocKy = Convert.ToInt32(reader["HocKy"]),
                                TrungBinh = reader["TrungBinh"] != DBNull.Value
                                    ? Convert.ToDouble(reader["TrungBinh"])
                                    : 0.0
                            });
                        }
                    }
                }
            }

            return list;
        }
    }
}
