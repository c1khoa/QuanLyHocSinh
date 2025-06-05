using QuanLyHocSinh.ViewModel; // Điều chỉnh namespace nếu cần
using System.Windows.Controls;

namespace QuanLyHocSinh.View.Controls.QuyDinh
{
    public partial class QuyDinhSuaUC : UserControl
    {
        public QuyDinhSuaUC(QuyDinhSuaViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}