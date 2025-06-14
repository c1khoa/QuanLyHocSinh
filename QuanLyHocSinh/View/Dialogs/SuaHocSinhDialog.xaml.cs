using System.Windows;
using QuanLyHocSinh.Model.Entities;
using QuanLyHocSinh.ViewModel.TraCuu;

namespace QuanLyHocSinh.View.Dialogs
{
    public partial class SuaHocSinhDialog : Window
    {
        //Khởi tạo học sinh
        public SuaHocSinhDialog(HocSinh hocSinh)
        {
            InitializeComponent();
            var vm = new SuaHocSinhViewModel(hocSinh);
            vm.CloseDialog += (result) =>
            {
                this.DialogResult = result;
                this.Close();
            };
            DataContext = vm;
        }
    }
}
