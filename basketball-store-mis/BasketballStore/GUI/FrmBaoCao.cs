using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BasketballStore.DAL;

namespace BasketballStore.GUI
{
    public class FrmBaoCao : Form
    {
        readonly DateTimePicker dtF = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 110 };
        readonly DateTimePicker dtT = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 110 };
        readonly DataGridView gDT = UI.Grid(), gTop = UI.Grid(), gTon = UI.Grid();
        readonly TabControl tabs = new TabControl { Dock = DockStyle.Fill };
        readonly Label lbSum = new Label { AutoSize = true, Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = UI.Accent, Margin = new Padding(16, 6, 0, 0) };

        public FrmBaoCao()
        {
            Text = "Báo cáo doanh thu và tồn kho";
            Width = 1000; Height = 600;
            dtF.Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            dtT.Value = DateTime.Today;

            AddTab("Doanh thu theo ngày", gDT);
            AddTab("Top 10 bán chạy", gTop);
            AddTab("Hàng sắp hết (tồn từ 5 trở xuống)", gTon);

            var bar = UI.Bar();
            bar.Controls.AddRange(new Control[]
            {
                UI.Lbl("Từ ngày"), dtF, UI.Lbl("Đến ngày"), dtT,
                UI.Btn("Xem báo cáo", delegate { Xem(); }, true), UI.Btn("Xuất CSV (tab hiện tại)", XuatCsv), lbSum
            });
            Controls.Add(tabs); Controls.Add(bar);
            Xem();
        }

        void AddTab(string title, DataGridView g)
        {
            var t = new TabPage(title);
            t.Controls.Add(g);
            tabs.TabPages.Add(t);
        }

        static decimal Tong(DataTable t, string col)
        {
            object o = t.Compute("SUM(" + col + ")", "");
            return o is DBNull ? 0m : Convert.ToDecimal(o);
        }

        void Xem()
        {
            if (dtF.Value.Date > dtT.Value.Date) { UI.Error("Ngày bắt đầu phải trước hoặc bằng ngày kết thúc."); return; }
            DateTime tu = dtF.Value.Date, den = dtT.Value.Date.AddDays(1);
            try
            {
                var dt = BaoCaoDAL.DoanhThuTheoNgay(tu, den);
                gDT.DataSource = dt;
                UI.Cols(gDT, "Ngay", "Ngày", "SoHD", "Số hóa đơn", "DoanhThu", "Doanh thu", "LoiNhuan", "Lợi nhuận gộp");
                UI.Money(gDT, "DoanhThu", "LoiNhuan");
                gDT.Columns["Ngay"].DefaultCellStyle.Format = "dd/MM/yyyy";

                gTop.DataSource = BaoCaoDAL.TopBanChay(tu, den);
                UI.Cols(gTop, "TenSP", "Sản phẩm", "Size", "Size", "SoLuongBan", "Đã bán", "DoanhThu", "Doanh thu");
                UI.Money(gTop, "DoanhThu");

                gTon.DataSource = BaoCaoDAL.TonThap(5);
                UI.Cols(gTon, "TenSP", "Sản phẩm", "Size", "Size", "Hang", "Hãng", "SoLuongTon", "Tồn", "TenNCC", "Nhà cung cấp");

                lbSum.Text = "Doanh thu: " + UI.Vnd(Tong(dt, "DoanhThu")) + "   |   Lợi nhuận gộp: " + UI.Vnd(Tong(dt, "LoiNhuan"));
            }
            catch (Exception ex) { UI.Error("Không tải được báo cáo: " + ex.Message); }
        }

        static string Csv(string s) { return "\"" + (s ?? "").Replace("\"", "\"\"") + "\""; }

        void XuatCsv(object sender, EventArgs e)
        {
            var g = (DataGridView)tabs.SelectedTab.Controls[0];
            using (var dlg = new SaveFileDialog { Filter = "CSV (*.csv)|*.csv", FileName = "bao-cao.csv" })
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                var cols = g.Columns.Cast<DataGridViewColumn>().Where(c => c.Visible).ToList();
                var sb = new StringBuilder();
                sb.AppendLine(string.Join(",", cols.Select(c => Csv(c.HeaderText))));
                foreach (DataGridViewRow r in g.Rows)
                    sb.AppendLine(string.Join(",", cols.Select(c => Csv(Convert.ToString(r.Cells[c.Index].FormattedValue)))));
                File.WriteAllText(dlg.FileName, sb.ToString(), new UTF8Encoding(true));
                UI.Info("Đã xuất file: " + dlg.FileName);
            }
        }
    }
}
