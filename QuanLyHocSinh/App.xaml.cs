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
using MySql.Data.MySqlClient;
using QuanLyHocSinh.Model.Entities;

public class TestConnection
{
    public static void RunTest()
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
                MessageBox.Show("Kết nối MySQL thất bại, vui lòng kiểm tra lại!!");
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

        var loginVM = new LoginViewModel();
        var loginWindow = new LoginWindow { DataContext = loginVM };

        loginVM.LoginSuccess += (s, user) =>
        {
            // Lấy MainVM từ App.xaml
            var mainVM = (MainViewModel)Application.Current.Resources["MainVM"];
            mainVM.CurrentUser = user;
            mainVM.CurrentView = new TrangChuViewModel(mainVM); // Gán trang chủ sau đăng nhập

            // Tạo MainWindow và gán DataContext
            var mainWindow = new MainWindow();

            mainWindow.Show();
            loginWindow.Close();
        };

        loginWindow.Show();
    }


}

