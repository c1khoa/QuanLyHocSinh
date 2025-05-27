using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using QuanLyHocSinh.ViewModel;

namespace QuanLyHocSinh.View.Windows
{
    public MainViewModel ViewModel { get; set; }
    public MainWindow(MainViewModel vm) 
    {
        InitializeComponent();
        ViewModel = new MainViewModel();
    }

    // Nếu cần, thêm constructor không tham số:
    public MainWindow() : this(new MainViewModel())
    {
    }
}
