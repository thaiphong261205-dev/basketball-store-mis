using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace BasketballStore.DAL
{
    /// <summary>Lớp truy cập CSDL dùng chung. Mọi truy vấn đều dùng tham số (chống SQL Injection).
    /// Tham số truyền theo cặp: tên, giá trị.</summary>
    public static class Db
    {
        static string ConnStr
        {
            get { return ConfigurationManager.ConnectionStrings["QLBongRo"].ConnectionString; }
        }

        public static SqlConnection Open()
        {
            var c = new SqlConnection(ConnStr);
            c.Open();
            return c;
        }

        public static SqlCommand Cmd(SqlConnection c, string sql, SqlTransaction tx, object[] p)
        {
            var cmd = new SqlCommand(sql, c, tx);
            for (int i = 0; i + 1 < p.Length; i += 2)
                cmd.Parameters.AddWithValue((string)p[i], p[i + 1] ?? DBNull.Value);
            return cmd;
        }

        public static DataTable Table(string sql, params object[] p)
        {
            using (var c = Open())
            using (var cmd = Cmd(c, sql, null, p))
            using (var ad = new SqlDataAdapter(cmd))
            {
                var t = new DataTable();
                ad.Fill(t);
                return t;
            }
        }

        public static int Exec(string sql, params object[] p)
        {
            using (var c = Open())
            using (var cmd = Cmd(c, sql, null, p))
                return cmd.ExecuteNonQuery();
        }

        public static object Scalar(string sql, params object[] p)
        {
            using (var c = Open())
            using (var cmd = Cmd(c, sql, null, p))
                return cmd.ExecuteScalar();
        }

        /// <summary>Chạy nhiều lệnh trong một giao dịch: thành công thì commit, lỗi thì rollback.</summary>
        public static void Transaction(Action<SqlConnection, SqlTransaction> work)
        {
            using (var c = Open())
            using (var tx = c.BeginTransaction())
            {
                try
                {
                    work(c, tx);
                    tx.Commit();
                }
                catch
                {
                    try { tx.Rollback(); }
                    catch (InvalidOperationException) { /* SQL Server đã tự rollback */ }
                    throw;
                }
            }
        }
    }
}
