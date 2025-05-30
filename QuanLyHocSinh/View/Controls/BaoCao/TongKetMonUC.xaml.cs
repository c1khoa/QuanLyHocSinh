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
using QuanLyHocSinh.Model.Entities;

namespace QuanLyHocSinh.View.Controls.BaoCao
{
    public partial class TongKetMonUC : UserControl
    {
        public TongKetMonUC()
        {
            InitializeComponent();
        }

        private void XemChiTiet_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is TongKetMonItem item)
            {
                var dialog = new QuanLyHocSinh.View.Dialogs.TongKetMonDetailDialog(item);
                dialog.ShowDialog();
            }
        }
    }
}
