using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using QuanLyHocSinh.ViewModel.BaoCao;
using LiveCharts;
using LiveCharts.Wpf;
using QuanLyHocSinh.Model.Entities;

namespace QuanLyHocSinh.View.Controls.BaoCao
{
    /// <summary>
    /// Interaction logic for BieuDoThongKeUC.xaml
    /// </summary>
    public partial class BieuDoThongKeUC : UserControl
    {
        private BieuDoThongKeViewModel _viewModel;
        private bool _isHocKyMode = true; 

        public BieuDoThongKeUC()
        {
            try
        {
            InitializeComponent();
                
                _viewModel = DataContext as BieuDoThongKeViewModel;
                if (_viewModel == null)
                {
                    try
                    {
                        _viewModel = new BieuDoThongKeViewModel();
                        DataContext = _viewModel;
                    }
                    catch (Exception vmEx)
                    {
                    }
                }
                else
                {
                }
                
                InitializeUI();
                
                this.Loaded += (sender, e) => {
                    LoadInitialChart();
                };
                
            }
            catch (Exception ex)
            {
            }
        }
        
        private void InitializeUI()
        {
            try
            {
                if (ToggleHocKy != null)
                    ToggleHocKy.IsChecked = true;
                if (ToggleMonHoc != null)
                    ToggleMonHoc.IsChecked = false;
                
                if (PanelLop != null)
                    PanelLop.Visibility = Visibility.Collapsed;      
                if (PanelHocKy != null)
                    PanelHocKy.Visibility = Visibility.Visible;      
                if (PanelMonHoc != null)
                    PanelMonHoc.Visibility = Visibility.Collapsed;   
                if (BieuDoChart != null)
                    BieuDoChart.Visibility = Visibility.Visible;
                if (PieChart != null)
                    PieChart.Visibility = Visibility.Visible;
                
            }
            catch (Exception ex)
            {
            }
        }
        
        private void LoadInitialChart()
        {
            try
            {
                
                LogViewModelData();
                LogCurrentSelections();
                
                
                if (BieuDoChart != null)
                    BieuDoChart.Visibility = Visibility.Visible;
                if (PieChart != null)
                    PieChart.Visibility = Visibility.Visible;
                
                
                if (_isHocKyMode)
                {
                    LoadHocKyChart();
                }
                else
                {
                    LoadMonHocChart();
                }
                
            }
            catch (Exception ex)
            {   
                
                LoadEmptyChart();
            }
        }
        
        #region Event Handlers
        
        private void OnToggleHocKy_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                
                
