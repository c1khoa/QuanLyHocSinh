using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyHocSinh.ViewModel.TraCuu
{
    public class TraCuuHocSinhViewModel : BaseViewModel
    {
        private MainViewModel _mainVM;
        public TraCuuHocSinhViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM;
        }
    }
}
