using QuanLyHocSinh.ViewModel;
using QuanLyHocSinh.ViewModel.QuanLyTaiKhoan;
using System;
using System.Windows.Controls;

namespace QuanLyHocSinh.View.Controls.QuanLyTaiKhoan
{
    public partial class QuanLyTaiKhoanMainUC : UserControl
    {
        public QuanLyTaiKhoanMainUC() : this(null) { }

        public QuanLyTaiKhoanMainUC(MainViewModel mainVM)
        {
            InitializeComponent();

            if (mainVM != null)
                DataContext = new QuanLyTaiKhoanMainViewModel(mainVM);
            else
                DataContext = new QuanLyTaiKhoanMainViewModel(new MainViewModel()); // hoặc xử lý null phù hợp
        }
    }

}
//public partial class QuanLyTaiKhoanMainUC : UserControl
//{
//    public QuanLyTaiKhoanMainUC(MainViewModel mainVM)
//    {
//        InitializeComponent();

//        if (mainVM == null)
//            throw new ArgumentNullException(nameof(mainVM));

//        DataContext = new QuanLyTaiKhoanMainViewModel(mainVM);
//    }
//}
