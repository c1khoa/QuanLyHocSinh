using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyHocSinh.ViewModel
{
    public class TrangChuViewModel : BaseViewModel
    {
        private MainViewModel mainViewModel;

        public TrangChuViewModel(MainViewModel mainViewModel)
        {
            this.mainViewModel = mainViewModel;
        }
    }
}
