using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiGoldenstarServices.Models
{
    public class ShippingAddress
    {
        //To do: Agregar mas campos que sean necesarios en todas las 
        //paqueterias

        public string? ShippingAddressId { get; set; } // cve_sucursal
        
        public string? CustomerId  { get; set; }//cve_cliente

        public string? Alias { get; set; }//sucursal

        public string? Street { get; set; } //calle

        public string? Colony { get; set; } //colonia

        public string? City { get; set; } //ciudad

        public string? ZipCode { get; set; } //codigo_postal

        public string? CityKey { get; set; } //cve_ciudad

        public string? Phone { get; set; }//telefono

    }
}
