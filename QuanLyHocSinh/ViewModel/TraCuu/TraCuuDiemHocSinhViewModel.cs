using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyHocSinh.ViewModel.TraCuu
{
    public class TraCuuDiemHocSinhViewModel : BaseViewModel
    {
        private MainViewModel _mainVM;
        public TraCuuDiemHocSinhViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM;
        }
    }
}
