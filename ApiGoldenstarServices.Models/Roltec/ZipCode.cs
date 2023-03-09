using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiGoldenstarServices.Models.Roltec
{
    public class ZipCode
    {
        public int id { get; set; }

        public string postal_code { get; set; }

        public string pob_cve { get; set; }

        public string pob_des { get; set; }

        public string pob_municipio { get; set; }

        public int pob_edo { get; set; }

        public string edo_des { get; set; }

        public string tasa_iva { get; set; }

        public string tiempo_entrega { get; set; }

        public string is_tablerate { get; set; }
    }
}
