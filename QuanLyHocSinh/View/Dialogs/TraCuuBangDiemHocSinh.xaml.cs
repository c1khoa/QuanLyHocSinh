using System.Windows;

namespace QuanLyHocSinh.View.Dialogs
{
    public partial class BangDiemHocSinh : Window
    {
        public BangDiemHocSinh()
        {
            InitializeComponent();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
