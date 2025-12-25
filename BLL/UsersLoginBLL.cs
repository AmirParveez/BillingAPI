using api.Helpers;
using api.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace api.BLL
{
    public class UserLoginBLL
    {
        private readonly SqlHelper _sqlHelper;

        public UserLoginBLL(SqlHelper sqlHelper)
        {
            _sqlHelper = sqlHelper;
        }

        public UserLoginModel? GetUser(string username, string password)
        {
            string query = @"
                SELECT UserId, Username, Password, Current_Session
                FROM UserLogin
                WHERE Username = @username AND Password = @password";

            SqlParameter[] parameters =
            {
                new SqlParameter("@username", username),
                new SqlParameter("@password", password)
            };

            DataTable dt = _sqlHelper.ExecuteDataTable(query, parameters);

            if (dt.Rows.Count == 0)
                return null;

            DataRow dr = dt.Rows[0];

            return new UserLoginModel
            {
                UserId = Convert.ToInt32(dr["UserId"]),
                Username = dr["Username"].ToString()!,
                Password = dr["Password"].ToString()!,
                Current_Session = dr["Current_Session"].ToString()!
            };
        }
    }
}
