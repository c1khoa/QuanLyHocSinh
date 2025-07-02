using QuanLyHocSinh.View.Windows;
using QuanLyHocSinh.ViewModel;
using System.Configuration;
using System.Data;
using System.Windows;

namespace QuanLyHocSinh;

/// <summary>  
/// Interaction logic for App.xaml  
/// </summary>  
/// using System;
using System.Configuration;
using MaterialDesignThemes.Wpf;
using MySql.Data.MySqlClient;
using QuanLyHocSinh.Model.Entities;
using QuanLyHocSinh.View.Dialogs.MessageBox;

public class TestConnection
{
    public async static void RunTest()
    {
        string connStr = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
        using (MySqlConnection conn = new MySqlConnection(connStr))
        {
            try
            {
                conn.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi kết nối MySQL: {ex.Message}");
            }
        }
    }
}

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        TestConnection.RunTest();
        base.OnStartup(e);
        var window = new BeginWindow();
        window.DataContext = Resources["MainVM"]; // Gán DataContext MVVM
        window.Show();
    }


}
