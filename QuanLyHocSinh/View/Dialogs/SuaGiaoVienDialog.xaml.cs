using System.Windows;
using QuanLyHocSinh.Model.Entities;
using QuanLyHocSinh.ViewModel.TraCuu;

namespace QuanLyHocSinh.View.Dialogs
{

    public partial class SuaGiaoVienDialog : Window
    {
        //Khởi tạo giáo viên
        public SuaGiaoVienDialog(GiaoVien giaoVien)
        {
            InitializeComponent();
            var vm = new SuaGiaoVienViewModel(giaoVien);
            vm.CloseDialog += (result) =>
            {
                this.DialogResult = result;
                this.Close();
            };
            DataContext = vm;
        }
    }
}