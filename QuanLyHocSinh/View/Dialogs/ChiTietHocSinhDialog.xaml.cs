using System.Windows;
using QuanLyHocSinh.ViewModel.BaoCao;

namespace QuanLyHocSinh.View.Dialogs
{
    public partial class ChiTietHocSinhDialog : Window
    {
        public ChiTietHocSinhDialog(string hocSinhID, string namHoc, string lop)
        {
            InitializeComponent();
            DataContext = new ChiTietHocSinhViewModel(hocSinhID, namHoc, lop);
        }
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
