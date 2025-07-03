using QuanLyHocSinh.View.Controls.QuanLyTaiKhoan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using QuanLyHocSinh.Model.Entities;
using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Input;
using QuanLyHocSinh.View.Dialogs;

namespace QuanLyHocSinh.ViewModel.QuanLyTaiKhoan
{
    public class QuanLyTaiKhoanCaNhanViewModel : BaseViewModel
    {
        private readonly MainViewModel _mainVM;
        private User _currentUser;
        private string _newPassword;
        private string _confirmPassword;


        public User CurrentUser
        {
            get => _currentUser;
            set { _currentUser = value; OnPropertyChanged(); }
        }

        public string NewPassword
        {
            get => _newPassword;
            set { _newPassword = value; OnPropertyChanged(); }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set { _confirmPassword = value; OnPropertyChanged(); }
        }
        public bool IsGiaoVienVisible => _mainVM?.IsGiaoVienVisible ?? false;
        public bool IsHocSinhVisible => _mainVM?.IsHocSinhVisible ?? false;
        public bool IsGiaoVuVisible => _mainVM?.IsGiaoVuVisible ?? false;
        public bool LaGiaoVienBoMon => CurrentUser.ChucVu == "Giáo viên bộ môn";
        public bool LaGiaoVienChuNhiem => CurrentUser.ChucVu == "Giáo viên chủ nhiệm";



        public ICommand ChangePasswordCommand { get; }
        public ICommand ShowScoreCommand { get; }


        public QuanLyTaiKhoanCaNhanViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM;
            CurrentUser = mainVM.CurrentUser?.Clone() as User;
            ChangePasswordCommand = new RelayCommand(OpenChangePasswordDialog);
            ShowScoreCommand = new RelayCommand(ShowScore);
        }
        private void OpenChangePasswordDialog()
        {
            var changePasswordVM = new DoiMatKhauViewModel(CurrentUser); // truyền đúng người dùng hiện tại

            var dialog = new DoiMatKhauDialog
            {
                DataContext = changePasswordVM,
                Owner = Application.Current.MainWindow
            };

            dialog.ShowDialog();
        }
        private void ShowScore()
        {
            // Mở cửa sổ xem điểm
            var scoreWindow = new DiemDialogViewModel(CurrentUser);
            var dialog = new DiemDialog(CurrentUser); // truyền vào User hiện tại
            dialog.ShowDialog();
        } 


    }
}
