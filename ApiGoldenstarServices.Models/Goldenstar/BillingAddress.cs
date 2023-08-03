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
        public string FullName { get; set; } 
        public string Rfc { get; set; }
        public string CompanyName { get; set; } 

        public string TaxRegime { get; set; } //regimenFiscal
        public string PaymentTypeKey { get; set; } //cve_forma_pago
        public string PaymentMethodKey { get; set; } //cve_metodo_pago
        public string Email { get; set; }


        // Se Utiliza en Order, pero NO en Customer
        // ( NO se guarda en ninguna columna de la tabla  [inctclie] )
        // ( SÍ se guarda en la columna  [Magento_H].[dias_credito] )
        public Int16 CreditDays { get; set; } //dias_credito
        //public string CreditDays { get; set; } //dias_credito


        public string CfdiUsageKey { get; set; } //cve_uso_cfdi

        public string ZipCode { get; set; }

        // Se Utiliza en Order, 
        // en Customer se asigna a un parámetro de StoredProcedure que NO se guarda en ninguna columna 
        public string City { get; set; }

        // Se Utiliza en Order, pero NO en Customer
        public string CityKey { get; set; }

        // NO Guarda ni Muestra ningún valor para BillingAddress,
        // se usa para obtener la Zona y el AgenteWeb  en Customer (inctclie) únicamente al darlo de alta (insert). 
        public int State { get; set; }  

        public string Street { get; set; }
        public string Colony { get; set; }
    }
}
