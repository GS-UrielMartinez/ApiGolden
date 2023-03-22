using ApiGoldenstarServices.Models.Roltec;
using ApiGoldenstarServices.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ApiGoldenstarServices.HttpServices.ExternalServices.Roltec
{
    public interface IRoltecApi
    {
        //Task<CustomerRoltec> AddCustomerToWeb(CustomerRoltec customer);
        Task<string> GetTokenAsync(UserApi2 user);
    }
}
