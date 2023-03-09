
using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiGoldenstarServices.Models
{
    public class Customer
    {
        public string idCliente { get; set; }

        public string cli_cvematriz { get; set; } = null;// viene vacio

        public string cve_cliente { get; set; }

        public string nombreCompra { get; set; }

        public string apellidoCompra { get; set; }

        public string cli_email { get; set; }

        public int CodigoEstado { get; set; }

        public string giro_cve { get; set; }

        public string cli_medio { get; set; }

        public string claveAgente { get; set; } = null;// response golden

        public string credito { get; set; }

        public string credito_dias { get; set; }

        public bool CreditAvilable { get; set; }

        public string Cli_ComprasCel { get; set; }

        public List<BillingAddress> billing_address { get; set; }

        public List<ShippingAddress> shipping_address { get; set; }

    }

    /// <summary>
    /// Regresa datos para la api de la pagina de roltec
    /// </summary>
    public class CustomerResponse
    {
        public string cli_cvematriz { get; set; }

        public string cve_cliente { get; set; }

        public string claveAgente { get; set; }

        public string Message { get; set; }

    }

}
