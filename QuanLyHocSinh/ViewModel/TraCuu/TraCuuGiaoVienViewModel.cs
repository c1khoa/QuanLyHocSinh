using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyHocSinh.ViewModel.TraCuu
{
    public class TraCuuGiaoVienViewModel : BaseViewModel
    {
        private MainViewModel _mainVM;
        public TraCuuGiaoVienViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM;
        }
    }
}
