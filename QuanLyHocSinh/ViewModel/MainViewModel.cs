using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using QuanLyHocSinh.View.Windows;


namespace QuanLyHocSinh.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        public bool Isloaded { get; set; } = false;

        public ICommand LoadedWindowCommand { get; set; }
        // Mọi thứ xử lý nằm trong này
        public MainViewModel()
        {
            LoadedWindowCommand = new RelayCommand<object>(
                (p) => { return true; },
                (p) =>
                {
                    if (Isloaded) return;
                    Isloaded = true;
                    LoginWindow login = new LoginWindow();
                    login.ShowDialog();
                }
            );
            // Đoạn này để xử lý khi nhấn nút thoát
            Application.Current.MainWindow.Closing += (s, e) =>
            {
                if (MessageBox.Show("Bạn có muốn thoát không?", "Thông báo", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                {
                    e.Cancel = true;
                }
            };


        }
    }
}

