using ApiGoldenstarServices.Models;
using ApiGoldenstarServices.Models.Roltec;
using Newtonsoft.Json;
using CustomerRoltec = ApiGoldenstarServices.Models.RoltecApi.CustomerRoltec;

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

        public async Task AddCustomerToWeb()
        {
            var user = new UserApi2
            {
                Username = "kminchelle",
                Password = "0lelplR"
            };
            var token = await GetTokenAsync(user);


            try
            {
                //var newCustomer = await _dataServices.PostAsJsonAsync<UserApi2>("api/users/data", user, token);

            }catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }



        //public Task<CustomerRoltec> AddCustomerToWeb(CustomerRoltec customer)
        //{
        //    throw new NotImplementedException();
        //}

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
