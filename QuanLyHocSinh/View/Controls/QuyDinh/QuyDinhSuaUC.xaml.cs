using QuanLyHocSinh.ViewModel.QuyDinh;
using QuanLyHocSinh.Model.Entities;
using System.Windows;
using System.Windows.Controls;

namespace QuanLyHocSinh.View.Controls.QuyDinh
{
    public partial class QuyDinhSuaUC : UserControl
    {
        public QuyDinhSuaUC(QuanLyHocSinh.Model.Entities.QuyDinh quyDinh)
        {
            InitializeComponent();

            var vm = new QuyDinhSuaViewModel(quyDinh);
            vm.CloseRequested += () =>
            {
                if (Parent is Grid parentGrid)
                {
                    parentGrid.Children.Remove(this);
                }
            };
            DataContext = vm;
        }
    }
}
