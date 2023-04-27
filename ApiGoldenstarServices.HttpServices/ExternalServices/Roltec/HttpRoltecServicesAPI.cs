using ApiGoldenstarServices.HttpServices.Utils;
using ApiGoldenstarServices.Models;
using ApiGoldenstarServices.Models.Goldenstar;
using ApiGoldenstarServices.Models.Roltec;
using ApiGoldenstarServices.Models.RoltecApi;
using ApiGoldenstarServices.Models.ServicesModels.RoltecApi;
using Newtonsoft.Json;
using CustomerRoltec = ApiGoldenstarServices.Models.RoltecApi.CustomerRoltec;

namespace ApiGoldenstarServices.HttpServices.ExternalServices.Roltec
{
    /// <summary>
    /// Se realizan todas las peticiones hacia el servicio de Roltec.mx
    /// </summary>
    public class HttpRoltecServicesAPI : IRoltecApi
    {
        private HttpDataServicesBase _dataServices;
        //get token
        private readonly ITokenServices _tokenServices;
        //Data access customer

        public HttpRoltecServicesAPI(ITokenServices tokenServices)
        {
            _dataServices = new HttpDataServicesBase("http://137.184.87.110:80/");
            _tokenServices = tokenServices;
        }



        /// <summary>
        /// Agregar Criente a la pagina roltec.mx , ya sea desde marka o salesforce
        /// </summary>
        /// <param name="customerRoltec"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<CustomerRoltec> AddCustomerToWeb(CustomerRoltec customerRoltec)
        {
            var token = await _tokenServices.GetTokenAsync();

            try
            {
                var newCustomer = await _dataServices.PostAsJsonAsync<CustomerRoltec>("api/users/data", customerRoltec, token);

                return newCustomer;
            }catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Actualizar el inventario de Productos de roltec
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task UpdateInventoryProduct(Product product)
        {
            var token = await _tokenServices.GetTokenAsync();

            try
            {
                //put
                var newProduct = await _dataServices.PutAsJsonAsync<Product>("api/products/data", product, token);
                //response 
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Actualizar el estatus de una orden hacia Roltec.mx
        /// </summary>
        /// <typeparam name="OrderRoltec"></typeparam>
        /// <param name="orderRoltec"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<OrderRoltec> UpdateOrderStatus<OrderRoltec>(OrderRoltec orderRoltec)
        {
            var token = await _tokenServices.GetTokenAsync();

            try
            {
                //orders
                OrderRoltec order = await _dataServices.PostAsJsonAsync<OrderRoltec>("api/orders/data", orderRoltec, token);

                return order;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Actualizacion de Codigos postales
        /// </summary>
        /// <param name="zipCode"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task UpdateZipCode(ZipCode zipCode)
        {
            var token = await _tokenServices.GetTokenAsync();

            try
            {
                var order = await _dataServices.PostAsJsonAsync<ZipCode>("api/zip_code/data", zipCode, token);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

       
    }
}
