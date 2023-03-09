using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiGoldenstarServices.Models
{
    public class BillingAddress
    {
        public string IdBillingAddress { get; set; }

        public string NombreCliente { get; set; }

        public string apellidoPaterno { get; set; }

        public string apellidoMaterno { get; set; }

        public string cli_nombre { get; set; }

        public string rfc { get; set; }

        public string denominacionSocial { get; set; }

        public string regimenFiscal { get; set; }

        public string cve_forma_pago { get; set; }

        public string cve_metodo_pago { get; set; }

        public string dias_credito { get; set; }

        public string cve_uso_cfdi { get; set; }

        public string codigo_postal { get; set; }

        public string ciudad { get; set; }

        public string cve_ciudad { get; set; }

        public string estado { get; set; }

        public string calle { get; set; }

        public string colonia { get; set; }
    }
}
