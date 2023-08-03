
using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiGoldenstarServices.Models.Goldenstar
{
    public class CustomerResponse
    {
        public string IdCustomer { get; set; }
        public string CustomerKey { get; set; } //cve_cliente
        public string ParentCustomerKey { get; set; } //cli_cvematriz
        public string AgentKey { get; set; } //claveAgente

        public string Message { get; set; }
    }

    public class Customer
    {
        public string IdCustomer { get; set; } 
        public string CustomerKey { get; set; } 
        public string? ParentCustomerKey { get; set; } 

        public string ShoppingName { get; set; } 
        public string ShoppingFirstName { get; set; } 
        public int StateCode { get; set; } 
        public int ZoneCode { get; set; }
        public Byte KeyTurn { get; set; } //giro_cve
        public string Email { get; set; } //cli_email

        // La columna [inctclie].Cli_Medio  es de tipo [tinyint]  
        public Byte MeansOfContact { get; set; } //cli_medio

        public string? AgentKey { get; set; } 

        //TODO: Hasta el momento NO se guarda en ninguna Columna de la Tabla
        public string Credit { get; set; } = "NO SE GUARDA en ninguna Columna";

        // La columna [inctclie].cli_dias  es de tipo [smallint] 
        public Int16 CreditDays { get; set; } //credito_dias

        public bool CreditAvailable { get; set; }  //TODO: NO HAY REFERENCIAS,  corresponde a ¿CreditoActivo, CreditoWeb, ó Ninguno?
        public string ShoppingPhoneNumber { get; set; } 


        public BillingAddress BillingAddress { get; set; } 
        //public ShippingAddress ShippingAddress { get; set; } //TODO: Cambiar por Lista 
        public List<ShippingAddress>? ShippingAddressList { get; set; } 
    }

    //
}
