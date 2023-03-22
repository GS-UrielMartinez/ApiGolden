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
        public int Id { get; set; }

        public string Name { get; set; }

        public string last_name_1 { get; set; }

        public string last_name_2 { get; set; }

        public string Phone { get; set; }
        
        public string Email { get; set; }

        public string rfc { get; set; } 
    }

    
}
