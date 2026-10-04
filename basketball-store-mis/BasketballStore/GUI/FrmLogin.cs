using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using BasketballStore.BLL;

namespace BasketballStore.GUI
{
    public class FrmLogin : Form
    {
        readonly TextBox tbU = new TextBox { Left = 110, Top = 62, Width = 200 };
        readonly TextBox tbP = new TextBox { Left = 110, Top = 98, Width = 200, UseSystemPasswordChar = true };
        int fails;

        public FrmLogin()
        {
            Text = "Đăng nhập";
            ClientSize = new Size(340, 200);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            var title = new Label
            {
                Text = "Cửa hàng dụng cụ bóng rổ",
                Left = 0, Top = 0, Width = 340, Height = 44,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = UI.Court, ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };
            var ok = UI.Btn("Đăng nhập", DangNhap, true);
            ok.Left = 110; ok.Top = 138; ok.Width = 200;
            Controls.AddRange(new Control[]
            {
                title,
                new Label { Text = "Tài khoản", Left = 24, Top = 66, AutoSize = true },
                new Label { Text = "Mật khẩu", Left = 24, Top = 102, AutoSize = true },
                tbU, tbP, ok
            });
            AcceptButton = ok;
        }

        void DangNhap(object sender, EventArgs e)
        {
            try
            {
                var nv = AuthService.DangNhap(tbU.Text, tbP.Text);
                if (nv == null)
                {
                    fails++;
                    if (fails >= 5) { UI.Error("Sai quá 5 lần. Chương trình sẽ đóng."); DialogResult = DialogResult.Cancel; return; }
                    UI.Error("Sai tài khoản hoặc mật khẩu (lần " + fails + "/5).");
                    tbP.Clear();
                    tbP.Focus();
                    return;
                }
                Session.Current = nv;
                DialogResult = DialogResult.OK;
            }
            catch (SqlException ex)
            {
                UI.Error("Không kết nối được CSDL: " + ex.Message);
            }
        }
    }
}
