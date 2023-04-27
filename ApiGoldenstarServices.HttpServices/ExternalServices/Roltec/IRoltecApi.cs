using ApiGoldenstarServices.Models.Roltec;
using ApiGoldenstarServices.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApiGoldenstarServices.Models.RoltecApi;
using ApiGoldenstarServices.Models.ServicesModels.RoltecApi;
using System.Net.Http.Headers;

namespace ApiGoldenstarServices.HttpServices.ExternalServices.Roltec
{
    public interface IRoltecApi
    {
        Task<CustomerRoltec> AddCustomerToWeb(CustomerRoltec customerRoltec);

        Task UpdateInventoryProduct(Product product);

        Task<T> UpdateOrderStatus<T>(T item);

        Task UpdateZipCode(ZipCode zipCode);
    }
}
