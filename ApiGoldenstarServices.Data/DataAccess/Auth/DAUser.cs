using ApiGoldenstarServices.Models.Goldenstar;
using Dapper;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiGoldenstarServices.Data.DataAccess.Auth
{
    public class DAUser : IUser
    {
        private SqlConfiguration _conectionString;

        public DAUser(SqlConfiguration conectionString)
        {
            _conectionString = conectionString;
        }

        // Get connection to Database
        public SqlConnection DbConnection()
        {
            return new SqlConnection(_conectionString.ConnetionString);
        }

        public async Task<User> ReadByEmail(string email)
        {
            var db = DbConnection();

            var sql = @" SELECT u.""Id"", u.""Name"", u.""Email"", u.""Password"", u.""isActive"", u.""Rol"" as Rol  
                         FROM ""User"" u
                         JOIN ""Rol"" r on u.""IdRol"" = r.""Id""
                         WHERE ""Email"" = @email";
            try
            {
                var user = await db.QueryFirstOrDefaultAsync<User>(sql, new { email });

                return user;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }


        }
    }
}
