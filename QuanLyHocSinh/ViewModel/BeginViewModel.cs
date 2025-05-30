using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyHocSinh.ViewModel
{
    public class BeginViewModel : BaseViewModel
    {
        private string _logoPath;
        private string _roleName;

        public string LogoPath
        {
            get => _logoPath;
            set { _logoPath = value; OnPropertyChanged(); }
        }

        public string RoleName
        {
            get => _roleName;
            set { _roleName = value; OnPropertyChanged(); }
        }

        public BeginViewModel(string logoPath, string roleName)
        {
            LogoPath = logoPath;
            RoleName = roleName;
        }
    }
}
