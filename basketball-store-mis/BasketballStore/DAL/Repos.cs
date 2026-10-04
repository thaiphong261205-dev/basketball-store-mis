using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BasketballStore.Models;

namespace BasketballStore.DAL
{
    public static class SanPhamDAL
    {
        public static DataTable Search(string kw, bool all)
        {
            return Db.Table(
                "SELECT s.MaSP, s.TenSP, l.TenLoai, s.Hang, s.Size, s.GiaNhap, s.GiaBan, s.SoLuongTon, s.TrangThai, s.MaLoai, s.MaNCC " +
                "FROM SanPham s JOIN LoaiSP l ON l.MaLoai = s.MaLoai " +
                "WHERE (@all = 1 OR s.TrangThai = 1) AND (s.TenSP LIKE @k OR s.Hang LIKE @k OR l.TenLoai LIKE @k) " +
                "ORDER BY s.TenSP",
                "@all", all ? 1 : 0, "@k", "%" + kw + "%");
        }

        public static DataTable Loai() { return Db.Table("SELECT MaLoai, TenLoai FROM LoaiSP ORDER BY TenLoai"); }
        public static DataTable Ncc() { return Db.Table("SELECT MaNCC, TenNCC FROM NhaCungCap ORDER BY TenNCC"); }

        public static List<SanPham> DangBan(bool chiConHang)
        {
            var t = Db.Table(
                "SELECT MaSP, TenSP, Size, GiaNhap, GiaBan, SoLuongTon FROM SanPham " +
                "WHERE TrangThai = 1 AND (@c = 0 OR SoLuongTon > 0) ORDER BY TenSP", "@c", chiConHang ? 1 : 0);
            return t.Rows.Cast<DataRow>().Select(r => new SanPham
            {
                MaSP = (int)r["MaSP"],
                TenSP = (string)r["TenSP"],
                Size = r["Size"] as string ?? "",
                GiaNhap = (decimal)r["GiaNhap"],
                GiaBan = (decimal)r["GiaBan"],
                SoLuongTon = (int)r["SoLuongTon"]
            }).ToList();
        }

        public static void Insert(SanPham p)
        {
            Db.Exec("INSERT SanPham(TenSP, MaLoai, MaNCC, Hang, Size, GiaNhap, GiaBan) VALUES(@t, @l, @n, @h, @s, @gn, @gb)",
                "@t", p.TenSP, "@l", p.MaLoai, "@n", p.MaNCC, "@h", p.Hang, "@s", p.Size, "@gn", p.GiaNhap, "@gb", p.GiaBan);
        }

        public static void Update(SanPham p)
        {
            Db.Exec("UPDATE SanPham SET TenSP=@t, MaLoai=@l, MaNCC=@n, Hang=@h, Size=@s, GiaNhap=@gn, GiaBan=@gb WHERE MaSP=@id",
                "@t", p.TenSP, "@l", p.MaLoai, "@n", p.MaNCC, "@h", p.Hang, "@s", p.Size, "@gn", p.GiaNhap, "@gb", p.GiaBan, "@id", p.MaSP);
        }

        /// <summary>Không xóa cứng để giữ lịch sử hóa đơn: chỉ đổi trạng thái kinh doanh.</summary>
        public static void DoiTrangThai(int maSP, bool dangBan)
        {
            Db.Exec("UPDATE SanPham SET TrangThai=@t WHERE MaSP=@id", "@t", dangBan, "@id", maSP);
        }
    }

    public static class NhanVienDAL
    {
        public static int Dem() { return (int)Db.Scalar("SELECT COUNT(*) FROM NhanVien"); }

        public static void Them(string hoTen, string user, string hash, string vaiTro)
        {
            Db.Exec("INSERT NhanVien(HoTen, TenDangNhap, MatKhauHash, VaiTro) VALUES(@h, @u, @p, @r)",
                "@h", hoTen, "@u", user, "@p", hash, "@r", vaiTro);
        }

        public static DataRow Lay(string user)
        {
            var t = Db.Table("SELECT MaNV, HoTen, TenDangNhap, MatKhauHash, VaiTro, TrangThai FROM NhanVien WHERE TenDangNhap=@u", "@u", user);
            return t.Rows.Count == 0 ? null : t.Rows[0];
        }
    }

    public static class KhachHangDAL
    {
        public static int FindOrCreate(string ten, string sdt)
        {
            object o = Db.Scalar("SELECT MaKH FROM KhachHang WHERE SDT=@s", "@s", sdt);
            if (o != null) return (int)o;
            return (int)Db.Scalar("INSERT KhachHang(HoTen, SDT) VALUES(@t, @s); SELECT CAST(SCOPE_IDENTITY() AS INT)", "@t", ten, "@s", sdt);
        }
    }

