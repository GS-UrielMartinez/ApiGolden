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
        public string id { get; set; }
        public string name { get; set; }
        public string lastname { get; set; }
        //public string last_name_2 { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string rfc { get; set; }
        public string user_rol_id { get; set; }
        public string avatar { get; set; }
        public string giro_cve { get; set; }
        public string cli_medio_contacto { get; set; }
        public string state_id { get; set; }


    }

    // Estructura para la api de roltec
    public class UserApiRoltec
    {
        public string email { get; set; }

        public string password { get; set; }
    }

    public class UserRoltecResponse
    {
        public string message { get; set; }

        public UserRoltec user { get; set; }

        public string accessToken { get; set; }
    }
}
