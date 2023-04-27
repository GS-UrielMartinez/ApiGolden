using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiGoldenstarServices.Models.Goldenstar
{
    public class BillingAddress
    {
        public string IdBillingAddress { get; set; }

        public string Name { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string FullName { get; set; } // de aqui se puede jalar denominacion social

        public string Rfc { get; set; }

        public string CompanyName { get; set; } //denominacionSocial

        public string TaxRegime { get; set; } //regimenFiscal

        public string PaymentTypeKey { get; set; } //cve_forma_pago

        public string PaymentMethodKey { get; set; } //cve_metodo_pago

        public string CreditDays { get; set; } //dias_credito

        public string CfdiUsageKey { get; set; } //cve_uso_cfdi

        public string ZipCode { get; set; }

        public string City { get; set; }

        public string CityKey { get; set; }

        public string State { get; set; }

        public string Street { get; set; }

        public string Colony { get; set; }
    }
}
