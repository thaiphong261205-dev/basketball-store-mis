using System;
using System.Windows.Forms;
using BasketballStore.BLL;
using BasketballStore.GUI;

namespace BasketballStore
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                AuthService.KhoiTaoTaiKhoanMau();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không kết nối được CSDL QLBongRo.\nHãy chạy docs\\schema.sql và kiểm tra chuỗi kết nối trong App.config.\n\n" + ex.Message,
                    "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            while (true)
            {
                using (var login = new FrmLogin())
                {
                    if (login.ShowDialog() != DialogResult.OK) return;
                }
                var main = new FrmMain();
                Application.Run(main);
                if (!main.DangXuat) return;
            }
        }
    }
}
