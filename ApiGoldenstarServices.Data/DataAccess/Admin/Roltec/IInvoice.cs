using ApiGoldenstarServices.Models.ServicesModels.RoltecApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiGoldenstarServices.Data.DataAccess.Admin.Roltec
{
    public interface IInvoice
    {
        Task<OrderRoltec> UpdateInvoiceToWebAsync(string folio);

        Task<Invoice> GetInvoiceAsync(string folio);

        Task<string> GetInvoiceFileAsync(string folio);
    }
}
