using ApiGoldenstarServices.Models.Goldenstar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiGoldenstarServices.Models.ServicesModels.RoltecApi
{
    public class OrderRoltec : Order
    {
        public InvoiceDetail InvoiceDetail { get; set; }
        public ShippingDetail? ShippingDetail { get; set; }
    }

    public class InvoiceDetail
    {
        public int IdInvoice { get; set; }

        public string TypeInvoice { get; set; }

        public string PDFInvoiceFile { get; set; }

        public string XMLInvoiceFile { get; set; }
    }

    public class ShippingDetail
    {
        public string? ParcelService { get; set; }

        public string? TrackingNumber { get; set; }

        public string? UrlTrakingNumbrer { get; set; }
    }
}
