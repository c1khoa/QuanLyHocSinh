### 1. **LỖI BINDING**: DataContext sai
```
System.Windows.Data Error: 40 : BindingExpression path error: 'ShowThemTaiKhoanCommand' property not found on 'object' ''MainViewModel' (HashCode=38451933)'. BindingExpression:Path=ShowThemTaiKhoanCommand; DataItem='MainViewModel' (HashCode=38451933); target element is 'Button' (Name=''); target property is 'Command' (type 'ICommand')
```

### 2. **LỖI HASH MẬT KHẨU**: Mật khẩu bị mã hóa không mong muốn
- **Hiện tượng**: Mật khẩu `khanghy1102` thành `+1cqZeVy4TCtUPtRepnyuDoc/jQVFQ2FN27xeaN35cI=`
- **Nguyên nhân**: Áp dụng SHA-256 hashing

### 3. **LỖI DATABASE**: Không tạo record trong bảng tương ứng
- **Hiện tượng**: Thêm tài khoản "Giáo vụ" nhưng bảng `giaovu` trống
- **Nguyên nhân**: Chỉ insert vào bảng `USERS`, bỏ qua các bảng liên quan


### **BƯỚC 1: Sửa DataContext và Binding**
#### 1.1 Sửa QuanLyTaiKhoanMainUC.xaml
```xml
<!-- TRƯỚC -->
DataContext="{StaticResource MainVM}"
d:DataContext="{d:DesignInstance Type=viewmodel:ControlBarViewModel}"

<!-- SAU -->
<!-- Xóa DataContext="{StaticResource MainVM}" -->
d:DataContext="{d:DesignInstance Type=viewmodel:QuanLyTaiKhoanMainViewModel}"
```

#### 1.2 Sửa DataTemplate trong MainResource.xaml
```xml
<!-- THÊM DataContext binding -->
<DataTemplate DataType="{x:Type vmQuanLyTaiKhoan:QuanLyTaiKhoanMainViewModel}">
    <viewqltk:QuanLyTaiKhoanMainUC DataContext="{Binding}" />
</DataTemplate>
```

### **BƯỚC 2: Sửa Command Binding**

#### 2.1 Thêm Command cho Button "Thêm"
```xml
<!-- TRƯỚC -->
<Button Content="Thêm">
</Button>

<!-- SAU -->
<Button Content="Thêm" Command="{Binding AddAccountCommand}">
</Button>
```

#### 2.2 Sửa Command cho Button "Hủy"
```xml
<!-- TRƯỚC -->
<Button Command="{Binding HuyThemTaiKhoanCommand}">

<!-- SAU -->
<Button Command="{Binding CancelCommand}">
```

### **BƯỚC 3: Sửa Property Binding**

#### 3.1 Binding các TextBox
```xml
<!-- UserID -->
<TextBox Text="{Binding UserID, UpdateSourceTrigger=PropertyChanged}"/>

<!-- Tên đăng nhập -->
<TextBox Text="{Binding TenDangNhap, UpdateSourceTrigger=PropertyChanged}"/>

<!-- ComboBox vai trò -->
<ComboBox ItemsSource="{Binding Roles}"
          SelectedValue="{Binding VaiTroID, UpdateSourceTrigger=PropertyChanged}"/>
```

### **BƯỚC 4: Sửa Constructor UserControl**

#### 4 Thêm lại constructor mặc định
```csharp
// TRƯỚC
public QuanLyTaiKhoanMainUC() : this(null) { }

// SAU  
public QuanLyTaiKhoanMainUC()
{
    InitializeComponent();
}
```

### **BƯỚC 5: Bỏ Hash mật khẩu**

#### 5 Sửa AddAccountCommand
```csharp
// TRƯỚC
var hashedPassword = HashPassword(MatKhau);
MatKhau = hashedPassword,

// SAU
MatKhau = MatKhau, // Lưu mật khẩu trực tiếp
```

### **BƯỚC 6: Sửa logic Database - Tạo record trong bảng tương ứng**

#### 6Cập nhật UserService.ThemTaiKhoan()
```csharp
// THÊM logic tạo record theo vai trò
switch (user.VaiTroID.ToUpper())
{
    case "VT03": // Giáo vụ
        roleQuery = "INSERT INTO GIAOVU (GiaoVuID, UserID) VALUES (@RoleID, @UserID)";
        roleID = "GVU" + userID.Substring(1);
        break;
    case "VT02": // Giáo viên 
        roleQuery = "INSERT INTO GIAOVIEN (GiaoVienID, UserID) VALUES (@RoleID, @UserID)";
        roleID = "GV" + userID.Substring(1);
        break;
    case "VT01": // Học sinh
        roleQuery = "INSERT INTO HOCSINH (HocSinhID, UserID) VALUES (@RoleID, @UserID)";
        roleID = "HS" + userID.Substring(1);
        break;
}
```
