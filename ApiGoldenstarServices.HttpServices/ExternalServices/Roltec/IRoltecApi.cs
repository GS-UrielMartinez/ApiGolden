using ApiGoldenstarServices.Models.Roltec;
using ApiGoldenstarServices.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApiGoldenstarServices.Models.RoltecApi;

namespace ApiGoldenstarServices.HttpServices.ExternalServices.Roltec
{
    public interface IRoltecApi
    {
        Task AddCustomerToWeb(CustomerRoltec customerRoltec);
        Task<string> GetTokenAsync(UserApiRoltec userRoltec);
    }
}
