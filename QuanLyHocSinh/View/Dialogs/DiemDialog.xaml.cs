using QuanLyHocSinh.Model.Entities;
using QuanLyHocSinh.ViewModel.QuanLyTaiKhoan;
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

namespace QuanLyHocSinh.View.Dialogs
{
    /// <summary>
    /// Interaction logic for DiemDialog.xaml
    /// </summary>
    public partial class DiemDialog : Window
    {
        public DiemDialog(User currentUser)
        {
            InitializeComponent();
            DataContext = new DiemDialogViewModel(currentUser);
        }
    }
}
