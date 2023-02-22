
using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiGoldenstarServices.Models
{
    public class Customer
    {
        public string cve_cliente { get; set; }
        public string cli_email { get; set; }
        public string company_id { get; set; }
        public string rfc { get; set; }
        public string cli_nombre { get; set; }
        public string nombreCompra { get; set; }
        public string apellidoCompra { get; set; }
        public string cli_medio { get; set; }
        public string credito { get; set; }
        public string credito_dias { get; set; }
        public string giro_cve { get; set; }
        public string Cli_ComprasCel { get; set; }
        //public int Cli_MedioContacto { get; set; }
        //public string NombreCliente { get; set; } = string.Empty;
        //public string apellidoPaterno { get; set; } = string.Empty;
        //public string apellidoMaterno { get; set; } = string.Empty;
        public string denominacionSocial { get; set; } = string.Empty;
        public string regimenFiscal { get; set; } = string.Empty;
        // Verificar si se tienen que hacer tablas nuevas
        public List<BillingAddress> billing_address { get; set; }
        public List<ShippingAddress> shipping_address { get; set; }

    }

    public class BillingAddress
    {
        public string cve_sucursal { get; set; }
        public string sucursal { get; set; }
        public string calle { get; set; }
        public string colonia { get; set; }
        public string ciudad { get; set; }
        public string estado { get; set; }
        public string telefono { get; set; }
        public string pais { get; set; }
        public string codigo_postal { get; set; }
        public string cve_ciudad { get; set; }
        public string principal { get; set; }
    }

    public class ShippingAddress
    {
        public string cve_sucursal { get; set; }
        public string sucursal { get; set; }
        public string calle { get; set; }
        public string colonia { get; set; }
        public string ciudad { get; set; }
        public string estado { get; set; }
        public string telefono { get; set; }
        public string pais { get; set; }
        public string codigo_postal { get; set; }
        public string cve_ciudad { get; set; }
    }
}
