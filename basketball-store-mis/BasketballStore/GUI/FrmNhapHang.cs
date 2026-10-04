using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BasketballStore.BLL;
using BasketballStore.DAL;
using BasketballStore.Models;

namespace BasketballStore.GUI
{
    public class FrmNhapHang : Form
    {
        readonly ComboBox cbNcc = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 220 };
        readonly ComboBox cbSp = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 380 };
        readonly NumericUpDown nSl = new NumericUpDown { Minimum = 1, Maximum = 99999, Value = 10, Width = 80 };
        readonly NumericUpDown nGia = new NumericUpDown { Maximum = 1000000000, ThousandsSeparator = true, Increment = 1000, Width = 120 };
        readonly Label lbTong = new Label { AutoSize = true, Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = UI.Accent, Margin = new Padding(16, 0, 16, 0) };
        readonly BindingList<DongHang> lines = new BindingList<DongHang>();
        readonly DataGridView g = UI.Grid();

        public FrmNhapHang()
        {
            Text = "Nhập hàng";
            Width = 1000; Height = 600;

            cbNcc.DataSource = SanPhamDAL.Ncc();
            cbNcc.DisplayMember = "TenNCC"; cbNcc.ValueMember = "MaNCC";
            cbSp.DataSource = SanPhamDAL.DangBan(false);
            cbSp.DisplayMember = "HienThi";
            cbSp.SelectedIndexChanged += delegate
            {
                var p = cbSp.SelectedItem as SanPham;
                if (p != null) nGia.Value = p.GiaNhap;
            };
            var first = cbSp.SelectedItem as SanPham;
            if (first != null) nGia.Value = first.GiaNhap;

            g.DataSource = lines;
            UI.Cols(g, "MaSP", "", "TenSP", "Sản phẩm", "SoLuong", "SL", "DonGia", "Giá nhập", "ThanhTien", "Thành tiền");
            UI.Money(g, "DonGia", "ThanhTien");

            var top = UI.Bar();
            top.Controls.AddRange(new Control[]
            {
                UI.Lbl("Nhà cung cấp"), cbNcc, UI.Lbl("Sản phẩm"), cbSp, UI.Lbl("SL"), nSl, UI.Lbl("Giá nhập"), nGia,
                UI.Btn("Thêm dòng", ThemDong, true), UI.Btn("Bỏ dòng chọn", BoDong)
            });
            var bot = UI.Bar();
            bot.Dock = DockStyle.Bottom;
            bot.Controls.AddRange(new Control[] { lbTong, UI.Btn("Lưu phiếu nhập", Luu, true) });

            Controls.Add(g); Controls.Add(bot); Controls.Add(top);
            CapNhatTong();
        }

        void CapNhatTong() { lbTong.Text = "Tổng tiền nhập: " + UI.Vnd(lines.Sum(x => x.ThanhTien)); }

        void ThemDong(object s, EventArgs e)
        {
            var p = cbSp.SelectedItem as SanPham;
            if (p == null) return;
            var d = lines.FirstOrDefault(x => x.MaSP == p.MaSP);
            if (d == null)
                lines.Add(new DongHang { MaSP = p.MaSP, TenSP = p.TenSP, SoLuong = (int)nSl.Value, DonGia = nGia.Value });
            else
            {
                d.SoLuong += (int)nSl.Value; d.DonGia = nGia.Value;
                lines.ResetItem(lines.IndexOf(d));
            }
            CapNhatTong();
        }

        void BoDong(object s, EventArgs e)
        {
            if (g.CurrentRow == null) return;
            lines.RemoveAt(g.CurrentRow.Index);
            CapNhatTong();
        }

        void Luu(object s, EventArgs e)
        {
            try
            {
                int id = NhapHangBLL.Luu(Session.Current.MaNV, (int)cbNcc.SelectedValue, lines);
                lines.Clear(); CapNhatTong();
                cbSp.DataSource = SanPhamDAL.DangBan(false);
                UI.Info("Đã lưu phiếu nhập PN" + id.ToString("D4") + ". Tồn kho đã được cập nhật.");
            }
            catch (InvalidOperationException ex) { UI.Error(ex.Message); }
            catch (Exception ex) { UI.Error("Không lưu được phiếu nhập: " + ex.Message); }
        }
    }
}
