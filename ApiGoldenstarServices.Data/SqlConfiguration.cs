using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiGoldenstarServices.Data
{
    public class SqlConfiguration
    {
        public SqlConfiguration(string connectionString) => ConnetionString = connectionString;
        public string ConnetionString { get; set; }
    }
}
