using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiGoldenstarServices.Models.Goldenstar
{
    public class ShippingAddress
    {
        public string? ShippingAddressId { get; set; } 
        public string? CustomerKey { get; set; }  
        //public string? IdCustomer { get; set; }  // DEBERÍA SER  CustomerKey 
        //public string? CustomerId { get; set; }//cve_cliente  // NO tiene referencias, podría cambiarse a "IdCustomer"
        public string? Alias { get; set; } //sucursal

        public string? FullAddress { get; set; } //Domicilio  [ = (Street + ' ' + Colony) ]
        public string? Street { get; set; } // NO tiene columna en BD :  Domicilio = (Street + ' ' + Colony) 
        public string? Colony { get; set; } // NO tiene columna en BD :  Domicilio = (Street + ' ' + Colony) 

        public string? City { get; set; } 
        public string? ZipCode { get; set; } //codigo_postal
        public string? CityKey { get; set; } //cve_ciudad
        public string? Phone { get; set; } 
    }
}
