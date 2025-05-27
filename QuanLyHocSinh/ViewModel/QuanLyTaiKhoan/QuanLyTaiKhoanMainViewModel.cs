using QuanLyHocSinh.Model.Entities;
using QuanLyHocSinh.View.Controls.QuanLyTaiKhoan;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace QuanLyHocSinh.ViewModel.QuanLyTaiKhoan
{
    public class QuanLyTaiKhoanMainViewModel : BaseViewModel
    {
        private MainViewModel _mainVM;

        public ICommand ShowThemTaiKhoanCommand { get; set; }

        public QuanLyTaiKhoanMainViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM;

            ShowThemTaiKhoanCommand = new RelayCommand<object>((p) => true, (p) =>
            {
                _mainVM.CurrentView = new QuanLyTaiKhoanThemViewModel(_mainVM);
            });
        }
    }
}
