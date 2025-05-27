using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyHocSinh.ViewModel.UserRole
{
    public class GiaoVuViewModel : BaseViewModel
    {
        private MainViewModel _mainVM;
        public GiaoVuViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM;
        }
    }
}
