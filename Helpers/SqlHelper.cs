using Microsoft.Data.SqlClient;
using System.Data;

namespace ApiBilling.Helpers
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

        public int ExecuteNonQuery(string query, SqlParameter[]? parameters = null)
        {
            using SqlConnection con = new SqlConnection(_conn);
            using SqlCommand cmd = new SqlCommand(query, con);

            if (parameters != null)
                cmd.Parameters.AddRange(parameters);

            con.Open();
            return cmd.ExecuteNonQuery();
        }

        public object ExecuteScalar(string query, SqlParameter[]? parameters = null)
        {
            using SqlConnection con = new SqlConnection(_conn);
            using SqlCommand cmd = new SqlCommand(query, con);

            if (parameters != null)
                cmd.Parameters.AddRange(parameters);

            con.Open();
            return cmd.ExecuteScalar()!;
        }
    }
}
