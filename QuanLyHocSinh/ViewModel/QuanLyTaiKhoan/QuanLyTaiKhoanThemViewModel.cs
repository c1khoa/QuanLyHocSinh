using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace QuanLyHocSinh.ViewModel.QuanLyTaiKhoan
{
    public class QuanLyTaiKhoanThemViewModel : BaseViewModel
    {
        private MainViewModel _mainVM;
        public ICommand HuyThemTaiKhoanCommand { get; set; }

        public QuanLyTaiKhoanThemViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM;
            HuyThemTaiKhoanCommand = new RelayCommand<object>((p) => true, (p) =>
            {
                _mainVM.CurrentView = new QuanLyTaiKhoanMainViewModel(_mainVM);
            });
        }
    }

}
