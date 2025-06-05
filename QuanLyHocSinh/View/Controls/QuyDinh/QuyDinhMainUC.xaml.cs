using QuanLyHocSinh.ViewModel.QuyDinh;
using System.Windows.Controls;

namespace QuanLyHocSinh.View.Controls.QuyDinh
{
    public partial class QuyDinhMainUC : UserControl
    {
        public QuyDinhMainUC(QuyDinhMainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}