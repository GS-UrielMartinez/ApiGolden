using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiGoldenstarServices.Models.Roltec
{
    public class UserRoltec
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string last_name_1 { get; set; }
        public string last_name_2 { get; set; }
        public string email { get; set; }
        public string Phone { get; set; }
        public string rfc { get; set; }
        public string user_rol_id { get; set; }


    }

    public class UserRoltecResponse
    {
        public string Message { get; set; }

        public UserRoltec UserRoltec { get; set; }

        public string AccesToken { get; set; }
    }
}