                _isHocKyMode = true;
                
                
                if (ToggleMonHoc != null)
                    ToggleMonHoc.IsChecked = false;
                
                
                if (PanelLop != null)
                    PanelLop.Visibility = Visibility.Collapsed;      
                if (PanelHocKy != null)
                    PanelHocKy.Visibility = Visibility.Visible;      
                if (PanelMonHoc != null)
                    PanelMonHoc.Visibility = Visibility.Collapsed;   
                
                
                if (BieuDoChart != null)
                    BieuDoChart.Visibility = Visibility.Visible;
                if (PieChart != null)
                    PieChart.Visibility = Visibility.Visible;
                
                
                LoadHocKyChart();
            }
            catch (Exception ex)
            {
            }
        }
        
        private void OnToggleMonHoc_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                
                _isHocKyMode = false;
                
                
                if (ToggleHocKy != null)
                    ToggleHocKy.IsChecked = false;
                
                
                if (PanelLop != null)
                    PanelLop.Visibility = Visibility.Visible;        
                if (PanelHocKy != null)
                    PanelHocKy.Visibility = Visibility.Visible;      
                if (PanelMonHoc != null)
                    PanelMonHoc.Visibility = Visibility.Visible;     
                
                if (BieuDoChart != null)
                    BieuDoChart.Visibility = Visibility.Visible;
                if (PieChart != null)
                    PieChart.Visibility = Visibility.Visible;
                
                
                LoadMonHocChart();
            }
            catch (Exception ex)
            {
            }
        }
        
        private void OnComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                
                if (IsLoaded)
                {
                    var comboBox = sender as ComboBox;
                    string comboName = comboBox?.Name ?? "Unknown";
                    string selectedValue = comboBox?.SelectedItem?.ToString() ?? "null";
                    LogViewModelData();
                    LogCurrentSelections();
                    RefreshChart();
                    
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
            }
        }
        
        private void LogViewModelData()
        {
            try
            {
                if (_viewModel == null)
                {
                    return;
                }
                
                
                if (_viewModel.DanhSachLop?.Count > 0)
                {
                }
            }
            catch (Exception ex)
            {
            }
        }
        
        private void LogCurrentSelections()
        {
            try
            {
                string namHoc = _viewModel?.CurrentNamHoc ?? "null";
                string lop = _viewModel?.SelectedLop ?? "null";
                string hocKy = _viewModel?.SelectedHocKy ?? "null";
                string monHoc = _viewModel?.SelectedMonHoc ?? "null";
                
            }
            catch (Exception ex)
            {
            }
        }
        
        private void OnRefresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RefreshChart();
            }
            catch (Exception ex)
            {
            }
        }
        
        #endregion
        #region Chart Loading Methods
        
        private void RefreshChart()
        {
            if (_isHocKyMode)
            {
                LoadHocKyChart();
            }
            else
            {
                LoadMonHocChart();
            }
        }
        
        private void LoadHocKyChart()
        {
            try
            {
                
                if (BieuDoChart == null)
                {
                    
                    
                    this.Dispatcher.BeginInvoke(new Action(() => {
                        LoadHocKyChart();
                    }), System.Windows.Threading.DispatcherPriority.Loaded);
                    return;
                }
                
                
                string namHoc = _viewModel?.CurrentNamHoc ?? "NH2025";
                string hocKy = _viewModel?.SelectedHocKy ?? "Học kỳ 2";
                
                
                
                int hocKyFilter = (hocKy == "Học kỳ 1") ? 1 : 2;
                
                
                string namHocFilter = namHoc;
                
                
                var data = TongKetMonDAL.GetThongKeHocSinhTheoXepLoai(namHocFilter, null, hocKyFilter, null);   
                
                SeriesCollection chartSeries;
                string chartTitle = $"Thống kê xếp loại học sinh - {namHoc} - {hocKy}";
                
                
                bool hasData = data.Values.Sum() > 0;
                
                if (hasData)
                {
                    
                    var gioiColor = System.Windows.Media.Brushes.Gold;         
                    var khaColor = System.Windows.Media.Brushes.LimeGreen;     
                    var trungBinhColor = System.Windows.Media.Brushes.DeepSkyBlue; 
                    var yeuColor = System.Windows.Media.Brushes.Orange;        
                    
                    chartSeries = new SeriesCollection
                    {
                        new ColumnSeries
                        {
                            Title = "Giỏi",
                            Values = new ChartValues<int> { data.ContainsKey("Giỏi") ? Math.Max(0, data["Giỏi"]) : 0 },
                            DataLabels = true,
                            MaxColumnWidth = 120, 
                            Fill = gioiColor,
                            Stroke = System.Windows.Media.Brushes.DarkGoldenrod,
                            StrokeThickness = 2
                        },
                        new ColumnSeries
                        {
                            Title = "Khá", 
                            Values = new ChartValues<int> { data.ContainsKey("Khá") ? Math.Max(0, data["Khá"]) : 0 },
                            DataLabels = true,
                            MaxColumnWidth = 120,
                            Fill = khaColor,
                            Stroke = System.Windows.Media.Brushes.Green,
                            StrokeThickness = 2
                        },
                        new ColumnSeries
                        {
                            Title = "Trung bình",
                            Values = new ChartValues<int> { data.ContainsKey("Trung bình") ? Math.Max(0, data["Trung bình"]) : 0 },
                            DataLabels = true,
                            MaxColumnWidth = 120,
                            Fill = trungBinhColor,
                            Stroke = System.Windows.Media.Brushes.RoyalBlue,
                            StrokeThickness = 2
                        },
                        new ColumnSeries
                        {
                            Title = "Yếu",
                            Values = new ChartValues<int> { data.ContainsKey("Yếu") ? Math.Max(0, data["Yếu"]) : 0 },
                            DataLabels = true,
                            MaxColumnWidth = 120,
                            Fill = yeuColor,
                            Stroke = System.Windows.Media.Brushes.DarkOrange,
                            StrokeThickness = 2
                        }
                    };
                    
                    LoadPieChartData(data);
                }
                else
                {
                    chartSeries = new SeriesCollection
                    {
                        new ColumnSeries
                        {
                            Title = "Không có dữ liệu",
                            Values = new ChartValues<int> { 0, 0, 0, 0 },
                            DataLabels = false,
                            MaxColumnWidth = 120,
                            Fill = System.Windows.Media.Brushes.LightGray
                        }
                    };
                    chartTitle += " (Chưa có dữ liệu)";
                    
                    
                    if (PieChart != null)
                    {
                        PieChart.Series = new SeriesCollection();
                    }
                }
                
                
                if (BieuDoChart != null)
                {
                    BieuDoChart.Series = chartSeries;
                
                    
                    BieuDoChart.AxisX.Clear();
                    BieuDoChart.AxisY.Clear();
                    
                    BieuDoChart.AxisX.Add(new Axis
                    {
                        Title = "Xếp loại",
                        Labels = new[] { "Giỏi", "Khá", "Trung bình", "Yếu" },
                        FontSize = 14,
                        FontWeight = FontWeights.SemiBold,
                        ShowLabels = false  
                    });
                    
                    BieuDoChart.AxisY.Add(new Axis
                    {
                        Title = "Số lượng học sinh",
                        LabelFormatter = value => Math.Max(0, (int)Math.Round(value)).ToString(),
                        MinValue = 0,
                        FontSize = 14,
                        FontWeight = FontWeights.SemiBold
                    });
                    
                    
                    BieuDoChart.LegendLocation = LegendLocation.Bottom;
                    BieuDoChart.Hoverable = true;
                    BieuDoChart.DisableAnimations = false;
                }
                
                
                if (_viewModel != null)
                {
                    _viewModel.ChartTitle = chartTitle;
                }
                
            }
            catch (Exception ex)
            {
                LoadEmptyChart();
            }
        }
        
        private void LoadMonHocChart()
        {
            try
            {
                
                if (BieuDoChart == null)
                {
                    
                    this.Dispatcher.BeginInvoke(new Action(() => {
                        LoadMonHocChart();
                    }), System.Windows.Threading.DispatcherPriority.Loaded);
                    return;
                }
                
                
                string namHoc = _viewModel?.CurrentNamHoc ?? "NH2025";
                string lop = _viewModel?.SelectedLop ?? "10A1";
                string monHoc = _viewModel?.SelectedMonHoc ?? "Đạo Đức";
                string hocKy = _viewModel?.SelectedHocKy ?? "Học kỳ 2";
                
                
                string namHocFilter = namHoc; 
                string lopFilter = lop; 
                string monHocFilter = monHoc; 
                int hocKyFilter = (hocKy == "Học kỳ 1") ? 1 : 2;
                

                var data = TongKetMonDAL.GetThongKeHocSinhTheoXepLoai(namHocFilter, lopFilter, hocKyFilter, monHocFilter);
                
                var chartSeries = new SeriesCollection();
                string chartTitle = $"Thống kê {monHocFilter} - Lớp {lopFilter} - {hocKy}";
                
                
                bool hasData = data.Values.Sum() > 0;
                
                if (hasData)
                {

                    var gioiColor = new System.Windows.Media.LinearGradientBrush(
                        System.Windows.Media.Colors.Gold, System.Windows.Media.Colors.Goldenrod, 90);
                    var khaColor = new System.Windows.Media.LinearGradientBrush(
                        System.Windows.Media.Colors.LimeGreen, System.Windows.Media.Colors.ForestGreen, 90);
                    var trungBinhColor = new System.Windows.Media.LinearGradientBrush(
                        System.Windows.Media.Colors.DeepSkyBlue, System.Windows.Media.Colors.RoyalBlue, 90);
                    var yeuColor = new System.Windows.Media.LinearGradientBrush(
                        System.Windows.Media.Colors.Orange, System.Windows.Media.Colors.DarkOrange, 90);
                    
                    chartSeries.Add(new ColumnSeries
                    {
                        Title = "Giỏi",
                        Values = new ChartValues<int> { data.ContainsKey("Giỏi") ? Math.Max(0, data["Giỏi"]) : 0 },
                        DataLabels = true,
                        MaxColumnWidth = 120, 
                        Fill = gioiColor,
                        Stroke = System.Windows.Media.Brushes.DarkGoldenrod,
                        StrokeThickness = 2
                    });
                    
                    chartSeries.Add(new ColumnSeries
                    {
                        Title = "Khá",
                        Values = new ChartValues<int> { data.ContainsKey("Khá") ? Math.Max(0, data["Khá"]) : 0 },
                        DataLabels = true,
                        MaxColumnWidth = 120,
                        Fill = khaColor,
                        Stroke = System.Windows.Media.Brushes.Green,
                        StrokeThickness = 2
                    });
                    
                    chartSeries.Add(new ColumnSeries
                    {
                        Title = "Trung bình",
                        Values = new ChartValues<int> { data.ContainsKey("Trung bình") ? Math.Max(0, data["Trung bình"]) : 0 },
                        DataLabels = true,
                        MaxColumnWidth = 120,
                        Fill = trungBinhColor,
                        Stroke = System.Windows.Media.Brushes.RoyalBlue,
                        StrokeThickness = 2
                    });
                    
                    chartSeries.Add(new ColumnSeries
                    {
                        Title = "Yếu",
                        Values = new ChartValues<int> { data.ContainsKey("Yếu") ? Math.Max(0, data["Yếu"]) : 0 },
                        DataLabels = true,
                        MaxColumnWidth = 120,
                        Fill = yeuColor,
                        Stroke = System.Windows.Media.Brushes.DarkOrange,
                        StrokeThickness = 2
                    });
                    
                    
                    LoadPieChartData(data);
                }
                else
                {
                    chartSeries.Add(new ColumnSeries
                    {
                        Title = "Không có dữ liệu",
                        Values = new ChartValues<int> { 0, 0, 0, 0 },
                        DataLabels = false,
                        MaxColumnWidth = 120,
                        Fill = System.Windows.Media.Brushes.LightGray
                    });
                    chartTitle += " (Chưa có dữ liệu)";
                    
                    
                    if (PieChart != null)
                    {
                        PieChart.Series = new SeriesCollection();
                    }
                }
                
                
                
                if (BieuDoChart != null)
                {
                    BieuDoChart.Series = chartSeries;
                    
                    
                    BieuDoChart.AxisX.Clear();
                    BieuDoChart.AxisY.Clear();
                    
                    BieuDoChart.AxisX.Add(new Axis
                    {
                        Title = "Xếp loại",
                        Labels = new[] { "Giỏi", "Khá", "Trung bình", "Yếu" },
                        FontSize = 14,
                        FontWeight = FontWeights.SemiBold,
                        ShowLabels = false  
                    });
                    
                    BieuDoChart.AxisY.Add(new Axis
                    {
                        Title = "Số lượng học sinh",
                        LabelFormatter = value => Math.Max(0, (int)Math.Round(value)).ToString(),
                        MinValue = 0,
                        FontSize = 14,
                        FontWeight = FontWeights.SemiBold
                    });
                    
                    
                    BieuDoChart.LegendLocation = LegendLocation.Bottom;
                    BieuDoChart.Hoverable = true;
                    BieuDoChart.DisableAnimations = false;
                }
                
                
                if (_viewModel != null)
                {
                    _viewModel.ChartTitle = chartTitle;
                }
                
            }
            catch (Exception ex)
            {
                LoadEmptyChart();
            }
        }
        
        private void LoadPieChartData(Dictionary<string, int> data)
        {
            try
            {
                if (PieChart == null) return;
                
                
                var pieSeries = new SeriesCollection();
                
                if (data.Values.Sum() > 0)
                {
                    int totalStudents = data.Values.Sum();
                    

                    if (data.ContainsKey("Giỏi") && data["Giỏi"] > 0)
                    {
                        pieSeries.Add(new PieSeries
                        {
                            Title = $"Giỏi ({data["Giỏi"]} HS)",
                            Values = new ChartValues<int> { data["Giỏi"] },
                            DataLabels = true,
                            LabelPoint = chartPoint => $"{((double)chartPoint.Y / totalStudents * 100):F1}%",
                            Fill = System.Windows.Media.Brushes.Gold
                        });
                    }
                    
                    if (data.ContainsKey("Khá") && data["Khá"] > 0)
                    {
                        pieSeries.Add(new PieSeries
                        {
                            Title = $"Khá ({data["Khá"]} HS)",
                            Values = new ChartValues<int> { data["Khá"] },
                            DataLabels = true,
                            LabelPoint = chartPoint => $"{((double)chartPoint.Y / totalStudents * 100):F1}%",
                            Fill = System.Windows.Media.Brushes.LimeGreen
                        });
                    }
                    
                    if (data.ContainsKey("Trung bình") && data["Trung bình"] > 0)
                    {
                        pieSeries.Add(new PieSeries
                        {
                            Title = $"Trung bình ({data["Trung bình"]} HS)",
                            Values = new ChartValues<int> { data["Trung bình"] },
                            DataLabels = true,
                            LabelPoint = chartPoint => $"{((double)chartPoint.Y / totalStudents * 100):F1}%",
                            Fill = System.Windows.Media.Brushes.DeepSkyBlue
                        });
                    }
                    
                    if (data.ContainsKey("Yếu") && data["Yếu"] > 0)
                    {
                        pieSeries.Add(new PieSeries
                        {
                            Title = $"Yếu ({data["Yếu"]} HS)",
                            Values = new ChartValues<int> { data["Yếu"] },
                            DataLabels = true,
                            LabelPoint = chartPoint => $"{((double)chartPoint.Y / totalStudents * 100):F1}%",
                            Fill = System.Windows.Media.Brushes.Orange
                        });
                    }
                }
                
                PieChart.Series = pieSeries;
                PieChart.LegendLocation = LegendLocation.Bottom;
                PieChart.Hoverable = true;
                PieChart.DisableAnimations = false;
                
            }
            catch (Exception ex)
            {
            }
        }
        
        private void LoadEmptyChart()
        {
            try
            {
                
                
                if (BieuDoChart != null)
                {
                    var emptySeries = new SeriesCollection
                {
                    new ColumnSeries
                    {
                        Title = "Không có dữ liệu",
                        Values = new ChartValues<int> { 0, 0, 0, 0 },
                            DataLabels = false,
                            MaxColumnWidth = 120,
                            Fill = System.Windows.Media.Brushes.LightGray,
                            Stroke = System.Windows.Media.Brushes.Gray,
                            StrokeThickness = 1
                        }
                    };
                    
                    BieuDoChart.Series = emptySeries;
                    
                    
                BieuDoChart.AxisX.Clear();
                BieuDoChart.AxisY.Clear();
                
                BieuDoChart.AxisX.Add(new Axis
                {
                    Title = "Xếp loại",
                    Labels = new[] { "Giỏi", "Khá", "Trung bình", "Yếu" },
                        FontSize = 14,
                        FontWeight = FontWeights.SemiBold,
                        ShowLabels = false
                });
                
                BieuDoChart.AxisY.Add(new Axis
                {
                    Title = "Số lượng học sinh",
                        LabelFormatter = value => "0",
                    MinValue = 0,
                        FontSize = 14,
                        FontWeight = FontWeights.SemiBold
                    });
                    
                    
                    BieuDoChart.LegendLocation = LegendLocation.Bottom;
                    BieuDoChart.Hoverable = false;
                    BieuDoChart.DisableAnimations = true;
                }
                
                
                if (PieChart != null)
                {
                    PieChart.Series = new SeriesCollection();
                    PieChart.LegendLocation = LegendLocation.Bottom;
                    PieChart.Hoverable = false;
                    PieChart.DisableAnimations = true;
                }
                
                
                if (_viewModel != null)
                {
                    _viewModel.ChartTitle = "Lỗi khi tải biểu đồ";
                }
                
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR LoadEmptyChart: {ex.Message}");
            }
        }
        
        #endregion
    }
}
