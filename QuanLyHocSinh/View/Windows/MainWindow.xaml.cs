using System.Windows;
using QuanLyHocSinh.ViewModel;

namespace QuanLyHocSinh.View.Windows
{
    public partial class MainWindow : Window
    {
        public MainViewModel ViewModel { get; set; }

        public MainWindow(MainViewModel vm)
        {
            InitializeComponent();
            ViewModel = vm;
            this.DataContext = ViewModel; // Gán DataContext nếu chưa
        }

        // Constructor không tham số nếu cần
        public MainWindow() : this(new MainViewModel()) { }
    }
}
