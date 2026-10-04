using System;
using System.Windows.Forms;
using BasketballStore.BLL;
using BasketballStore.Models;

namespace BasketballStore.GUI
{
    /// <summary>Form MDI chính. Menu hiển thị theo vai trò (phân quyền).</summary>
    public class FrmMain : Form
    {
        public bool DangXuat;

        public FrmMain()
        {
            Text = "Quản lý cửa hàng dụng cụ bóng rổ";
            WindowState = FormWindowState.Maximized;
            IsMdiContainer = true;

            var ms = new MenuStrip();
            var he = new ToolStripMenuItem("Hệ thống");
            he.DropDownItems.Add("Đăng xuất", null, delegate { DangXuat = true; Close(); });
            he.DropDownItems.Add("Thoát", null, delegate { Close(); });
            ms.Items.Add(he);

            var dm = new ToolStripMenuItem("Danh mục");
            Them(dm, "Sản phẩm", delegate { Mo<FrmSanPham>(); }, Roles.Admin, Roles.ThuKho, Roles.ThuNgan);
            var nv = new ToolStripMenuItem("Nghiệp vụ");
            Them(nv, "Bán hàng", delegate { Mo<FrmBanHang>(); }, Roles.Admin, Roles.ThuNgan);
            Them(nv, "Nhập hàng", delegate { Mo<FrmNhapHang>(); }, Roles.Admin, Roles.ThuKho);
            var bc = new ToolStripMenuItem("Báo cáo");
            Them(bc, "Doanh thu và tồn kho", delegate { Mo<FrmBaoCao>(); }, Roles.Admin);
            foreach (var m in new[] { dm, nv, bc })
                if (m.DropDownItems.Count > 0) ms.Items.Add(m);

            var st = new StatusStrip();
            st.Items.Add(new ToolStripStatusLabel(string.Format("Nhân viên: {0}  |  Vai trò: {1}", Session.Current.HoTen, Session.Current.VaiTro)));

            MainMenuStrip = ms;
            Controls.Add(st);
            Controls.Add(ms);
        }

        static void Them(ToolStripMenuItem parent, string text, EventHandler h, params string[] roles)
        {
            if (Session.Is(roles)) parent.DropDownItems.Add(text, null, h);
        }

        void Mo<T>() where T : Form, new()
        {
            foreach (var f in MdiChildren)
                if (f is T) { f.Activate(); return; }
            var n = new T();
            n.MdiParent = this;
            n.Show();
        }
    }
}
