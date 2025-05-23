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
using System.Windows.Controls;
using QuanLyHocSinh.View.Windows;
using QuanLyHocSinh.View.Controls;
using QuanLyHocSinh.View.Controls.TraCuu;

namespace QuanLyHocSinh.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        public bool Isloaded { get; set; } = false;
        public ICommand LoadedWindowCommand { get; set; }

        private UserControl _currentControl;
        public UserControl CurrentControl
        {
            get => _currentControl;
            set
            {
                _currentControl = value;
                OnPropertyChanged();
            }
        }

        public ICommand ShowTraCuuHSCommand { get; set; }
        public ICommand ShowTraCuuGVCommand { get; set; }
        public ICommand ShowTraCuuDiemCommand { get; set; }

        // Mọi thứ xử lý nằm trong này
        public MainViewModel()
        {
            LoadedWindowCommand = new RelayCommand<object>(
                (p) => { return true; },
                (p) =>
                {
                    if (Isloaded) return;
                    Isloaded = true;
                    var login = new LoginWindow();
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

            //Xử lý chuyển trang tra cứu
            CurrentControl = new TrangChuUC();
            ShowTraCuuHSCommand = new RelayCommand<object>(
                (p) => true,
                (p) => { CurrentControl = new TraCuuHocSinhUC(); }
            );
            ShowTraCuuGVCommand = new RelayCommand<object>(
                (p) => true,
                (p) => { CurrentControl = new TraCuuGiaoVienUC(); }
            );
            ShowTraCuuDiemCommand = new RelayCommand<object>(
                (p) => true,
                (p) => { CurrentControl = new TraCuuDiemHocSinhUC(); }
            );
        }
    }
}

