using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiGoldenstarServices.Models.RoltecApi
{
    public class CustomerRoltec
    {
        public string id { get; set; }

        public string name { get; set; }

        public string last_name_1 { get; set; }

        public string last_name_2 { get; set; }

        public string phone { get; set; }
        
        public string email { get; set; }

        public string rfc { get; set; }

        public string giro_cve { get; set; }
        
        public string cli_medio_contacto { get; set; }
        
        public string state_id { get; set; }
    }

   
    
}
