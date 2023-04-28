using ApiGoldenstarServices.Models.Roltec;
using ApiGoldenstarServices.Models.RoltecApi;

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiGoldenstarServices.HttpServices.Utils
{
    public class HttpTokenServices : ITokenServices
    {
        //private readonly GetValuesFromEnvFile _envFile;
        //Por alguna razon no me deja implementar la clase, la opcion es quenerar una clase similar en esta bibilioteca de clases
        //e implementarla de la misma forma que en UrlFile.cs
        public static string hashValue { get; set; }
        private HttpDataServicesBase _dataServices;
        public HttpTokenServices()
        {
            _dataServices = new HttpDataServicesBase("http://137.184.87.110:80/");
        }

        public Task<UserApiRoltec> GetCurrentUser()
        {
            //to do: obtener los valores desde al app settings
            var userRoltec = new UserApiRoltec
            {
                email = "admin1@roltec.mx",
                password = "M2Gt_ky7NL+?p"
            };


            
            return Task.FromResult(userRoltec);
        }

        /// <summary>
        /// get token from roltec.mx
        /// </summary>
        /// <param name="userApiRoltec"></param>
        /// <returns></returns>
        public async Task<string> GetTokenAsync()
        {
            var userApiRoltec = await GetCurrentUser();
            try
            {
                var token = await _dataServices.PostAsJsonAsyncItem("api/sessions/data", userApiRoltec);
                var contentResponse = await token.Content.ReadAsStringAsync();
                UserRoltecResponse newToken = await Task.Run(() => JsonConvert.DeserializeObject<UserRoltecResponse>(contentResponse));

                return newToken.accessToken;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        
    }
}
