-- Dữ liệu mẫu cho bảng VAITRO
INSERT INTO VAITRO (VaiTroID, TenVaiTro) VALUES
('VT01', 'Học sinh'),
('VT02', 'Giáo viên'),
('VT03', 'Giáo vụ');

-- Dữ liệu mẫu cho bảng USER
INSERT INTO USERS (UserID, TenDangNhap, MatKhau, VaiTroID) VALUES
('U001', 'hs001', '123456', 'VT01'),
('U002', 'hs002', '123456', 'VT01'),
('U003', 'hs003', '123456', 'VT01'),
('U004', 'hs004', '123456', 'VT01'),
('U005', 'hs005', '123456', 'VT01'),
('U006', 'gv001', '123456', 'VT02'),
('U007', 'gv002', '123456', 'VT02'),
('U008', 'gv003', '123456', 'VT02'),
('U009', 'gv004', '123456', 'VT02'),
('U010', 'gv005', '123456', 'VT02'),
('U011', 'gvu001', '123456', 'VT03'),
('U012', 'gvu002', '123456', 'VT03');

-- Dữ liệu mẫu cho bảng GIAOVIEN
INSERT INTO GIAOVIEN (GiaoVienID, UserID) VALUES
('GV001', 'U006'),
('GV002', 'U007'),
('GV003', 'U008'),
('GV004', 'U009'),
('GV005', 'U010');

-- Dữ liệu mẫu cho bảng GIAOVU
INSERT INTO GIAOVU (GiaoVuID, UserID) VALUES
('GVU001', 'U011'),
('GVU002', 'U012');

-- Dữ liệu mẫu cho bảng HOCSINH
INSERT INTO HOCSINH (HocSinhID, UserID) VALUES
('HS001', 'U001'),
('HS002', 'U002'),
('HS003', 'U003'),
('HS004', 'U004'),
('HS005', 'U005');

-- Dữ liệu mẫu cho bảng MONHOC
INSERT INTO MONHOC (MonHocID, TenMonHoc) VALUES
('MH01', 'Toán học'),
('MH02', 'Vật lý'),
('MH03', 'Hóa học'),
('MH04', 'Sinh học'),
('MH05', 'Ngữ văn'),
('MH06', 'Lịch sử'),
('MH07', 'Địa lý'),
('MH08', 'Tiếng Anh');

-- Dữ liệu mẫu cho bảng NAMHOC
INSERT INTO NAMHOC (NamHocID, MoTa, BatDau, KetThuc) VALUES
('NH2023', 'Năm học 2023-2024', '2023-09-01', '2024-05-31'),
('NH2022', 'Năm học 2022-2023', '2022-09-01', '2023-05-31'),
('NH2021', 'Năm học 2021-2022', '2021-09-01', '2022-05-31');

-- Dữ liệu mẫu cho bảng LOP
INSERT INTO LOP (LopID, TenLop, SiSo, GVCNID) VALUES
('10A1', 'Lớp 10A1', 40, 'GV001'),
('10A2', 'Lớp 10A2', 38, 'GV002'),
('11A1', 'Lớp 11A1', 42, 'GV003'),
('11A2', 'Lớp 11A2', 39, 'GV004'),
('12A1', 'Lớp 12A1', 41, 'GV005');

-- Dữ liệu mẫu cho bảng DIEM
INSERT INTO DIEM (DiemID, HocSinhID, MonHocID, NamHocID, HocKy, DiemTrungBinh, XepLoai) VALUES
('D001', 'HS001', 'MH01', 'NH2023', 1, 8.5, 'Gioi'),
('D002', 'HS001', 'MH02', 'NH2023', 1, 7.5, 'Kha'),
('D003', 'HS002', 'MH01', 'NH2023', 1, 9.0, 'Gioi'),
('D004', 'HS002', 'MH02', 'NH2023', 1, 8.0, 'Gioi'),
('D005', 'HS003', 'MH01', 'NH2023', 1, 7.0, 'Kha');

-- Dữ liệu mẫu cho bảng LOAIDIEM
INSERT INTO LOAIDIEM (LoaiDiemID, TenLoaiDiem, MoTa, HeSo) VALUES
('LD001', 'Miệng', 'Điểm kiểm tra miệng', 1),
('LD002', '15 phút', 'Điểm kiểm tra 15 phút', 1),
('LD003', '1 tiết', 'Điểm kiểm tra 1 tiết', 2),
('LD004', 'Học kỳ', 'Điểm kiểm tra học kỳ', 3);

-- Dữ liệu mẫu cho bảng CHITIETDIEM
INSERT INTO CHITIETDIEM (ChiTietDiemID, DiemID, LoaiDiemID, GiaTri) VALUES
('CTD001', 'D001', 'LD001', 9.0),
('CTD002', 'D001', 'LD002', 8.5),
('CTD003', 'D001', 'LD003', 8.0),
('CTD004', 'D002', 'LD001', 8.0),
('CTD005', 'D002', 'LD002', 7.5);

