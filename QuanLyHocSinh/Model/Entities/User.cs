using System;
using System.ComponentModel;
using System.Data.Entity;
using System.Runtime.CompilerServices;

namespace QuanLyHocSinh.Model.Entities
{
    public class User : INotifyPropertyChanged, ICloneable
    {
        private string _userID;
        private string _tenDangNhap;
        private string _matKhau;
        private string _vaiTroID;
        private string _hoTen;
        private VaiTro _vaiTro; // Private field cho navigation property

        public string MaskedPassword
        {
            get
            {
                if (string.IsNullOrEmpty(MatKhau))
                    return "";
                return new string('*', 8); // ho?c return "********"
            }
        }
        public string UserID
        {
            get => _userID;
            set
            {
                if (_userID != value)
                {
                    _userID = value;
                    OnPropertyChanged();
                }
            }
        }

        public string TenDangNhap
        {
            get => _tenDangNhap;
            set
            {
                if (_tenDangNhap != value)
                {
                    _tenDangNhap = value;
                    OnPropertyChanged();
                }
            }
        }

        public string MatKhau
        {
            get => _matKhau;
            set
            {
                if (_matKhau != value)
                {
                    _matKhau = value;
                    OnPropertyChanged();
                }
            }
        }

        public string VaiTroID
        {
            get => _vaiTroID;
            set
            {
                if (_vaiTroID != value)
                {
                    _vaiTroID = value;
                    OnPropertyChanged();
                }
            }
        }

        public string HoTen
        {
            get => _hoTen;
            set
            {
                if (_hoTen != value)
                {
                    _hoTen = value;
                    OnPropertyChanged();
                }
            }
        }

        public VaiTro VaiTro // Navigation property
        {
            get => _vaiTro;
            set
            {
                if (_vaiTro != value)
                {
                    _vaiTro = value;
                    OnPropertyChanged();
                }
            }
        }

        // Constructor m?c ??nh
        public User() { }

        // ?ây là ph??ng th?c Clone() ?úng và duy nh?t cho l?p User
        public object Clone()
        {
            // Quan tr?ng: T?o m?t b?n sao m?i, không ph?i tham chi?u
            return new User
            {
                UserID = this.UserID,
                TenDangNhap = this.TenDangNhap,
                MatKhau = this.MatKhau,
                VaiTroID = this.VaiTroID,
                HoTen = this.HoTen,
                // N?u VaiTro c?ng c?n là m?t b?n sao ??c l?p, và nó implement ICloneable,
                // thì dùng VaiTro = this.VaiTro?.Clone() as VaiTro
                // N?u không, ch? c?n gán VaiTro = this.VaiTro (n?u b?n ch?p nh?n chia s? tham chi?u ho?c VaiTro là m?t enum/struct)
                VaiTro = this.VaiTro?.Clone() as VaiTro
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public class QLHocSinhEntities : DbContext
    {
        public DbSet<User> Users { get; set; }
        // B?n thêm các DbSet khác n?u có
    }

}