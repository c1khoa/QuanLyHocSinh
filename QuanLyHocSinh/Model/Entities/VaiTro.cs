using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace QuanLyHocSinh.Model.Entities
{
    public class VaiTro : INotifyPropertyChanged, ICloneable
    {
        private string _vaiTroID;
        private string _tenVaiTro;

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

        public string TenVaiTro
        {
            get => _tenVaiTro;
            set
            {
                if (_tenVaiTro != value)
                {
                    _tenVaiTro = value;
                    OnPropertyChanged();
                }
            }
        }

        public object Clone()
        {
            return new VaiTro
            {
                VaiTroID = this.VaiTroID,
                TenVaiTro = this.TenVaiTro
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // --- BỔ SUNG HAI PHƯƠNG THỨC NÀY ---
        public override bool Equals(object obj)
        {
            return obj is VaiTro vaiTro &&
                   VaiTroID == vaiTro.VaiTroID; // So sánh dựa trên VaiTroID
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(VaiTroID); // Hash dựa trên VaiTroID
        }
        // --- KẾT THÚC BỔ SUNG ---
    }
}