-- Dữ liệu mẫu cho bảng CHITIETMONHOC
INSERT INTO CHITIETMONHOC (ChiTietMonHocID, GiaoVienID, MonHocID, LopDayID, NgayDay, NoiDungDay) VALUES
('CTMH001', 'GV001', 'MH01', '10A1', '2024-01-15', 'Chương 1: Hàm số'),
('CTMH002', 'GV002', 'MH02', '10A2', '2024-01-15', 'Chương 1: Cơ học'),
('CTMH003', 'GV003', 'MH03', '11A1', '2024-01-15', 'Chương 1: Cấu tạo nguyên tử'),
('CTMH004', 'GV004', 'MH04', '11A2', '2024-01-15', 'Chương 1: Sinh học tế bào'),
('CTMH005', 'GV005', 'MH05', '12A1', '2024-01-15', 'Chương 1: Văn học Việt Nam');

-- Dữ liệu mẫu cho bảng CHUCVU
INSERT INTO CHUCVU (ChucVuID, TenChucVu, MoTa, VaiTroID) VALUES
('CV001', 'Lop truong', 'Quản lý lớp học', 'VT01'),
('CV002', 'To truong', 'Quản lý tổ học tập', 'VT01'),
('CV003', 'Giao vien chu nhiem', 'Quản lý lớp', 'VT02'),
('CV004', 'Giao vien bo mon', 'Giảng dạy bộ môn', 'VT02'),
('CV005', 'Giao vien chu nhiem', 'Quản lý học vụ', 'VT03');

-- Dữ liệu mẫu cho bảng HOSO (Đã sửa lại với 10 hồ sơ)
INSERT INTO HOSO (HoSoID, HoTen, GioiTinh, NgaySinh, Email, DiaChi, ChucVuID, TrangThaiHoSo, NgayTao, NgayCapNhatGanNhat) VALUES
-- Hồ sơ học sinh
('HS001', 'Nguyễn Văn A', 'Nam', '2006-01-15', 'nguyenvana@gmail.com', 'Hà Nội', 'CV001', 'danghoatdong', '2023-09-01', '2024-01-15'),
('HS002', 'Trần Thị B', 'Nu', '2006-03-20', 'tranthib@gmail.com', 'Hà Nội', 'CV002', 'danghoatdong', '2023-09-01', '2024-01-15'),
('HS003', 'Lê Văn C', 'Nam', '2006-05-10', 'levanc@gmail.com', 'Hà Nội', 'CV001', 'danghoatdong', '2023-09-01', '2024-01-15'),
('HS004', 'Phạm Thị D', 'Nu', '2006-07-25', 'phamthid@gmail.com', 'Hà Nội', 'CV002', 'danghoatdong', '2023-09-01', '2024-01-15'),
('HS005', 'Hoàng Văn E', 'Nam', '2006-09-30', 'hoangvane@gmail.com', 'Hà Nội', 'CV001', 'danghoatdong', '2023-09-01', '2024-01-15'),
-- Hồ sơ giáo viên
('HS006', 'Nguyễn Văn Giáo', 'Nam', '1980-05-12', 'nguyenvangv@gmail.com', 'Hà Nội', 'CV003', 'danghoatdong', '2023-08-01', '2024-01-15'),
('HS007', 'Trần Thị Giáo', 'Nu', '1982-09-20', 'tranthigv@gmail.com', 'Hà Nội', 'CV004', 'danghoatdong', '2023-08-01', '2024-01-15'),
('HS008', 'Lê Văn Giáo', 'Nam', '1979-11-30', 'levangv@gmail.com', 'Hà Nội', 'CV003', 'danghoatdong', '2023-08-01', '2024-01-15'),
('HS009', 'Phạm Thị Giáo', 'Nu', '1985-03-25', 'phamthigv@gmail.com', 'Hà Nội', 'CV004', 'danghoatdong', '2023-08-01', '2024-01-15'),
('HS010', 'Hoàng Văn Giáo', 'Nam', '1983-07-18', 'hoangvangv@gmail.com', 'Hà Nội', 'CV003', 'danghoatdong', '2023-08-01', '2024-01-15');

-- Dữ liệu mẫu cho bảng HOSOGIAOVIEN (Đã sửa lại để dùng hồ sơ giáo viên)
INSERT INTO HOSOGIAOVIEN (HoSoGiaoVienID, GiaoVienID, HoSoID, LopDayID, NgayBatDauLamViec) VALUES
('HGV001', 'GV001', 'HS006', '10A1', '2023-09-01'),
('HGV002', 'GV002', 'HS007', '10A2', '2023-09-01'),
('HGV003', 'GV003', 'HS008', '11A1', '2023-09-01'),
('HGV004', 'GV004', 'HS009', '11A2', '2023-09-01'),
('HGV005', 'GV005', 'HS010', '12A1', '2023-09-01');

