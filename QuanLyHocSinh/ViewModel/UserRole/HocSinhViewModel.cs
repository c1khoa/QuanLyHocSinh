using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyHocSinh.ViewModel.UserRole
{
    public class HocSinhViewModel : BaseViewModel
    {
        private MainViewModel _mainVM;
        public HocSinhViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM;
        }
    }
}
