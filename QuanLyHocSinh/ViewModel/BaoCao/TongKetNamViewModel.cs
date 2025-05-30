using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyHocSinh.ViewModel.BaoCao
{
    public class TongKetNamViewModel : BaseViewModel
    {
        private MainViewModel _mainVM;
        public TongKetNamViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM;
        }
    }
}
