namespace BasketballStore.Models
{
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string ThuNgan = "ThuNgan";
        public const string ThuKho = "ThuKho";
    }

    public class NhanVien
    {
        public int MaNV { get; set; }
        public string HoTen { get; set; }
        public string TenDangNhap { get; set; }
        public string VaiTro { get; set; }
    }

    public class SanPham
    {
        public int MaSP { get; set; }
        public string TenSP { get; set; }
        public int MaLoai { get; set; }
        public int? MaNCC { get; set; }
        public string Hang { get; set; }
        public string Size { get; set; }
        public decimal GiaNhap { get; set; }
        public decimal GiaBan { get; set; }
        public int SoLuongTon { get; set; }

        public string HienThi
        {
            get { return string.Format("{0} ({1}) - tồn {2}", TenSP, Size, SoLuongTon); }
        }
    }

    /// <summary>Một dòng hàng trong hóa đơn bán hoặc phiếu nhập (DonGia là giá bán hoặc giá nhập).</summary>
    public class DongHang
    {
        public int MaSP { get; set; }
        public string TenSP { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal ThanhTien { get { return SoLuong * DonGia; } }
    }
}
