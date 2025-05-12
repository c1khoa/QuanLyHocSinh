
Cấu trúc thư mục dự án WPF
```
|── App.xaml
├── App.xaml.cs
├── AssemblyInfo.cs
│
├── Model/
│   ├── Entities/
│   │   ├── CapNhat.cs
│   │   ├── CapNhatDiem.cs
│   │   ├── ChucVu.cs
│   │   ├── Diem.cs
│   │   ├── GiaoVien.cs
│   │   ├── GiaoVu.cs
│   │   ├── HocSinh.cs
│   │   ├── HoSo.cs
│   │   ├── LoaiDiem.cs
│   │   ├── Lop.cs
│   │   ├── MonHoc.cs
│   │   ├── NamHoc.cs
│   │   ├── Quyen.cs
│   │   ├── User.cs
│   │   └── VaiTro.cs
│   │
│   └── Relationships/
│       ├── ChiTietDiem.cs
│       ├── ChiTietMonHoc.cs
│       ├── ChiTietQuyen.cs
│       ├── HoSoGiaoVien.cs
│       ├── HoSoHocSinh.cs
│       └── PhanQuyen.cs
│
├── ResourceXaml/
│   └── MainResource.xaml
│
├── View/
│   ├── Controls/
│   │   ├── ControlBarUC.xaml
│   │   ├── ControlBarUC.xaml.cs
│   │   ├── TrangChuUC.xaml
│   │   ├── TrangChuUC.xaml.cs
│   │   │
│   │   ├── BaoCao/
│   │   │   ├── TongKetMonUC.xaml
│   │   │   ├── TongKetMonUC.xaml.cs
│   │   │   ├── TongKetNamHocUC.xaml
│   │   │   └── TongKetNamHocUC.xaml.cs
│   │   │
│   │   ├── QuanLyTaiKhoan/
│   │   │   ├── QuanLyTaiKhoanMainUC.xaml
│   │   │   ├── QuanLyTaiKhoanMainUC.xaml.cs
│   │   │   ├── QuanLyTaiKhoanSuaUC.xaml
│   │   │   ├── QuanLyTaiKhoanSuaUC.xaml.cs
│   │   │   ├── QuanLyTaiKhoanThemUC.xaml
│   │   │   └── QuanLyTaiKhoanThemUC.xaml.cs
│   │   │
│   │   ├── QuyDinh/
│   │   │   ├── QuyDinhMainUC.xaml
│   │   │   ├── QuyDinhMainUC.xaml.cs
│   │   │   ├── QuyDinhSuaUC.xaml
│   │   │   └── QuyDinhSuaUC.xaml.cs
│   │   │
│   │   └── TraCuu/
│   │       ├── TraCuuDiemHocSinhUC.xaml
│   │       ├── TraCuuDiemHocSinhUC.xaml.cs
│   │       ├── TraCuuGiaoVienUC.xaml
│   │       ├── TraCuuGiaoVienUC.xaml.cs
│   │       ├── TraCuuHocSinhUC.xaml
│   │       └── TraCuuHocSinhUC.xaml.cs
│   │
│   ├── Converters/
│   │   └── RoleToVisibilityConverter.cs
│   │
│   ├── RoleControls/
│   │   ├── GiaoVienUC.xaml
│   │   ├── GiaoVienUC.xaml.cs
│   │   ├── GiaoVuUC.xaml
│   │   ├── GiaoVuUC.xaml.cs
│   │   ├── HocSinhUC.xaml
│   │   └── HocSinhUC.xaml.cs
│   │
│   └── Windows/
│       ├── LoginWindow.xaml
│       ├── LoginWindow.xaml.cs
│       ├── MainWindow.xaml
│       └── MainWindow.xaml.cs
│
└── ViewModel/
    ├── BaseViewModel.cs
    ├── ControlBarViewModel.cs
    ├── MainViewModel.cs
    ├── NotEmptyValidationRule.cs.cs
    │
    ├── BaoCao/
    │   ├── TongKetMonViewModel.cs
    │   └── TongKetNamHocViewModel.cs
    │
    ├── QuanLyTaiKhoan/
    │   ├── QuanLyTaiKhoanMainViewModel.cs
    │   ├── QuanLyTaiKhoanSuaViewModel.cs
    │   └── QuanLyTaiKhoanThemViewModel.cs
    │
    ├── QuyDinh/
    │   ├── QuyDinhMainViewModel.cs
    │   └── QuyDinhSuaViewModel.cs
    │
    ├── TraCuu/
    │   ├── TraCuuDiemHocSinhViewModel.cs
    │   ├── TraCuuGiaoVienViewModel.cs
    │   └── TraCuuHocSinhViewModel.cs
    │
    └── UserRole/
        ├── GiaoVienViewModel.cs
        ├── GiaoVuViewModel.cs
        └── HocSinhViewModel.cs
```
### Giải thích cấu trúc và chức năng từng file/folder trong dự án WPF

