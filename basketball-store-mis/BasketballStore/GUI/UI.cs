using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace BasketballStore.GUI
{
    /// <summary>Các hàm dựng giao diện dùng chung để các form gọn và thống nhất.</summary>
    public static class UI
    {
        public static readonly Color Court = Color.FromArgb(14, 42, 71);
        public static readonly Color Accent = Color.FromArgb(184, 57, 46);
        static readonly CultureInfo Vi = new CultureInfo("vi-VN");

        public static string Vnd(decimal v) { return v.ToString("N0", Vi) + " đ"; }

        public static void Info(string m) { MessageBox.Show(m, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information); }
        public static void Error(string m) { MessageBox.Show(m, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        public static bool Ask(string m) { return MessageBox.Show(m, "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes; }

        public static DataGridView Grid()
        {
            var g = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                MultiSelect = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                EnableHeadersVisualStyles = false
            };
            g.ColumnHeadersDefaultCellStyle.BackColor = Court;
            g.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            g.ColumnHeadersHeight = 32;
            return g;
        }

        public static Button Btn(string text, EventHandler click, bool primary = false)
        {
            var b = new Button
            {
                Text = text,
                AutoSize = true,
                MinimumSize = new Size(0, 32),
                FlatStyle = FlatStyle.Flat,
                BackColor = primary ? Accent : Court,
                ForeColor = Color.White,
                Margin = new Padding(0, 0, 8, 0),
                Padding = new Padding(8, 2, 8, 2)
            };
            b.FlatAppearance.BorderSize = 0;
            b.Click += click;
            return b;
        }

        public static ComboBox Combo() { return new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 280 }; }

        public static NumericUpDown Num()
        {
            return new NumericUpDown { Maximum = 1000000000, ThousandsSeparator = true, Increment = 1000, Width = 280 };
        }

        public static FlowLayoutPanel Bar()
        {
            return new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(8), WrapContents = true };
        }

        public static Label Lbl(string t) { return new Label { Text = t, AutoSize = true, Margin = new Padding(0, 8, 6, 0) }; }

        /// <summary>Đổi tiêu đề cột theo cặp (tên cột, tiêu đề); tiêu đề rỗng nghĩa là ẩn cột.</summary>
        public static void Cols(DataGridView g, params string[] pairs)
        {
            for (int i = 0; i + 1 < pairs.Length; i += 2)
            {
                var c = g.Columns[pairs[i]];
                if (c == null) continue;
                if (pairs[i + 1].Length == 0) c.Visible = false; else c.HeaderText = pairs[i + 1];
            }
        }

        public static void Money(DataGridView g, params string[] names)
        {
            foreach (var n in names)
            {
                var c = g.Columns[n];
                if (c == null) continue;
                c.DefaultCellStyle.Format = "N0";
                c.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
        }
    }
}
