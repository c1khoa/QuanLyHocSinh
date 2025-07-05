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
using QuanLyHocSinh.ViewModel.DanhSachLop;
using QuanLyHocSinh.ViewModel;

namespace QuanLyHocSinh.View.Controls.DanhSachLop
{
    /// <summary>
    /// Interaction logic for DanhSachLopUC.xaml
    /// </summary>
    public partial class DanhSachLopUC : UserControl
    {
        public DanhSachLopUC()
        {
            InitializeComponent();
        }
        public DanhSachLopUC(MainViewModel mainVM)
        {
            InitializeComponent();
            DataContext = new DanhSachLopViewModel(mainVM);
        }
        private void DiemDataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this); // Lấy cửa sổ chứa UserControl
            if (mainWindow != null)
            {
                var mainVM = mainWindow.DataContext as MainViewModel;
                if (mainVM != null)
                {
                    var chiTietColumn = DiemDataGrid.Columns.FirstOrDefault(c => c.Header?.ToString() == "Sửa");
                    if (chiTietColumn != null)
                    {
                        chiTietColumn.Visibility = mainVM.IsGiaoVuVisible ? Visibility.Visible : Visibility.Collapsed;

                        mainVM.PropertyChanged += (s, args) =>
                        {
                            if (args.PropertyName == nameof(mainVM.IsGiaoVuVisible))
                            {
                                chiTietColumn.Visibility = mainVM.IsGiaoVuVisible ? Visibility.Visible : Visibility.Collapsed;
                            }
                        };
                    }
                }
            }
        }
    }
} 