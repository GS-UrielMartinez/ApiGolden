
using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiGoldenstarServices.Models
{
    public class Customer
    {
        public string IdCustomer { get; set; }//SP=cve_cliente -- db= cli_claveExterna

        public string? ParentCustomerKey { get; set; } // viene vacio //matriz cve

        public string CustomerKey { get; set; } //SP=cve_cliente -- bd =cli_clave

        public string ShoppingName { get; set; }//nombreCompra

        public string ShoppingFirstName { get; set; } //apellidoCompra

        public string Email { get; set; } //cli_email

        public int StateCode { get; set; } //CodigoEstado

        public string KeyTurn { get; set; } //giro_cve

        public string MeansOfContact { get; set; } //cli_medio

        public string? AgentKey { get; set; } // response golden

        public string Credit { get; set; } //credito

        public string CreditDays { get; set; } //credito_dias

        public bool CreditAvilable { get; set; }

        public string ShoppingPhoneNumber { get; set; } //Cli_ComprasCel

        public BillingAddress BillingAddress { get; set; }

        public ShippingAddress ShippingAddress { get; set; }

    }

    /// <summary>
    /// Regresa datos para la api de la pagina de roltec
    /// </summary>
    public class CustomerResponse
    {
        public string ParentCustomerKey { get; set; } //cli_cvematriz

        public string CustomerKey { get; set; } //cve_cliente

        public string AgentKey { get; set; } //claveAgente

        public string Message { get; set; }

    }

}
