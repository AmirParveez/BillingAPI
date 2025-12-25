using Microsoft.Data.SqlClient;
using System.Data;

namespace api.Helpers
{
    public class SqlHelper
    {
        private readonly string _conn;

        public SqlHelper(string connectionString)
        {
            _conn = connectionString;
        }

        public DataTable ExecuteDataTable(string query, SqlParameter[]? parameters = null)
        {
            using SqlConnection con = new SqlConnection(_conn);
            using SqlCommand cmd = new SqlCommand(query, con);

            if (parameters != null)
                cmd.Parameters.AddRange(parameters);

            using SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }
    }
}
