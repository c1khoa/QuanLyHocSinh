using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyHocSinh.ViewModel.BaoCao
{
    public class BieuDoThongKeViewModel : BaseViewModel
    {
        private MainViewModel _mainVM;
        public BieuDoThongKeViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM; 
        }
    }
}
