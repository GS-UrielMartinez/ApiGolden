using ApiGoldenstarServices.Models;
using ApiGoldenstarServices.Models.Roltec;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CustomerRoltec = ApiGoldenstarServices.Models.Roltec.CustomerRoltec;

namespace ApiGoldenstarServices.HttpServices.ExternalServices.Roltec
{
    public class HttpRoltecServicesAPI : IRoltecApi
    {
        private HttpDataServicesBase _dataServices;

        public HttpRoltecServicesAPI()
        {
            _dataServices = new HttpDataServicesBase("https://dummyjson.com/");
            
        }


        public async Task<CustomerRoltec> AddCustomerToWeb(CustomerRoltec customer)
        {
            var user = new UserApi2
            {
                Username = "kminchelle",
                Password = "0lelplR"
            };
            var token = await GetTokenAsync(user);
            
            var newCustomer= await _dataServices.PostAsJsonAsync<CustomerRoltec>("api/users/data", customer,token);


            return newCustomer;
        }

        public async Task<string> GetTokenAsync(UserApi2 userApi)
        {
            try
            {

                var token = await _dataServices.PostAsJsonAsyncItem("auth/login", userApi);
                var contentResponse = await token.Content.ReadAsStringAsync();
                UserRoltecResponse newToken = await Task.Run(() => JsonConvert.DeserializeObject<UserRoltecResponse>(contentResponse));

                return newToken.AccesToken;
            }
            catch
            {
                throw;
            }

        }
    }
}
