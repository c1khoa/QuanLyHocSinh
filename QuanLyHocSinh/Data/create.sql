DROP DATABASE quanlyhocsinh;
-- Tạo database
CREATE DATABASE quanlyhocsinh;
USE quanlyhocsinh;

-- Tạo bảng VAITRO (tạo trước vì các bảng khác phụ thuộc)
CREATE TABLE VAITRO (
    VaiTroID CHAR(4) PRIMARY KEY,
    TenVaiTro VARCHAR(30) NOT NULL
);

-- Tạo bảng USER
CREATE TABLE USERS (
    UserID CHAR(8) PRIMARY KEY,
    TenDangNhap VARCHAR(255) NOT NULL,
    MatKhau VARCHAR(255) NOT NULL,
    VaiTroID CHAR(4) NOT NULL,
    FOREIGN KEY (VaiTroID) REFERENCES VAITRO(VaiTroID)
);

-- Tạo bảng GIAOVIEN
CREATE TABLE GIAOVIEN (
    GiaoVienID CHAR(8) PRIMARY KEY,
    UserID CHAR(8) NOT NULL,
    FOREIGN KEY (UserID) REFERENCES USERS(UserID)
);

-- Tạo bảng GIAOVU
CREATE TABLE GIAOVU (
    GiaoVuID CHAR(8) PRIMARY KEY,
    UserID CHAR(8) NOT NULL,
    FOREIGN KEY (UserID) REFERENCES USERS(UserID)
);

-- Tạo bảng HOCSINH
CREATE TABLE HOCSINH (
    HocSinhID CHAR(8) PRIMARY KEY,
    UserID CHAR(8) NOT NULL,
    FOREIGN KEY (UserID) REFERENCES USERS(UserID)
);

-- Tạo bảng MONHOC
CREATE TABLE MONHOC (
    MonHocID CHAR(4) PRIMARY KEY,
    TenMonHoc VARCHAR(100) NOT NULL
);

-- Tạo bảng NAMHOC
CREATE TABLE NAMHOC (
    NamHocID CHAR(8) PRIMARY KEY,
    MoTa VARCHAR(255) NOT NULL,
    BatDau DATETIME NOT NULL,
    KetThuc DATETIME NOT NULL
);

-- Tạo bảng LOP
CREATE TABLE LOP (
    LopID CHAR(4) PRIMARY KEY,
    TenLop VARCHAR(50) NOT NULL,
    SiSo INT NOT NULL CHECK(SiSo > 0),
    GVCNID CHAR(8),
    FOREIGN KEY (GVCNID) REFERENCES GIAOVIEN(GiaoVienID)
);

-- Tạo bảng DIEM
CREATE TABLE DIEM (
    DiemID CHAR(8) PRIMARY KEY,
    HocSinhID CHAR(8) NOT NULL,
    MonHocID CHAR(4) NOT NULL,
    NamHocID CHAR(8) NOT NULL,
    HocKy INT NOT NULL CHECK (HocKy IN (1, 2)),
    DiemTrungBinh FLOAT CHECK(DiemTrungBinh BETWEEN 0 AND 10),
    XepLoai VARCHAR(10) CHECK (XepLoai IN ('Giỏi', 'Khá', 'Trung bình', 'Yếu')),
    FOREIGN KEY (HocSinhID) REFERENCES HOCSINH(HocSinhID),
    FOREIGN KEY (MonHocID) REFERENCES MONHOC(MonHocID),
    FOREIGN KEY (NamHocID) REFERENCES NAMHOC(NamHocID)
);

-- Tạo bảng LOAIDIEM
CREATE TABLE LOAIDIEM (
    LoaiDiemID CHAR(4) PRIMARY KEY,
    TenLoaiDiem VARCHAR(8) NOT NULL,
    MoTa VARCHAR(255),
    HeSo FLOAT NOT NULL CHECK(HeSo > 0)
);

