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
using DocumentFormat.OpenXml.Spreadsheet;
using QuanLyHocSinh.ViewModel;
using QuanLyHocSinh.ViewModel.TraCuu;

namespace QuanLyHocSinh.View.Controls.TraCuu
{
    public partial class TraCuuHocSinhUC : UserControl
    {
        public TraCuuHocSinhUC()
        {
            InitializeComponent();
        }
        private void DiemDataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this); // Lấy cửa sổ chứa UserControl
            if (mainWindow != null)
            {
                var mainVM = mainWindow.DataContext as MainViewModel;
                if (mainVM != null)
                {
                    var chiTietColumn = DiemDataGrid.Columns.FirstOrDefault(c => c.Header?.ToString() == "Chi tiết điểm");
                    if (chiTietColumn != null)
                    {
                        chiTietColumn.Visibility = mainVM.IsNotHocSinhVisible ? Visibility.Visible : Visibility.Collapsed;

                        mainVM.PropertyChanged += (s, args) =>
                        {
                            if (args.PropertyName == nameof(mainVM.IsNotHocSinhVisible))
                            {
                                chiTietColumn.Visibility = mainVM.IsNotHocSinhVisible ? Visibility.Visible : Visibility.Collapsed;
                            }
                        };
                    }
                }
            }
        }










    }
}
