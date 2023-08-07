
using System.Security.Cryptography.X509Certificates;

namespace ApiGoldenstarServices.Models.Goldenstar
{
    public class FolioResponse
    {
        public string Folio { get; set; }
    }
    public class OrderResponse
    {
        public string Folio { get; set; }
        public string Message { get; set; }
    }

    public class Order
    {
        public string IdOrder { get; set; } //id_ord
        public string? Folio { get; set; } //folio
        public string CreatedAt { get; set; } //ord_fecha
        public string CustomerId { get; set; } //ord_cli
        public string CustomerName { get; set; } //cli_nombre
        public string PaymentMethod { get; set; } //metodo_pago
        public string Cupon { get; set; } //ord_cupon


        // Se cambiaron de string a Decimal, que es el tipo que les corresponde en la tabla [Magento_H]
        public decimal Discount { get; set; } //descuentoimporte 
        public decimal Subtotal { get; set; } //subtotal 
        public decimal Vat { get; set; } //iva 
        //public string Discount { get; set; } //descuentoimporte
        //public string Subtotal { get; set; } //subtotal
        //public string Vat { get; set; } //iva
 

        public string VatRate { get; set; } //tasa_iva
        public string Notes { get; set; } //notas
        public string Status { get; set; }

        public BillingAddress BillingAddress { get; set; }
        public ShippingAddress ShippingAddress { get; set; }

        public List<OrderDetail> OrderDetail { get; set; }
    }

    //
}