#### Root Files
- **App.xaml**: Khai báo resource chung, style, theme và entry point của ứng dụng.
- **App.xaml.cs**: Code-behind chứa sự kiện khởi tạo hoặc xử lý khởi động app.
- **AssemblyInfo.cs**: Chứa metadata của assembly như version, title, author...

---

#### Model/
##### Entities/
Chứa các class đại diện cho các bảng dữ liệu nhóm trưởng đã thiết kế trong đồ án.
- **CapNhat.cs**: Lịch sử cập nhật hệ thống.
- **CapNhatDiem.cs**: Ghi lại thay đổi điểm của học sinh.
- **ChucVu.cs**: Chức danh trong hệ thống.
- **Diem.cs**: Thông tin điểm số học sinh.
- **GiaoVien.cs / GiaoVu.cs / HocSinh.cs**: Thông tin người dùng theo vai trò.
- **HoSo.cs**: Hồ sơ chung.
- **LoaiDiem.cs**: Loại điểm (miệng, 15p, học kỳ,...).
- **Lop.cs**: Lớp học.
- **MonHoc.cs**: Môn học.
- **NamHoc.cs**: Năm học.
- **Quyen.cs / VaiTro.cs / User.cs**: Hệ thống phân quyền và người dùng.

##### Relationships/
Chứa các mối quan hệ liên kết nhiều-nhiều giữa các entity.
- **ChiTietDiem.cs**: Mối liên hệ giữa điểm và học sinh/môn học.
- **ChiTietMonHoc.cs**: Phân công môn học theo lớp và giáo viên.
- **ChiTietQuyen.cs / PhanQuyen.cs / QuyenThuocVaiTro.cs**: Hệ thống phân quyền.
- **HoSoHocSinh.cs / HoSoGiaoVien.cs**: Liên kết hồ sơ với người dùng.

---

#### ResourceXaml/
- **MainResource.xaml**: Tài nguyên dùng chung (styles, brushes, templates).

---

#### View/
##### Controls/
UserControls giao diện chia nhỏ theo chức năng:
- **ControlBarUC**: Thanh điều khiển của cửa sổ.
- **TrangChuUC**: Trang chính sau khi đăng nhập.

###### BaoCao/
- **TongKetMonUC**: UI báo cáo theo môn.
- **TongKetNamHocUC**: UI báo cáo tổng kết năm.

###### QuanLyTaiKhoan/
Giao diện quản lý tài khoản:
- **MainUC**: Màn chính.
- **SuaUC / ThemUC**: Chỉnh sửa và thêm mới tài khoản.

###### QuyDinh/
- **MainUC / SuaUC**: UI quản lý quy định điểm, học lực, lên lớp...

###### TraCuu/
- **DiemHocSinhUC / GiaoVienUC / HocSinhUC**: UI tra cứu thông tin tương ứng.

##### Converters/
- **RoleToVisibilityConverter.cs**: Chuyển vai trò người dùng sang thuộc tính hiển thị trong UI.

##### RoleControls/
Các giao diện chính theo vai trò:
- **GiaoVienUC / GiaoVuUC / HocSinhUC**: Màn hình chính từng vai trò.

##### Windows/
- **LoginWindow**: Cửa sổ đăng nhập.
- **MainWindow**: Cửa sổ chính khi vào hệ thống.

---

#### ViewModel/
Chứa logic xử lý dữ liệu, liên kết với View (MVVM pattern).
- **BaseViewModel.cs**: Lớp nền cho binding và NotifyPropertyChanged.
- **ControlBarViewModel.cs**: Xử lý logic cho thanh điều khiển cửa sổ.
- **MainViewModel.cs**: ViewModel chính của MainWindow.
- **NotEmptyValidationRule.cs.cs**: Kiểm tra ô nhập không được để trống.

