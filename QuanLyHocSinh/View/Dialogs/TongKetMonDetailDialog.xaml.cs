using System.Windows;
using QuanLyHocSinh.Model.Entities;

namespace QuanLyHocSinh.View.Dialogs
{
    public partial class TongKetMonDetailDialog : Window
    {
        public TongKetMonDetailDialog(TongKetMonItem item)
        {
            InitializeComponent();
            DataContext = item;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
