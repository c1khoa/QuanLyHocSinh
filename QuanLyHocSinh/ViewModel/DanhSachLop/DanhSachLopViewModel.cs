using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyHocSinh.ViewModel.DanhSachLop
{

    public class DanhSachLopViewModel : BaseViewModel
    {
        private readonly MainViewModel _mainVM;
        public DanhSachLopViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM ?? throw new ArgumentNullException(nameof(mainVM));
            // Initialize commands and load data here
        }
    }
}