-- Tạo bảng CHITIETDIEM
CREATE TABLE CHITIETDIEM (
    ChiTietDiemID CHAR(12) PRIMARY KEY,
    DiemID CHAR(8) NOT NULL,
    LoaiDiemID CHAR(8) NOT NULL,
    GiaTri FLOAT CHECK(GiaTri BETWEEN 0 AND 10),
    FOREIGN KEY (DiemID) REFERENCES DIEM(DiemID),
    FOREIGN KEY (LoaiDiemID) REFERENCES LOAIDIEM(LoaiDiemID)
);

-- Tạo bảng CHITIETMONHOC
CREATE TABLE CHITIETMONHOC (
    ChiTietMonHocID CHAR(8) PRIMARY KEY,
    GiaoVienID CHAR(8) NOT NULL,
    MonHocID CHAR(4) NOT NULL,
    LopDayID CHAR(4) NOT NULL,
    NgayDay DATETIME NOT NULL,
    NoiDungDay VARCHAR(255),
    FOREIGN KEY (GiaoVienID) REFERENCES GIAOVIEN(GiaoVienID),
    FOREIGN KEY (MonHocID) REFERENCES MONHOC(MonHocID),
    FOREIGN KEY (LopDayID) REFERENCES LOP(LopID)
);

-- Tạo bảng CHUCVU
CREATE TABLE CHUCVU (
    ChucVuID CHAR(8) PRIMARY KEY,
    TenChucVu VARCHAR(255) NOT NULL CHECK (TenChucVu IN ('Lớp trưởng', 'Thành viên', 'Giáo viên chủ nhiệm', 'Giáo viên bộ môn', 'Giáo vụ')),
    MoTa VARCHAR(255) NOT NULL,
    VaiTroID CHAR(8) NOT NULL,
    FOREIGN KEY (VaiTroID) REFERENCES VAITRO(VaiTroID)
);

-- Tạo bảng HOSO
CREATE TABLE HOSO (
    HoSoID CHAR(12) PRIMARY KEY,
    HoTen VARCHAR(255) NOT NULL,
    GioiTinh CHAR(8) NOT NULL CHECK (GioiTinh IN ('Nam','Nữ')),
    NgaySinh DATETIME NOT NULL,
    Email VARCHAR(255) NOT NULL CHECK (Email LIKE '%@%.com' OR Email LIKE '%@%.vn'),
    DiaChi VARCHAR(255) NOT NULL,
    ChucVuID CHAR(8) NOT NULL,
    TrangThaiHoSo VARCHAR(20) NOT NULL CHECK(TrangThaiHoSo IN ('Đang hoạt động', 'Đã bị hủy')),
    NgayTao DATETIME NOT NULL,
    NgayCapNhatGanNhat DATETIME NOT NULL,
    FOREIGN KEY (ChucVuID) REFERENCES CHUCVU(ChucVuID)
);

-- Tạo bảng HOSOGIAOVIEN
CREATE TABLE HOSOGIAOVIEN (
    HoSoGiaoVienID CHAR(10) PRIMARY KEY,
    GiaoVienID CHAR(8) NOT NULL,
    HoSoID CHAR(12) NOT NULL,
    NgayBatDauLamViec DATETIME NOT NULL,
    FOREIGN KEY (GiaoVienID) REFERENCES GIAOVIEN(GiaoVienID),
    FOREIGN KEY (HoSoID) REFERENCES HOSO(HoSoID)
);

