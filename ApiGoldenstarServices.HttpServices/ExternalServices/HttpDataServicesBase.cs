using ApiGoldenstarServices.Models;
using ApiGoldenstarServices.Models.Roltec;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ApiGoldenstarServices.HttpServices.ExternalServices
{
    public class HttpDataServicesBase 
    {
        private HttpClient client;

        public HttpDataServicesBase(string defaultBaseUrl)
        {
            client = new HttpClient();

            if (!string.IsNullOrEmpty(defaultBaseUrl))
            {
                client.BaseAddress = new Uri(defaultBaseUrl);
                //client.DefaultRequestHeaders.Accept.Clear();
            }
        }


        public async Task<T> GetAsync<T>(string uri, string accessToken = null, bool forceRefresh = false)
        {
            T result = default;

            // The responseCache is a simple store of past responses to avoid unnecessary requests for the same resource.
            // Feel free to remove it or extend this request logic as appropraite for your app.
                AddAuthorizationHeader(accessToken);

                var json = await client.GetStringAsync(uri);

                result = await Task.Run(() => JsonConvert.DeserializeObject<T>(json));
            
            return result;
        }


        public async Task<T> PostAsJsonAsync<T>(string uri, T item,string token)
        {
           
            AddAuthorizationHeader(token);
            var serializedItem = JsonConvert.SerializeObject(item);

            var response = await client.PostAsync(uri, new StringContent(serializedItem, Encoding.UTF8, "application/json"));
            var contentResponse = await response.Content.ReadAsStringAsync();
            T newObject = await Task.Run(() => JsonConvert.DeserializeObject<T>(contentResponse));

            return newObject;
        }

        //to do: refactorizar esta funcion para que sea generica, por ahora se usa para obtener el token de la api de roltec
        public async Task<HttpResponseMessage> PostAsJsonAsyncItem(string uri,UserApiRoltec item)
        {
            
            HttpResponseMessage response = default;
            var jsonObject = JsonConvert.SerializeObject(item);
            var content = new StringContent(jsonObject.ToString(), Encoding.UTF8, "application/json");
            try
            {

                response = await client.PostAsync(uri, content);
                response.EnsureSuccessStatusCode();

                return response;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return response;
        }

        //Agrega el token al header de la peticion
        // to do: guardar el token en memoria para no hacer muchas peticiones
        private void AddAuthorizationHeader(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = null;
                return;
            }

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

    }
}
