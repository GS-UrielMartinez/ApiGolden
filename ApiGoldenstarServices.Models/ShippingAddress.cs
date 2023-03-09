using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiGoldenstarServices.Models
{
    public class ShippingAddress
    {
        public string? cve_cliente { get; set; }

        public string? cve_sucursal { get; set; }

        public string? sucursal { get; set; }

        public string? calle { get; set; }

        public string? colonia { get; set; }

        public string? ciudad { get; set; }

        public string? codigo_postal { get; set; }

        public string? cve_ciudad { get; set; }

        public string? telefono { get; set; }
    }
}
