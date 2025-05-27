using QuanLyHocSinh.View.Windows;
using QuanLyHocSinh.ViewModel;
using System.Configuration;
using System.Data;
using System.Windows;

namespace QuanLyHocSinh;

/// <summary>  
/// Interaction logic for App.xaml  
/// </summary>  
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
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

