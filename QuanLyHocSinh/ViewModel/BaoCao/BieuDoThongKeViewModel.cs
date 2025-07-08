using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using LiveCharts;
using LiveCharts.Wpf;
using QuanLyHocSinh.Model.DAL;
using QuanLyHocSinh.Model.Entities;
using QuanLyHocSinh.ViewModel.TraCuu;

namespace QuanLyHocSinh.ViewModel.BaoCao
{
    public class BieuDoThongKeViewModel : BaseViewModel
    {
        #region Properties

        public bool IsThongKeHocKy { get; set; } = true;
        public bool IsThongKeMonHoc => !IsThongKeHocKy;

        private string _currentNamHoc; 
        public string CurrentNamHoc 
        { 
            get => _currentNamHoc; 
            set { _currentNamHoc = value; OnPropertyChanged(nameof(CurrentNamHoc)); } 
        }

        public ObservableCollection<string> DanhSachHocKy { get; set; }
        public ObservableCollection<string> DanhSachLop { get; set; }
        public ObservableCollection<string> DanhSachMonHoc { get; set; }

        public string SelectedHocKy { get; set; } = "Học kỳ 2";
        public string SelectedLop { get; set; } = "10A1"; 
        public string SelectedMonHoc { get; set; } = "Đạo Đức";

        public SeriesCollection ChartSeries { get; set; }
        public string[] ChartLabels { get; set; }
        public string ChartTitle { get; set; } = "Thống kê xếp loại học sinh";
        
        public Func<double, string> YAxisFormatter { get; set; } = value => value.ToString("0");

        #endregion

        #region Commands

        public ICommand SwitchToHocKyCommand { get; }
        public ICommand SwitchToMonHocCommand { get; }
        public ICommand RefreshChartCommand { get; }

        #endregion

        #region Constructor

        private MainViewModel _mainVM;

        public BieuDoThongKeViewModel()
        {
            System.Diagnostics.Debug.WriteLine("DEBUG: BieuDoThongKeViewModel parameterless constructor called");
            
            SwitchToHocKyCommand = new RelayCommand<object>((p) => true, (p) => { });
            SwitchToMonHocCommand = new RelayCommand<object>((p) => true, (p) => { });
            RefreshChartCommand = new RelayCommand<object>((p) => true, (p) => { });

            LoadStaticData();
            LoadStaticChart();
        }

        public BieuDoThongKeViewModel(MainViewModel mainVM) : this()
        {
            _mainVM = mainVM;
            System.Diagnostics.Debug.WriteLine("DEBUG: BieuDoThongKeViewModel with MainViewModel constructor called");
        }

        #endregion

        #region Methods

        private void LoadStaticData()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("DEBUG: Starting LoadStaticData - auto select latest year...");
                
                System.Diagnostics.Debug.WriteLine("DEBUG: Auto-selecting latest năm học...");
                var namHocList = TongKetMonDAL.GetAllNamHoc();
                if (namHocList?.Count > 0)
                {
                    CurrentNamHoc = namHocList.OrderByDescending(n => n).FirstOrDefault() ?? "NH2025";
                }
                else
                {
                    CurrentNamHoc = "NH2025"; // Fallback
                }
                System.Diagnostics.Debug.WriteLine($"DEBUG: Auto-selected năm học: '{CurrentNamHoc}'");

                System.Diagnostics.Debug.WriteLine("DEBUG: Loading Học kỳ data without 'Tất cả'...");
                DanhSachHocKy = new ObservableCollection<string> { "Học kỳ 1", "Học kỳ 2" };
                System.Diagnostics.Debug.WriteLine($"DEBUG: Loaded {DanhSachHocKy.Count} học kỳ items");

                System.Diagnostics.Debug.WriteLine("DEBUG: Loading Lớp data from TongKetMonDAL...");
                var lopList = TongKetMonDAL.GetAllLop();
                if (lopList?.Count > 0)
                {
                }
                else
                {
                    lopList = new List<string> { "10A1", "10A2", "11A1", "12A1" }; // Fallback không có "Tất cả"
                }
                DanhSachLop = new ObservableCollection<string>(lopList);
                System.Diagnostics.Debug.WriteLine($"DEBUG: Loaded {DanhSachLop.Count} lớp items: {string.Join(", ", DanhSachLop.Take(5))}");

