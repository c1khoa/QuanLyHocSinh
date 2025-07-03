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
    }
} 