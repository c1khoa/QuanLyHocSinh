using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using QuanLyHocSinh.ViewModel;

namespace QuanLyHocSinh.View.Windows
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        //Lệnh click vào nút tra cứu học sinh
        private void TraCuuHS_Click(object sender, MouseButtonEventArgs e)
        {
            var mainVM = DataContext as MainViewModel;
            if (mainVM != null)
            {
                mainVM.ShowTraCuuHSCommand.Execute(null);
            }
        }

        //Lệnh click vào nút tra cứu giáo viên
        private void TraCuuGV_Click(object sender, MouseButtonEventArgs e)
        {
            var mainVM = DataContext as MainViewModel;
            if (mainVM != null)
            {
                mainVM.ShowTraCuuGVCommand.Execute(null);
            }
        }

        //Lệnh click vào nút tra cứu điểm học sinh
        private void TraCuuDiem_Click(object sender, MouseButtonEventArgs e)
        {
            var mainVM = DataContext as MainViewModel;
            if (mainVM != null)
            {
                mainVM.ShowTraCuuDiemCommand.Execute(null);
            }
        }
    }
}