-- =====================================================================
-- CSDL QLBongRo - Quản lý cửa hàng dụng cụ bóng rổ (SQL Server 2008 trở lên)
-- Chạy MỘT lần trên CSDL mới. Muốn làm lại: DROP DATABASE QLBongRo rồi chạy lại.
-- Tài khoản nhân viên được chương trình tự tạo ở lần chạy đầu (mật khẩu đã băm).
-- =====================================================================
IF DB_ID(N'QLBongRo') IS NULL CREATE DATABASE QLBongRo;
GO
USE QLBongRo;
GO

CREATE TABLE LoaiSP (
    MaLoai  INT IDENTITY PRIMARY KEY,
    TenLoai NVARCHAR(100) NOT NULL UNIQUE);

CREATE TABLE NhaCungCap (
    MaNCC  INT IDENTITY PRIMARY KEY,
    TenNCC NVARCHAR(150) NOT NULL,
    SDT    VARCHAR(15),
    DiaChi NVARCHAR(200));

CREATE TABLE SanPham (
    MaSP       INT IDENTITY PRIMARY KEY,
    TenSP      NVARCHAR(150) NOT NULL,
    MaLoai     INT NOT NULL REFERENCES LoaiSP(MaLoai),
    MaNCC      INT NULL REFERENCES NhaCungCap(MaNCC),
    Hang       NVARCHAR(50),
    Size       NVARCHAR(20),
    GiaNhap    DECIMAL(18,0) NOT NULL CHECK (GiaNhap >= 0),
    GiaBan     DECIMAL(18,0) NOT NULL CHECK (GiaBan > 0),
    SoLuongTon INT NOT NULL DEFAULT 0 CONSTRAINT CK_SanPham_Ton CHECK (SoLuongTon >= 0),
    TrangThai  BIT NOT NULL DEFAULT 1);          -- 1 = đang bán, 0 = ngừng bán (không xóa cứng)

CREATE TABLE KhachHang (
    MaKH        INT IDENTITY PRIMARY KEY,
    HoTen       NVARCHAR(100) NOT NULL,
    SDT         VARCHAR(15) NULL,
    DiemTichLuy INT NOT NULL DEFAULT 0);
CREATE UNIQUE INDEX UX_KhachHang_SDT ON KhachHang(SDT) WHERE SDT IS NOT NULL;

CREATE TABLE NhanVien (
    MaNV        INT IDENTITY PRIMARY KEY,
    HoTen       NVARCHAR(100) NOT NULL,
    TenDangNhap VARCHAR(50) NOT NULL UNIQUE,
    MatKhauHash VARCHAR(100) NOT NULL,
    VaiTro      VARCHAR(20) NOT NULL CHECK (VaiTro IN ('Admin','ThuNgan','ThuKho')),
    TrangThai   BIT NOT NULL DEFAULT 1);

CREATE TABLE HoaDon (
    MaHD     INT IDENTITY PRIMARY KEY,
    NgayLap  DATETIME NOT NULL DEFAULT GETDATE(),
    MaNV     INT NOT NULL REFERENCES NhanVien(MaNV),
    MaKH     INT NULL REFERENCES KhachHang(MaKH),
    TongTien DECIMAL(18,0) NOT NULL DEFAULT 0);
CREATE INDEX IX_HoaDon_NgayLap ON HoaDon(NgayLap);

CREATE TABLE ChiTietHoaDon (
    MaHD    INT NOT NULL REFERENCES HoaDon(MaHD),
    MaSP    INT NOT NULL REFERENCES SanPham(MaSP),
    SoLuong INT NOT NULL CHECK (SoLuong > 0),
    DonGia  DECIMAL(18,0) NOT NULL,
    GiaVon  DECIMAL(18,0) NOT NULL,              -- giá nhập tại thời điểm bán, để tính lợi nhuận
    PRIMARY KEY (MaHD, MaSP));

CREATE TABLE PhieuNhap (
    MaPN    INT IDENTITY PRIMARY KEY,
    NgayNhap DATETIME NOT NULL DEFAULT GETDATE(),
    MaNCC   INT NOT NULL REFERENCES NhaCungCap(MaNCC),
    MaNV    INT NOT NULL REFERENCES NhanVien(MaNV));

CREATE TABLE ChiTietPhieuNhap (
    MaPN    INT NOT NULL REFERENCES PhieuNhap(MaPN),
    MaSP    INT NOT NULL REFERENCES SanPham(MaSP),
    SoLuong INT NOT NULL CHECK (SoLuong > 0),
    GiaNhap DECIMAL(18,0) NOT NULL CHECK (GiaNhap >= 0),
    PRIMARY KEY (MaPN, MaSP));
GO

-- Bán hàng: tự trừ tồn kho. CHECK (SoLuongTon >= 0) sẽ chặn bán vượt tồn (lỗi 547).
CREATE TRIGGER trg_BanHang ON ChiTietHoaDon AFTER INSERT AS
BEGIN
    SET NOCOUNT ON;
    UPDATE s SET s.SoLuongTon = s.SoLuongTon - x.SL
    FROM SanPham s
    JOIN (SELECT MaSP, SUM(SoLuong) AS SL FROM inserted GROUP BY MaSP) x ON x.MaSP = s.MaSP;
END
GO

-- Nhập hàng: tự cộng tồn kho.
CREATE TRIGGER trg_NhapHang ON ChiTietPhieuNhap AFTER INSERT AS
BEGIN
    SET NOCOUNT ON;
    UPDATE s SET s.SoLuongTon = s.SoLuongTon + x.SL
    FROM SanPham s
    JOIN (SELECT MaSP, SUM(SoLuong) AS SL FROM inserted GROUP BY MaSP) x ON x.MaSP = s.MaSP;
END
GO

-- Dữ liệu mẫu
INSERT LoaiSP(TenLoai) VALUES (N'Bóng'), (N'Giày'), (N'Quần áo'), (N'Phụ kiện');
INSERT NhaCungCap(TenNCC, SDT, DiaChi) VALUES
    (N'Công ty Thể thao Sài Gòn', '0281234567', N'Quận 1, TP.HCM'),
    (N'Đại lý Nike Việt Nam',     '0287654321', N'Quận 3, TP.HCM'),
    (N'Nhà phân phối Molten',     '0289998888', N'Thủ Đức, TP.HCM');
INSERT SanPham(TenSP, MaLoai, MaNCC, Hang, Size, GiaNhap, GiaBan, SoLuongTon) VALUES
    (N'Bóng Spalding TF-1000', 1, 1, N'Spalding', N'Size 7',  900000, 1250000, 15),
    (N'Bóng Molten GG7X',      1, 3, N'Molten',   N'Size 7', 1100000, 1500000,  8),
    (N'Giày Nike LeBron 21',   2, 2, N'Nike',     N'42',     2600000, 3200000,  6),
    (N'Giày Anta KT9',         2, 1, N'Anta',     N'41',     1500000, 1990000, 10),
    (N'Áo đấu Lakers',         3, 2, N'Nike',     N'L',       350000,  550000, 20),
    (N'Băng cổ tay',           4, 2, N'Nike',     N'Free',     40000,   90000, 40),
    (N'Lưới rổ thi đấu',       4, 1, N'Spalding', N'Free',     70000,  120000,  4);
GO