-- Tạo bảng HOSOHOCSINH
CREATE TABLE HOSOHOCSINH (
    HoSoHocSinhID CHAR(10) PRIMARY KEY,
    HocSinhID CHAR(8) NOT NULL,
    HoSoID CHAR(12) NOT NULL,
    LopHocID CHAR(8) NOT NULL,
    NienKhoa INT NOT NULL CHECK (NienKhoa > 0),
    FOREIGN KEY (HocSinhID) REFERENCES HOCSINH(HocSinhID),
    FOREIGN KEY (HoSoID) REFERENCES HOSO(HoSoID),
    FOREIGN KEY (LopHocID) REFERENCES LOP(LopID)
);
CREATE TABLE PHANCONGDAY (
    PhanCongDayID CHAR(12) PRIMARY KEY,
    GiaoVienID CHAR(8) NOT NULL,
    MonHocID CHAR(4) NOT NULL,
    LopID CHAR(5) NOT NULL,
    NamHocID CHAR(8) NOT NULL,
    ChuanDauRa VARCHAR(255),
    FOREIGN KEY (GiaoVienID) REFERENCES GIAOVIEN(GiaoVienID),
    FOREIGN KEY (MonHocID) REFERENCES MONHOC(MonHocID),
    FOREIGN KEY (LopID) REFERENCES LOP(LopID),
    FOREIGN KEY (NamHocID) REFERENCES NAMHOC(NamHocID)
);


-- Tạo bảng HOSOGIAOVU
CREATE TABLE HOSOGIAOVU (
    HoSoGiaoVuID CHAR(10) PRIMARY KEY,
    GiaoVuID CHAR(8) NOT NULL,
    HoSoID CHAR(12) NOT NULL,
    FOREIGN KEY (GiaoVuID) REFERENCES GIAOVU(GiaoVuID),
    FOREIGN KEY (HoSoID) REFERENCES HOSO(HoSoID)
);

-- Tạo bảng QUYDINHTUOI
CREATE TABLE QUYDINHTUOI (
    QuyDinhTuoiID CHAR(8) PRIMARY KEY,
    TuoiToiThieu INT NOT NULL CHECK(TuoiToiThieu > 0),
    TuoiToiDa INT NOT NULL CHECK(TuoiToiDa > 0)
);

-- Tạo bảng QUYDINH
CREATE TABLE QUYDINH (
    QuyDinhID CHAR(8) PRIMARY KEY,
    QuyDinhTuoiID CHAR(8) NOT NULL,
    SiSoLop INT NOT NULL CHECK(SiSoLop > 0),
    SoLuongMonHoc INT NOT NULL CHECK(SoLuongMonHoc > 0),
    DiemDat INT NOT NULL CHECK (DiemDat BETWEEN 0 AND 10),
    QuyDinhKhac FLOAT,
    FOREIGN KEY (QuyDinhTuoiID) REFERENCES QUYDINHTUOI(QuyDinhTuoiID)
);

-- Tạo bảng QUYEN
CREATE TABLE QUYEN (
    QuyenID CHAR(8) PRIMARY KEY,
    TenQuyen CHAR(8) NOT NULL,
    MoTa VARCHAR(30) NOT NULL
);

-- Tạo bảng CHITIETQUYEN
CREATE TABLE CHITIETQUYEN (
    ChiTietQuyenID CHAR(8) PRIMARY KEY,
    QuyenID CHAR(8) NOT NULL,
    VaiTroID VARCHAR(30) NOT NULL,
    TuongTac CHAR(8) NOT NULL,
    FOREIGN KEY (QuyenID) REFERENCES QUYEN(QuyenID),
    FOREIGN KEY (VaiTroID) REFERENCES VAITRO(VaiTroID)
);

-- Tạo bảng PHANQUYEN
CREATE TABLE PHANQUYEN (
    PhanQuyenID CHAR(8) PRIMARY KEY,
    QuyenID CHAR(8) NOT NULL,
    GiaoVuPhanQuyenID CHAR(8) NOT NULL,
    UserDuocPhanQuyenID CHAR(8) NOT NULL,
    NgayPhanQuyen DATETIME NOT NULL,
    FOREIGN KEY (QuyenID) REFERENCES QUYEN(QuyenID),
    FOREIGN KEY (GiaoVuPhanQuyenID) REFERENCES GIAOVU(GiaoVuID),
    FOREIGN KEY (UserDuocPhanQuyenID) REFERENCES USERS(UserID)
);