                System.Diagnostics.Debug.WriteLine("DEBUG: Loading Môn học data from TongKetMonDAL...");
                var monHocList = TongKetMonDAL.GetAllMonHoc();
                if (monHocList?.Count > 0)
                {
                }
                else
                {
                    monHocList = new List<string> { "Đạo Đức", "Toán học", "Vật lý", "Hóa học", "Sinh học", "Ngữ văn" }; // Fallback có Đạo Đức đầu tiên
                }
                DanhSachMonHoc = new ObservableCollection<string>(monHocList);
                System.Diagnostics.Debug.WriteLine($"DEBUG: Loaded {DanhSachMonHoc.Count} môn học items: {string.Join(", ", DanhSachMonHoc.Take(5))}");

                SelectedHocKy = "Học kỳ 2"; 
                SelectedLop = DanhSachLop.FirstOrDefault() ?? "10A1"; 
                SelectedMonHoc = DanhSachMonHoc.FirstOrDefault() ?? "Đạo Đức"; 

                System.Diagnostics.Debug.WriteLine($"DEBUG: Default selections - CurrentNamHoc: '{CurrentNamHoc}', HocKy: '{SelectedHocKy}', Lop: '{SelectedLop}', MonHoc: '{SelectedMonHoc}'");
                System.Diagnostics.Debug.WriteLine("DEBUG: Static data loaded successfully with defaults (no 'Tất cả' options)");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR LoadStaticData: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"ERROR Stack trace: {ex.StackTrace}");
                
                CurrentNamHoc = "NH2025";
                DanhSachHocKy = new ObservableCollection<string> { "Học kỳ 1", "Học kỳ 2" };
                DanhSachLop = new ObservableCollection<string> { "10A1" };
                DanhSachMonHoc = new ObservableCollection<string> { "Đạo Đức" };
                
                SelectedHocKy = "Học kỳ 2";
                SelectedLop = "10A1";
                SelectedMonHoc = "Đạo Đức";
            }
        }

        private void LoadStaticChart()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("DEBUG: Loading initial chart data with GetThongKeHocSinhTheoXepLoai...");
                
                var data = TongKetMonDAL.GetThongKeHocSinhTheoXepLoai(null, null, null, null);
                
                System.Diagnostics.Debug.WriteLine($"DEBUG: Student count data: {string.Join(", ", data.Select(kv => $"{kv.Key}={kv.Value}"))}");
                
                bool hasRealData = data.Values.Sum() > 0;
                
                if (hasRealData)
                {
                    System.Diagnostics.Debug.WriteLine("DEBUG: Using REAL student count data for chart");

                    ChartSeries = new SeriesCollection
                    {
                        new ColumnSeries
                        {
                            Title = "Số lượng học sinh",
                            Values = new ChartValues<int> 
                            { 
                                data.ContainsKey("Giỏi") ? Math.Max(0, data["Giỏi"]) : 0,
                                data.ContainsKey("Khá") ? Math.Max(0, data["Khá"]) : 0,
                                data.ContainsKey("Trung bình") ? Math.Max(0, data["Trung bình"]) : 0,
                                data.ContainsKey("Yếu") ? Math.Max(0, data["Yếu"]) : 0
                            },
                            DataLabels = true
                        }
                    };
                    
                    ChartTitle = "Thống kê xếp loại học sinh";
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("WARNING: No real student data found, using demo fallback");
                    
                    ChartSeries = new SeriesCollection
                    {
                        new ColumnSeries
                        {
                            Title = "Demo Data",
                            Values = new ChartValues<int> { 15, 25, 30, 10 },
                            DataLabels = true
                        }
                    };
                    
                    ChartTitle = "Thống kê xếp loại học sinh (Demo)";
                }

                ChartLabels = new[] { "Giỏi", "Khá", "Trung bình", "Yếu" };
                System.Diagnostics.Debug.WriteLine("DEBUG: Chart data loaded successfully with real student counts (4 categories only)");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR LoadStaticChart: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"ERROR Stack trace: {ex.StackTrace}");
                
                    
                ChartSeries = new SeriesCollection
                {
                    new ColumnSeries
                    {
                        Title = "Không có dữ liệu",
                        Values = new ChartValues<int> { 0, 0, 0, 0 },
                        DataLabels = false
                    }
                };
                ChartLabels = new[] { "Giỏi", "Khá", "Trung bình", "Yếu" };
                ChartTitle = "Lỗi khi tải biểu đồ";
            }
        }

        #endregion
    }
}
