using System;
using System.Data;
using System.Windows.Forms;
using BasketballStore.BLL;
using BasketballStore.DAL;
using BasketballStore.Models;

namespace BasketballStore.GUI
{
    public class FrmSanPham : Form
    {
        readonly DataGridView g = UI.Grid();
        readonly TextBox tbK = new TextBox { Width = 220 };
        readonly TextBox tbTen = new TextBox { Width = 280 }, tbHang = new TextBox { Width = 280 }, tbSize = new TextBox { Width = 280 };
        readonly ComboBox cbLoai = UI.Combo(), cbNcc = UI.Combo();
        readonly NumericUpDown nGN = UI.Num(), nGB = UI.Num();
        readonly Label lbTon = new Label { AutoSize = true, Text = "Tồn kho: 0" };
        readonly CheckBox chkAll = new CheckBox { Text = "Hiện cả hàng ngừng bán", AutoSize = true };
        readonly Button btnTT;
        int selId;
        bool selActive = true;

        public FrmSanPham()
        {
            Text = "Quản lý sản phẩm";
            Width = 1100; Height = 600;

            cbLoai.DataSource = SanPhamDAL.Loai();
            cbLoai.DisplayMember = "TenLoai"; cbLoai.ValueMember = "MaLoai";
            var ncc = SanPhamDAL.Ncc();
            var none = ncc.NewRow(); none["MaNCC"] = 0; none["TenNCC"] = "(Chưa chọn)";
            ncc.Rows.InsertAt(none, 0);
            cbNcc.DataSource = ncc;
            cbNcc.DisplayMember = "TenNCC"; cbNcc.ValueMember = "MaNCC";

            var right = new FlowLayoutPanel
            {
                Dock = DockStyle.Right, Width = 330, Padding = new Padding(12),
                FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true
            };
            Field(right, "Tên sản phẩm", tbTen); Field(right, "Loại", cbLoai); Field(right, "Nhà cung cấp", cbNcc);
            Field(right, "Hãng", tbHang); Field(right, "Size", tbSize);
            Field(right, "Giá nhập (đ)", nGN); Field(right, "Giá bán (đ)", nGB);
            right.Controls.Add(lbTon);
            btnTT = UI.Btn("Ngừng bán", DoiTrangThai);
            var btns = new FlowLayoutPanel { AutoSize = true, Margin = new Padding(0, 12, 0, 0) };
            btns.Controls.AddRange(new Control[] { UI.Btn("Thêm mới", delegate { MoiMoi(); }), UI.Btn("Lưu", Luu, true), btnTT });
            right.Controls.Add(btns);
            right.Enabled = !Session.Is(Roles.ThuNgan); // thu ngân chỉ được xem

            var bar = UI.Bar();
            var tim = UI.Btn("Tìm", delegate { Tai(); });
            bar.Controls.AddRange(new Control[] { UI.Lbl("Tìm theo tên, hãng, loại"), tbK, tim, chkAll });
            tbK.KeyDown += delegate(object s, KeyEventArgs e) { if (e.KeyCode == Keys.Enter) Tai(); };
            chkAll.CheckedChanged += delegate { Tai(); };
            g.SelectionChanged += delegate { DoVaoForm(); };

            Controls.Add(g); Controls.Add(right); Controls.Add(bar);
            Tai();
        }

        static void Field(Control parent, string label, Control c)
        {
            parent.Controls.Add(new Label { Text = label, AutoSize = true, Margin = new Padding(0, 8, 0, 2) });
            parent.Controls.Add(c);
        }

        void Tai()
        {
            g.DataSource = SanPhamDAL.Search(tbK.Text.Trim(), chkAll.Checked);
            UI.Cols(g, "MaLoai", "", "MaNCC", "", "MaSP", "Mã", "TenSP", "Tên sản phẩm", "TenLoai", "Loại", "Hang", "Hãng",
                "Size", "Size", "GiaNhap", "Giá nhập", "GiaBan", "Giá bán", "SoLuongTon", "Tồn", "TrangThai", "Đang bán");
            UI.Money(g, "GiaNhap", "GiaBan");
            DoVaoForm();
        }

        void MoiMoi()
        {
            selId = 0; selActive = true;
            tbTen.Clear(); tbHang.Clear(); tbSize.Clear();
            nGN.Value = 0; nGB.Value = 0;
            lbTon.Text = "Tồn kho: 0 (nhập hàng để tăng tồn)";
            btnTT.Enabled = false; btnTT.Text = "Ngừng bán";
            tbTen.Focus();
        }

        void DoVaoForm()
        {
            var rv = g.CurrentRow == null ? null : g.CurrentRow.DataBoundItem as DataRowView;
            if (rv == null) { MoiMoi(); return; }
            DataRow r = rv.Row;
            selId = (int)r["MaSP"]; selActive = (bool)r["TrangThai"];
            tbTen.Text = (string)r["TenSP"];
            tbHang.Text = r["Hang"] as string ?? "";
            tbSize.Text = r["Size"] as string ?? "";
            cbLoai.SelectedValue = r["MaLoai"];
            cbNcc.SelectedValue = r["MaNCC"] is DBNull ? (object)0 : r["MaNCC"];
            nGN.Value = (decimal)r["GiaNhap"]; nGB.Value = (decimal)r["GiaBan"];
            lbTon.Text = "Tồn kho: " + r["SoLuongTon"];
            btnTT.Enabled = true;
            btnTT.Text = selActive ? "Ngừng bán" : "Bán lại";
        }

        void Luu(object s, EventArgs e)
        {
            int ncc = (int)cbNcc.SelectedValue;
            var p = new SanPham
            {
                MaSP = selId, TenSP = tbTen.Text.Trim(), MaLoai = (int)cbLoai.SelectedValue,
                MaNCC = ncc == 0 ? (int?)null : ncc,
                Hang = tbHang.Text.Trim(), Size = tbSize.Text.Trim(), GiaNhap = nGN.Value, GiaBan = nGB.Value
            };
            string loi = SanPhamBLL.Validate(p);
            if (loi != null) { UI.Error(loi); return; }
            try
            {
                if (selId == 0) SanPhamDAL.Insert(p); else SanPhamDAL.Update(p);
                Tai();
                UI.Info("Đã lưu sản phẩm.");
            }
            catch (Exception ex) { UI.Error("Không lưu được: " + ex.Message); }
        }

        void DoiTrangThai(object s, EventArgs e)
        {
            if (selId == 0) return;
            string hanhDong = selActive ? "ngừng bán" : "bán lại";
            if (!UI.Ask("Xác nhận " + hanhDong + " sản phẩm \"" + tbTen.Text + "\"?")) return;
            SanPhamDAL.DoiTrangThai(selId, !selActive);
            Tai();
        }
    }
}
