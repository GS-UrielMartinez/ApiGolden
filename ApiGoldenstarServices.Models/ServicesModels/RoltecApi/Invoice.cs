using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiGoldenstarServices.Models.ServicesModels.RoltecApi
{
    public class Invoice
    {
        public string Customerkey { get; set; }
        
        public string InvoiceNumber { get; set; }
        
        public string CreatedAt { get; set; }

        public double Amount { get; set; }

        public string InvoiceFilePDF { get; set; }

        public string InvoiceFileXML { get; set; }
    }

    public class InvoiceHistory
    {
        public string CustomerId { get; set; }

        public List<Invoice> Invoices { get; set; }
    }

    public class InvoiceResponse
    {
        public int IdInvoice { get; set; }

        public string TypeInvoice { get; set; }

        public string OrderId { get; set; }
    }
}
