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
            _dataServices = new HttpDataServicesBase("http://137.184.87.110:80/");
            
        }


        public async Task AddCustomerToWeb(CustomerRoltec customerRoltec)
        {
            //to do: obtener los valores desde al app settings
            var userRoltec = new UserApiRoltec
            {
                email = "admin1@roltec.mx",
                password = "M2Gt_ky7NL+?p"
            };
            

            var token = await GetTokenAsync(userRoltec);


            try
            {
                //var newCustomer = await _dataServices.PostAsJsonAsync<CustomerRoltec>("api/users/data", customerRoltec, token);

            }catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }



       //get token from roltec.mx
        public async Task<string> GetTokenAsync(UserApiRoltec userApiRoltec)
        {
            try
            {
                var token = await _dataServices.PostAsJsonAsyncItem("api/sessions/data", userApiRoltec);
                var contentResponse = await token.Content.ReadAsStringAsync();
                UserRoltecResponse newToken = await Task.Run(() => JsonConvert.DeserializeObject<UserRoltecResponse>(contentResponse));

                return newToken.accessToken;
            }
            catch
            {
                throw;
            }

        }
    }
}
