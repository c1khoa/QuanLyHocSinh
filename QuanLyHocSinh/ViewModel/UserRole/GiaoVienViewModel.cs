using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyHocSinh.ViewModel.UserRole
{
    public class GiaoVienViewModel : BaseViewModel
    {
        private MainViewModel _mainVM;
        public GiaoVienViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM;
        }
    }
}
