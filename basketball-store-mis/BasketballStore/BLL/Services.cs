using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using BasketballStore.DAL;
using BasketballStore.Models;

namespace BasketballStore.BLL
{
    /// <summary>Băm mật khẩu bằng PBKDF2 (Rfc2898) + salt ngẫu nhiên 16 byte; lưu base64(salt + hash).</summary>
    public static class Security
    {
        const int Iterations = 10000;

        public static string Hash(string password)
        {
            var salt = new byte[16];
            using (var rng = new RNGCryptoServiceProvider()) rng.GetBytes(salt);
            var hash = new Rfc2898DeriveBytes(password, salt, Iterations).GetBytes(20);
            var all = new byte[36];
            Buffer.BlockCopy(salt, 0, all, 0, 16);
            Buffer.BlockCopy(hash, 0, all, 16, 20);
            return Convert.ToBase64String(all);
        }

        public static bool Verify(string password, string stored)
        {
            try
            {
                var all = Convert.FromBase64String(stored);
                if (all.Length != 36) return false;
                var salt = new byte[16];
                Buffer.BlockCopy(all, 0, salt, 0, 16);
                var hash = new Rfc2898DeriveBytes(password, salt, Iterations).GetBytes(20);
                int diff = 0;
                for (int i = 0; i < 20; i++) diff |= hash[i] ^ all[16 + i];
                return diff == 0;
            }
            catch (FormatException) { return false; }
        }
    }

    public static class Session
    {
        public static NhanVien Current { get; set; }

        public static bool Is(params string[] roles)
        {
            return Current != null && Array.IndexOf(roles, Current.VaiTro) >= 0;
        }
    }

    public static class AuthService
    {
        /// <summary>Lần chạy đầu (bảng NhanVien trống) tạo 3 tài khoản mẫu, mỗi vai trò một tài khoản.</summary>
        public static void KhoiTaoTaiKhoanMau()
        {
            if (NhanVienDAL.Dem() > 0) return;
            NhanVienDAL.Them("Quản trị viên", "admin", Security.Hash("admin123"), Roles.Admin);
            NhanVienDAL.Them("Thu ngân mẫu", "thungan", Security.Hash("thungan123"), Roles.ThuNgan);
            NhanVienDAL.Them("Thủ kho mẫu", "thukho", Security.Hash("thukho123"), Roles.ThuKho);
        }

        public static NhanVien DangNhap(string user, string pass)
        {
            var r = NhanVienDAL.Lay((user ?? "").Trim());
            if (r == null || !(bool)r["TrangThai"] || !Security.Verify(pass ?? "", (string)r["MatKhauHash"]))
                return null;
            return new NhanVien
            {
                MaNV = (int)r["MaNV"],
                HoTen = (string)r["HoTen"],
                TenDangNhap = (string)r["TenDangNhap"],
                VaiTro = (string)r["VaiTro"]
            };
        }
    }

    public static class SanPhamBLL
    {
        /// <summary>Trả về thông báo lỗi, hoặc null nếu hợp lệ.</summary>
        public static string Validate(SanPham p)
        {
            if (string.IsNullOrWhiteSpace(p.TenSP)) return "Tên sản phẩm không được để trống.";
            if (p.GiaBan <= 0) return "Giá bán phải lớn hơn 0.";
            if (p.GiaNhap < 0) return "Giá nhập không được âm.";
            if (p.GiaBan < p.GiaNhap) return "Giá bán không được thấp hơn giá nhập.";
            return null;
        }
    }

    public static class BanHangBLL
    {
        public static int ThanhToan(int maNV, string tenKhach, string sdt, IList<DongHang> cart)
        {
            if (cart == null || cart.Count == 0) throw new InvalidOperationException("Giỏ hàng đang trống.");
            int? maKH = null;
            sdt = (sdt ?? "").Trim();
            if (sdt.Length > 0)
            {
                if (!sdt.All(char.IsDigit) || sdt.Length < 9 || sdt.Length > 11)
                    throw new InvalidOperationException("Số điện thoại không hợp lệ (9 đến 11 chữ số).");
                string ten = string.IsNullOrWhiteSpace(tenKhach) ? "Khách " + sdt : tenKhach.Trim();
                maKH = KhachHangDAL.FindOrCreate(ten, sdt);
            }
            try
            {
                return HoaDonDAL.Them(maNV, maKH, cart);
            }
            catch (SqlException ex)
            {
                if (ex.Number == 547)
                    throw new InvalidOperationException("Có sản phẩm không đủ tồn kho. Hãy kiểm tra lại số lượng.");
                throw;
            }
        }
    }

    public static class NhapHangBLL
    {
        public static int Luu(int maNV, int maNCC, IList<DongHang> lines)
        {
            if (lines == null || lines.Count == 0) throw new InvalidOperationException("Phiếu nhập chưa có sản phẩm nào.");
            return PhieuNhapDAL.Them(maNV, maNCC, lines);
        }
    }
}
