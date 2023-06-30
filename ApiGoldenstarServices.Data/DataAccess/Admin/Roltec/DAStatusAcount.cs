using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiGoldenstarServices.Data.DataAccess.Admin.Roltec
{
    public class DAStatusAcount : IStatusAcount
    {
        private SqlConfiguration _conectionString;

        public DAStatusAcount(SqlConfiguration connectionString)
        {
            _conectionString = connectionString;
        }

        // Get connection to Database
        protected SqlConnection DbConnection()
        {
            return new SqlConnection(_conectionString.ConnectionString);
        }


    }
}