    public static class HoaDonDAL
    {
        /// <summary>Lưu hóa đơn + chi tiết trong một giao dịch. Trigger trg_BanHang tự trừ tồn kho,
        /// ràng buộc CHECK(SoLuongTon &gt;= 0) chặn bán vượt tồn (lỗi 547).</summary>
        public static int Them(int maNV, int? maKH, IList<DongHang> lines)
        {
            int id = 0;
            decimal tong = lines.Sum(l => l.ThanhTien);
            Db.Transaction((c, tx) =>
            {
                id = (int)Db.Cmd(c, "INSERT HoaDon(MaNV, MaKH, TongTien) VALUES(@nv, @kh, @t); SELECT CAST(SCOPE_IDENTITY() AS INT)", tx,
                    new object[] { "@nv", maNV, "@kh", maKH, "@t", tong }).ExecuteScalar();
                foreach (var l in lines)
                    Db.Cmd(c, "INSERT ChiTietHoaDon(MaHD, MaSP, SoLuong, DonGia, GiaVon) SELECT @h, MaSP, @sl, @g, GiaNhap FROM SanPham WHERE MaSP=@sp", tx,
                        new object[] { "@h", id, "@sl", l.SoLuong, "@g", l.DonGia, "@sp", l.MaSP }).ExecuteNonQuery();
                if (maKH.HasValue)
                    Db.Cmd(c, "UPDATE KhachHang SET DiemTichLuy = DiemTichLuy + @d WHERE MaKH=@k", tx,
                        new object[] { "@d", (int)(tong / 100000m), "@k", maKH.Value }).ExecuteNonQuery();
            });
            return id;
        }
    }

    public static class PhieuNhapDAL
    {
        /// <summary>Lưu phiếu nhập; trigger trg_NhapHang tự cộng tồn kho.</summary>
        public static int Them(int maNV, int maNCC, IList<DongHang> lines)
        {
            int id = 0;
            Db.Transaction((c, tx) =>
            {
                id = (int)Db.Cmd(c, "INSERT PhieuNhap(MaNCC, MaNV) VALUES(@n, @nv); SELECT CAST(SCOPE_IDENTITY() AS INT)", tx,
                    new object[] { "@n", maNCC, "@nv", maNV }).ExecuteScalar();
                foreach (var l in lines)
                {
                    Db.Cmd(c, "INSERT ChiTietPhieuNhap(MaPN, MaSP, SoLuong, GiaNhap) VALUES(@p, @sp, @sl, @g)", tx,
                        new object[] { "@p", id, "@sp", l.MaSP, "@sl", l.SoLuong, "@g", l.DonGia }).ExecuteNonQuery();
                    Db.Cmd(c, "UPDATE SanPham SET GiaNhap=@g WHERE MaSP=@sp", tx,
                        new object[] { "@g", l.DonGia, "@sp", l.MaSP }).ExecuteNonQuery();
                }
            });
            return id;
        }
    }

    public static class BaoCaoDAL
    {
        public static DataTable DoanhThuTheoNgay(DateTime tu, DateTime denExclusive)
        {
            return Db.Table(
                "SELECT CAST(h.NgayLap AS DATE) AS Ngay, COUNT(DISTINCT h.MaHD) AS SoHD, " +
                "SUM(c.SoLuong * c.DonGia) AS DoanhThu, SUM(c.SoLuong * (c.DonGia - c.GiaVon)) AS LoiNhuan " +
                "FROM HoaDon h JOIN ChiTietHoaDon c ON c.MaHD = h.MaHD " +
                "WHERE h.NgayLap >= @f AND h.NgayLap < @t GROUP BY CAST(h.NgayLap AS DATE) ORDER BY Ngay",
                "@f", tu, "@t", denExclusive);
        }

        public static DataTable TopBanChay(DateTime tu, DateTime denExclusive)
        {
            return Db.Table(
                "SELECT TOP 10 s.TenSP, s.Size, SUM(c.SoLuong) AS SoLuongBan, SUM(c.SoLuong * c.DonGia) AS DoanhThu " +
                "FROM ChiTietHoaDon c JOIN HoaDon h ON h.MaHD = c.MaHD JOIN SanPham s ON s.MaSP = c.MaSP " +
                "WHERE h.NgayLap >= @f AND h.NgayLap < @t GROUP BY s.TenSP, s.Size ORDER BY SoLuongBan DESC",
                "@f", tu, "@t", denExclusive);
        }

        public static DataTable TonThap(int nguong)
        {
            return Db.Table(
                "SELECT s.TenSP, s.Size, s.Hang, s.SoLuongTon, n.TenNCC FROM SanPham s LEFT JOIN NhaCungCap n ON n.MaNCC = s.MaNCC " +
                "WHERE s.TrangThai = 1 AND s.SoLuongTon <= @n ORDER BY s.SoLuongTon, s.TenSP", "@n", nguong);
        }
    }
}