##### BaoCao/
- **TongKetMonViewModel / TongKetNamHocViewModel**: Xử lý logic báo cáo.

##### QuanLyTaiKhoan/
- **Main / Sua / Them ViewModel**: Xử lý tương tác CRUD tài khoản.

##### QuyDinh/
- **Main / Sua ViewModel**: Xử lý logic cho giao diện quy định.

##### TraCuu/
- **DiemHocSinh / GiaoVien / HocSinh ViewModel**: Xử lý logic cho tra cứu.

##### UserRole/
- **GiaoVien / GiaoVu / HocSinh ViewModel**: ViewModel tương ứng với từng vai trò đăng nhập.

---


### 📋 Phân công chi tiết công việc cho 4 thành viên dự án WPF

---

### 📋 Phân công công việc điều chỉnh cho 4 thành viên dự án WPF (cân bằng tải)

---

#### 🧑‍💻 **Thành viên 1: Giao diện chính + Trang chủ + Điều hướng + Đăng nhập**
##### Mục tiêu:
- Xây dựng giao diện chính của ứng dụng, trang chủ sau đăng nhập, và xử lý đăng nhập.

##### File phụ trách:
- `MainWindow.xaml` & `MainWindow.xaml.cs`: Cửa sổ chính của app.
- `ControlBarUC.xaml` & `ControlBarUC.xaml.cs`: Thanh điều khiển cửa sổ.
- `ControlBarViewModel.cs`: ViewModel cho thanh điều khiển.
- `TrangChuUC.xaml` & `TrangChuUC.xaml.cs`: Trang chủ chính.
- `MainViewModel.cs`: ViewModel tổng thể.
- `LoginWindow.xaml` & `LoginWindow.xaml.cs`: Giao diện đăng nhập.
- `BaseViewModel.cs`, `NotEmptyValidationRule.cs.cs`: Cấu trúc MVVM + validation.

##### Ghi chú:
- Tạo trải nghiệm người dùng liền mạch từ đăng nhập tới dashboard.

---

#### 👩‍💻 **Thành viên 2: Quản lý tài khoản + Quy định + Phân quyền**
##### Mục tiêu:
- Xây dựng UI và logic cho chức năng quản trị tài khoản, quy định hệ thống và xử lý phân quyền người dùng.

##### File phụ trách:
- `View/Controls/QuanLyTaiKhoan/*` & `ViewModel/QuanLyTaiKhoan/*`
- `View/Controls/QuyDinh/*` & `ViewModel/QuyDinh/*`
- `RoleToVisibilityConverter.cs`: Quyết định hiển thị control.
- Các file phân quyền:
  - `User.cs`, `Quyen.cs`, `VaiTro.cs`
  - `PhanQuyen.cs`, `ChiTietQuyen.cs`

##### Ghi chú:
- Kết hợp tốt với thành viên 1 để xử lý đăng nhập và hiển thị theo vai trò.

---

#### 👨‍💻 **Thành viên 3: Báo cáo + Tra cứu**
##### Mục tiêu:
- Xây dựng chức năng báo cáo thống kê và các chức năng tra cứu thông tin.

##### File phụ trách:
- `View/Controls/BaoCao/*` & `ViewModel/BaoCao/*`
- `View/Controls/TraCuu/*` & `ViewModel/TraCuu/*`

##### Ghi chú:
- Đảm bảo kết nối dữ liệu đúng với Model do thành viên 4 xây dựng.

---

#### 👩‍💻 **Thành viên 4: Xử lý dữ liệu (Model)**
##### Mục tiêu:
- Xây dựng toàn bộ tầng dữ liệu (Entities & Relationships) phục vụ toàn hệ thống.

##### File phụ trách:
- `Model/Entities/*`: Các bảng chính.
- `Model/Relationships/*`: Quan hệ nhiều-nhiều.

##### Ghi chú:
- Thiết kế chuẩn hóa, tránh trùng lặp.
- Làm việc song song với các thành viên còn lại để đảm bảo logic dữ liệu khớp với UI.

---

### ✅ Lưu ý chung:
- Sử dụng MVVM chuẩn.
- Commit đúng chức năng, rõ message.

