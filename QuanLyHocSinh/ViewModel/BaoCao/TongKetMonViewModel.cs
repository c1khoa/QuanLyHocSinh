using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyHocSinh.ViewModel.BaoCao
{
    public class TongKetMonViewModel : BaseViewModel
    {
        private MainViewModel _mainVM;
        public TongKetMonViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM;
        }
    }
}