-- Tạo bảng CAPNHAT
CREATE TABLE CAPNHAT (
    CapNhatID CHAR(8) PRIMARY KEY,
    UserID CHAR(8) NOT NULL,
    HoTenMoi VARCHAR(255) NOT NULL,
    NgaySinhMoi DATETIME NOT NULL,
    LyDoCapNhat VARCHAR(255),
    ThoiGianCapNhat DATETIME NOT NULL,
    TrangThaiDuyet BIT NOT NULL CHECK (TrangThaiDuyet IN (0,1,2)),
    FOREIGN KEY (UserID) REFERENCES USERS(UserID)
);

-- Tạo bảng CAPNHATDIEM
CREATE TABLE CAPNHATDIEM (
    CapNhatDiemID CHAR(15) PRIMARY KEY,
    GiaoVienID CHAR(8) NOT NULL,
    ChiTietDiemID CHAR(12) NOT NULL,
    DiemMoi FLOAT NOT NULL CHECK (DiemMoi BETWEEN 0 AND 10),
    ThoiGianCapNhat DATETIME NOT NULL,
    TrangThaiDuyet BIT NOT NULL CHECK (TrangThaiDuyet IN (0,1)),
    FOREIGN KEY (GiaoVienID) REFERENCES GIAOVIEN(GiaoVienID),
    FOREIGN KEY (ChiTietDiemID) REFERENCES CHITIETDIEM(ChiTietDiemID)
);

-- DELIMITER $$

-- CREATE TRIGGER trg_after_insert_chitietdiem
-- AFTER INSERT ON CHITIETDIEM
-- FOR EACH ROW
-- BEGIN
--     DECLARE tb FLOAT;
--     DECLARE xeploai VARCHAR(20);

--     -- Tính điểm trung bình
--     SELECT 
--         IFNULL(ROUND(SUM(cd.GiaTri * ld.HeSo) / NULLIF(SUM(CASE WHEN cd.GiaTri IS NOT NULL THEN ld.HeSo ELSE 0 END), 0), 2), 0)
--     INTO tb
--     FROM CHITIETDIEM cd
--     JOIN LOAIDIEM ld ON cd.LoaiDiemID = ld.LoaiDiemID
--     WHERE cd.DiemID = NEW.DiemID;

--     -- Gán xếp loại
--     SET xeploai = 
--         CASE 
--             WHEN tb >= 8 THEN 'Giỏi'
--             WHEN tb >= 6.5 THEN 'Khá'
--             WHEN tb >= 5 THEN 'Trung bình'
--             ELSE 'Yếu'
--         END;

--     -- Cập nhật bảng DIEM
--     UPDATE DIEM
--     SET DiemTrungBinh = tb,
--         XepLoai = xeploai
--     WHERE DiemID = NEW.DiemID;
-- END$$

-- DELIMITER ;


-- DELIMITER $$

-- CREATE TRIGGER trg_after_update_chitietdiem
-- AFTER UPDATE ON CHITIETDIEM
-- FOR EACH ROW
-- BEGIN
--     DECLARE tb FLOAT;
--     DECLARE xeploai VARCHAR(20);

--     -- Tính điểm trung bình
--     SELECT 
--         IFNULL(ROUND(SUM(cd.GiaTri * ld.HeSo) / NULLIF(SUM(CASE WHEN cd.GiaTri IS NOT NULL THEN ld.HeSo ELSE 0 END), 0), 2), 0)
--     INTO tb
--     FROM CHITIETDIEM cd
--     JOIN LOAIDIEM ld ON cd.LoaiDiemID = ld.LoaiDiemID
--     WHERE cd.DiemID = NEW.DiemID;

--     -- Gán xếp loại
--     SET xeploai = 
--         CASE 
--             WHEN tb >= 8 THEN 'Giỏi'
--             WHEN tb >= 6.5 THEN 'Khá'
--             WHEN tb >= 5 THEN 'Trung bình'
--             ELSE 'Yếu'
--         END;

--     -- Cập nhật bảng DIEM
--     UPDATE DIEM
--     SET DiemTrungBinh = tb,
--         XepLoai = xeploai
--     WHERE DiemID = NEW.DiemID;
-- END$$

-- DELIMITER ;


