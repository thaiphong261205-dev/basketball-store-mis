using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Windows.Forms;
using BasketballStore.BLL;
using BasketballStore.DAL;
using BasketballStore.Models;

namespace BasketballStore.GUI
{
    public class FrmBanHang : Form
    {
        readonly ComboBox cbSp = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 400 };
        readonly NumericUpDown nSl = new NumericUpDown { Minimum = 1, Maximum = 9999, Value = 1, Width = 70 };
        readonly TextBox tbTen = new TextBox { Width = 160 }, tbSdt = new TextBox { Width = 120 };
        readonly Label lbTong = new Label { AutoSize = true, Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = UI.Accent, Margin = new Padding(16, 0, 16, 0) };
        readonly BindingList<DongHang> cart = new BindingList<DongHang>();
        readonly DataGridView g = UI.Grid();

        public FrmBanHang()
        {
            Text = "Bán hàng";
            Width = 1000; Height = 600;

            g.DataSource = cart;
            UI.Cols(g, "MaSP", "", "TenSP", "Sản phẩm", "SoLuong", "SL", "DonGia", "Đơn giá", "ThanhTien", "Thành tiền");
            UI.Money(g, "DonGia", "ThanhTien");

            var top = UI.Bar();
            top.Controls.AddRange(new Control[]
            {
                UI.Lbl("Sản phẩm"), cbSp, UI.Lbl("SL"), nSl,
                UI.Btn("Thêm vào giỏ", ThemDong, true), UI.Btn("Bỏ dòng chọn", BoDong)
            });
            var bot = UI.Bar();
            bot.Dock = DockStyle.Bottom;
            bot.Controls.AddRange(new Control[]
            {
                UI.Lbl("Khách hàng (tùy chọn)"), tbTen, UI.Lbl("SĐT (để tích điểm)"), tbSdt,
                lbTong, UI.Btn("Thanh toán", ThanhToan, true)
            });

            Controls.Add(g); Controls.Add(bot); Controls.Add(top);
            TaiSanPham();
            CapNhatTong();
        }

        void TaiSanPham()
        {
            cbSp.DataSource = null;
            cbSp.DataSource = SanPhamDAL.DangBan(true);
            cbSp.DisplayMember = "HienThi";
        }

        void CapNhatTong() { lbTong.Text = "Tổng: " + UI.Vnd(cart.Sum(x => x.ThanhTien)); }

        void ThemDong(object s, EventArgs e)
        {
            var p = cbSp.SelectedItem as SanPham;
            if (p == null) { UI.Error("Không còn sản phẩm để bán."); return; }
            int sl = (int)nSl.Value;
            var d = cart.FirstOrDefault(x => x.MaSP == p.MaSP);
            int daCo = d == null ? 0 : d.SoLuong;
            if (daCo + sl > p.SoLuongTon)
            {
                UI.Error(string.Format("Chỉ còn {0} sản phẩm \"{1}\" trong kho.", p.SoLuongTon, p.TenSP));
                return;
            }
            if (d == null)
                cart.Add(new DongHang { MaSP = p.MaSP, TenSP = p.TenSP, SoLuong = sl, DonGia = p.GiaBan });
            else
            {
                d.SoLuong += sl;
                cart.ResetItem(cart.IndexOf(d));
            }
            CapNhatTong();
        }

        void BoDong(object s, EventArgs e)
        {
            if (g.CurrentRow == null) return;
            cart.RemoveAt(g.CurrentRow.Index);
            CapNhatTong();
        }

        void ThanhToan(object s, EventArgs e)
        {
            if (cart.Count == 0) { UI.Error("Giỏ hàng đang trống."); return; }
            int id;
            try { id = BanHangBLL.ThanhToan(Session.Current.MaNV, tbTen.Text, tbSdt.Text, cart); }
            catch (InvalidOperationException ex) { UI.Error(ex.Message); TaiSanPham(); return; }
            catch (Exception ex) { UI.Error("Lỗi hệ thống: " + ex.Message); return; }

            var dong = new List<DongHang>(cart);
            decimal tong = dong.Sum(x => x.ThanhTien);
            string kh = string.IsNullOrWhiteSpace(tbTen.Text) ? "Khách lẻ" : tbTen.Text.Trim();
            cart.Clear(); tbTen.Clear(); tbSdt.Clear();
            TaiSanPham(); CapNhatTong();
            if (UI.Ask("Thanh toán thành công. Hóa đơn HD" + id.ToString("D4") + ".\nBạn có muốn in hóa đơn?"))
                InHoaDon(id, kh, dong, tong);
        }

        void InHoaDon(int id, string kh, List<DongHang> dong, decimal tong)
        {
            var pd = new PrintDocument();
            pd.PrintPage += delegate(object s, PrintPageEventArgs e)
            {
                var gr = e.Graphics;
                using (var f = new Font("Segoe UI", 10))
                using (var fb = new Font("Segoe UI", 14, FontStyle.Bold))
                {
                    float y = 40;
                    gr.DrawString("CỬA HÀNG DỤNG CỤ BÓNG RỔ", fb, Brushes.Black, 40, y); y += 36;
                    gr.DrawString("Hóa đơn HD" + id.ToString("D4") + "  -  " + DateTime.Now.ToString("dd/MM/yyyy HH:mm"), f, Brushes.Black, 40, y); y += 22;
                    gr.DrawString("Thu ngân: " + Session.Current.HoTen + "   Khách: " + kh, f, Brushes.Black, 40, y); y += 36;
                    foreach (var l in dong)
                    {
                        gr.DrawString(l.TenSP, f, Brushes.Black, 40, y);
                        gr.DrawString(l.SoLuong + " x " + UI.Vnd(l.DonGia), f, Brushes.Black, 340, y);
                        gr.DrawString(UI.Vnd(l.ThanhTien), f, Brushes.Black, 560, y);
                        y += 22;
                    }
                    y += 12;
                    gr.DrawString("TỔNG CỘNG: " + UI.Vnd(tong), fb, Brushes.Black, 40, y);
                }
            };
            using (var dlg = new PrintPreviewDialog { Document = pd, Width = 900, Height = 700 })
                dlg.ShowDialog(this);
        }
    }
}
