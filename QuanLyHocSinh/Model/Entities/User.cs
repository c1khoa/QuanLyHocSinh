using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Data.Entity;
using QuanLyHocSinh.View.RoleControls;

namespace QuanLyHocSinh.Model.Entities
{
    public class User : INotifyPropertyChanged, ICloneable
    {
        // --- BACKING FIELDS ---
        private string _userID;
        private string _tenDangNhap;
        private string _matKhau;
        private string _hoTen;
        private string _gioiTinh;
        private DateTime? _ngaySinh;
        private string _email;
        private string _diaChi;

        private string _vaiTroID;
        private VaiTro _vaiTro;
        private string _chucVuTen;

        private string _lopHocID;
        private string _lopDayID;

        private string _maHoSoCaNhan;
        private string _maHoSo;

        private string _hocSinhID;
        private string _giaoVienID;
        private string _giaoVuID;
        // --- BACKING FIELDS BỔ SUNG ---
        private string _lopDayID1;
        private string _lopDayID2;
        private string _lopDayID3;
        private string _lopDayIDCN;
        private string _boMon;


        // --- PROPERTIES ---

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

        public string GioiTinh
        {
            get => _gioiTinh;
            set
            {
                if (_gioiTinh != value)
                {
                    _gioiTinh = value;
                    OnPropertyChanged();
                }
            }
        }
        // --- PROPERTIES BỔ SUNG ---
        public string LopDayID1
        {
            get => _lopDayID1;
            set
            {
                if (_lopDayID1 != value)
                {
                    _lopDayID1 = value;
                    OnPropertyChanged();
                }
            }
        }

        public string LopDayID2
        {
            get => _lopDayID2;
            set
            {
                if (_lopDayID2 != value)
                {
                    _lopDayID2 = value;
                    OnPropertyChanged();
                }
            }
        }

        public string LopDayID3
        {
            get => _lopDayID3;
            set
            {
                if (_lopDayID3 != value)
                {
                    _lopDayID3 = value;
                    OnPropertyChanged();
                }
            }
        }

        public string LopDayIDCN
        {
            get => _lopDayIDCN;
            set
            {
                if (_lopDayIDCN != value)
                {
                    _lopDayIDCN = value;
                    OnPropertyChanged();
                }
            }
        }

        public string BoMon
        {
            get => _boMon;
            set
            {
                if (_boMon != value)
                {
                    _boMon = value;
                    OnPropertyChanged();
                }
            }
        }


        public DateTime? NgaySinh
        {
            get => _ngaySinh;
            set
            {
                if (_ngaySinh != value)
                {
                    _ngaySinh = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                if (_email != value)
                {
                    _email = value;
                    OnPropertyChanged();
                }
            }
        }

        public string DiaChi
        {
            get => _diaChi;
            set
            {
                if (_diaChi != value)
                {
                    _diaChi = value;
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

        public VaiTro VaiTro
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

        public string ChucVu
        {
            get => _chucVuTen;
            set
            {
                _chucVuTen = value;
                OnPropertyChanged();
            }
        }

        public string LopHocID
        {
            get => _lopHocID;
            set
            {
                if (_lopHocID != value)
                {
                    _lopHocID = value;
                    OnPropertyChanged();
                }
            }
        }

        public string LopDayID
        {
            get => _lopDayID;
            set
            {
                if (_lopDayID != value)
                {
                    _lopDayID = value;
                    OnPropertyChanged();
                }
            }
        }

        public string MaHoSoCaNhan
        {
            get => _maHoSoCaNhan;
            set
            {
                if (_maHoSoCaNhan != value)
                {
                    _maHoSoCaNhan = value;
                    OnPropertyChanged();
                }
            }
        }

        public string MaHoSo
        {
            get => _maHoSo;
            set
            {
                if (_maHoSo != value)
                {
                    _maHoSo = value;
                    OnPropertyChanged();
                }
            }
        }

        public string HocSinhID
        {
            get => _hocSinhID;
            set
            {
                if (_hocSinhID != value)
                {
                    _hocSinhID = value;
                    OnPropertyChanged();
                }
            }
        }

        public string GiaoVienID
        {
            get => _giaoVienID;
            set
            {
                if (_giaoVienID != value)
                {
                    _giaoVienID = value;
                    OnPropertyChanged();
                }
            }
        }

        public string GiaoVuID
        {
            get => _giaoVuID;
            set
            {
                if (_giaoVuID != value)
                {
                    _giaoVuID = value;
                    OnPropertyChanged();
                }
            }
        }

        // --- MASKED PASSWORD ---
        public string MaskedPassword => string.IsNullOrEmpty(MatKhau) ? "" : new string('*', 8);

        // --- CONSTRUCTOR ---
        public User() { }

        // --- CLONE ---
        public object Clone()
        {
            return new User
            {
                UserID = this.UserID,
                TenDangNhap = this.TenDangNhap,
                MatKhau = this.MatKhau,
                VaiTroID = this.VaiTroID,
                HoTen = this.HoTen,
                VaiTro = this.VaiTro?.Clone() as VaiTro,
                GioiTinh = this.GioiTinh,
                NgaySinh = this.NgaySinh,
                Email = this.Email,
                DiaChi = this.DiaChi,
                LopHocID = this.LopHocID,
                LopDayID = this.LopDayID,
                MaHoSoCaNhan = this.MaHoSoCaNhan,
                MaHoSo = this.MaHoSo,
                ChucVu = this.ChucVu,
                HocSinhID = this.HocSinhID,
                GiaoVienID= this.GiaoVienID,
                GiaoVuID = this.GiaoVuID,
                LopDayID1 = this.LopDayID1,
                LopDayID2 = this.LopDayID2,
                LopDayID3 = this.LopDayID3,
                LopDayIDCN = this.LopDayIDCN,
                BoMon = this.BoMon

            };
        }

        // --- INotifyPropertyChanged ---
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // DbContext
    public class QLHocSinhEntities : DbContext
    {
        public DbSet<User> Users { get; set; }
        // Thêm các DbSet khác nếu có
    }
}