-- Dữ liệu mẫu cho bảng HOSOHOCSINH
INSERT INTO HOSOHOCSINH (HoSoHocSinhID, HocSinhID, HoSoID, LopHocID, NienKhoa) VALUES
('HHS001', 'HS001', 'HS001', '10A1', 2023),
('HHS002', 'HS002', 'HS002', '10A2', 2023),
('HHS003', 'HS003', 'HS003', '11A1', 2023),
('HHS004', 'HS004', 'HS004', '11A2', 2023),
('HHS005', 'HS005', 'HS005', '12A1', 2023);

-- Dữ liệu mẫu cho bảng QUYDINHTUOI
INSERT INTO QUYDINHTUOI (QuyDinhTuoiID, TuoiToiThieu, TuoiToiDa) VALUES
('QDT001', 15, 20),
('QDT002', 15, 20),
('QDT003', 15, 20);

-- Dữ liệu mẫu cho bảng QUYDINH
INSERT INTO QUYDINH (QuyDinhID, QuyDinhTuoiID, SiSoLop, SoLuongMonHoc, DiemDat, QuyDinhKhac) VALUES
('QD001', 'QDT001', 40, 8, 5, NULL),
('QD002', 'QDT002', 40, 8, 5, NULL),
('QD003', 'QDT003', 40, 8, 5, NULL);

-- Dữ liệu mẫu cho bảng QUYEN
INSERT INTO QUYEN (QuyenID, TenQuyen, MoTa) VALUES
('Q001', 'Xem', 'Quyền xem thông tin'),
('Q002', 'Them', 'Quyền thêm thông tin'),
('Q003', 'Sua', 'Quyền sửa thông tin'),
('Q004', 'Xoa', 'Quyền xóa thông tin'),
('Q005', 'Duyet', 'Quyền duyệt thông tin');

-- Dữ liệu mẫu cho bảng CHITIETQUYEN
INSERT INTO CHITIETQUYEN (ChiTietQuyenID, QuyenID, VaiTroID, TuongTac) VALUES
('CTQ001', 'Q001', 'VT01', 'Xem'),
('CTQ002', 'Q002', 'VT02', 'Them'),
('CTQ003', 'Q003', 'VT02', 'Sua'),
('CTQ004', 'Q004', 'VT03', 'Xoa'),
('CTQ005', 'Q005', 'VT03', 'Duyet');

-- Dữ liệu mẫu cho bảng PHANQUYEN
INSERT INTO PHANQUYEN (PhanQuyenID, QuyenID, GiaoVuPhanQuyenID, UserDuocPhanQuyenID, NgayPhanQuyen) VALUES
('PQ001', 'Q001', 'GVU001', 'U001', '2023-09-01'),
('PQ002', 'Q002', 'GVU001', 'U002', '2023-09-01'),
('PQ003', 'Q003', 'GVU001', 'U003', '2023-09-01'),
('PQ004', 'Q004', 'GVU002', 'U004', '2023-09-01'),
('PQ005', 'Q005', 'GVU002', 'U005', '2023-09-01');

-- Dữ liệu mẫu cho bảng CAPNHAT
INSERT INTO CAPNHAT (CapNhatID, UserID, HoTenMoi, NgaySinhMoi, LyDoCapNhat, ThoiGianCapNhat, TrangThaiDuyet) VALUES
('CN001', 'U001', 'Nguyễn Văn A Mới', '2006-01-15', 'Cập nhật thông tin', '2024-01-15', 1),
('CN002', 'U002', 'Trần Thị B Mới', '2006-03-20', 'Cập nhật thông tin', '2024-01-15', 1),
('CN003', 'U003', 'Lê Văn C Mới', '2006-05-10', 'Cập nhật thông tin', '2024-01-15', 0),
('CN004', 'U004', 'Phạm Thị D Mới', '2006-07-25', 'Cập nhật thông tin', '2024-01-15', 0),
('CN005', 'U005', 'Hoàng Văn E Mới', '2006-09-30', 'Cập nhật thông tin', '2024-01-15', 1);

-- Dữ liệu mẫu cho bảng CAPNHATDIEM
INSERT INTO CAPNHATDIEM (CapNhatDiemID, GiaoVienID, ChiTietDiemID, DiemMoi, ThoiGianCapNhat, TrangThaiDuyet) VALUES
('CND001', 'GV001', 'CTD001', 9.5, '2024-01-15', 1),
('CND002', 'GV002', 'CTD002', 8.0, '2024-01-15', 1),
('CND003', 'GV003', 'CTD003', 8.5, '2024-01-15', 0),
('CND004', 'GV004', 'CTD004', 7.0, '2024-01-15', 0),
('CND005', 'GV005', 'CTD005', 8.0, '2024-01-15', 1);
