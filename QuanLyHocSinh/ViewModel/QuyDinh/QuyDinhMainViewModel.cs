using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyHocSinh.ViewModel.QuyDinh
{
    public class QuyDinhMainViewModel: BaseViewModel
    {
        private MainViewModel _mainVM;
        public QuyDinhMainViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM;
        }
        // Các thuộc tính và phương thức khác cho ViewModel này
        // Ví dụ: Thông tin quy định, các lệnh để lưu thay đổi, v.v.
    }
}
