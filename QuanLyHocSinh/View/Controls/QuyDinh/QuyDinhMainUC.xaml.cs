using QuanLyHocSinh.ViewModel.QuyDinh;
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
using QuanLyHocSinh.ViewModel;

namespace QuanLyHocSinh.View.Controls.QuyDinh
{
    /// <summary>
    /// Interaction logic for QuyDinh.xaml
    /// </summary>
    public partial class QuyDinhMainUC : UserControl
    {
        public QuyDinhMainUC()
        {
            InitializeComponent();
            DataContext = new QuyDinhMainViewModel(MainViewModel.Instance);

        }
    }
}
