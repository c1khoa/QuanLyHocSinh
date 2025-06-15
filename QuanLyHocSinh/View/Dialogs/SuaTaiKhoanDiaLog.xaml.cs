using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using QuanLyHocSinh.ViewModel.QuanLyTaiKhoan;

namespace QuanLyHocSinh.View.Dialogs
{
    /// <summary>
    /// Interaction logic for SuaTaiKhoanDiaLog.xaml
    /// </summary>
    public partial class SuaTaiKhoanDiaLog : Window
    {
        private void pwdNewPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is QuanLyTaiKhoanSuaViewModel vm)
            {
                vm.NewPassword = ((PasswordBox)sender).Password;
            }
        }

        private void pwdConfirmPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is QuanLyTaiKhoanSuaViewModel vm)
            {
                vm.ConfirmPassword = ((PasswordBox)sender).Password;
            }
        }

        public SuaTaiKhoanDiaLog()
        {
            InitializeComponent();
        }
    }
}
