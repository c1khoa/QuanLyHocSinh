﻿using QuanLyHocSinh.ViewModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace QuanLyHocSinh.View.Windows;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
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
