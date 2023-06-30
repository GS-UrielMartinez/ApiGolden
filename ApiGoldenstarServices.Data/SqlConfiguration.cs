using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiGoldenstarServices.Data
{
    public class SqlConfiguration
    {
        public SqlConfiguration(string sqlConnectionString) => ConnectionString = sqlConnectionString;
        public string ConnectionString { get; set; }
    }
}
