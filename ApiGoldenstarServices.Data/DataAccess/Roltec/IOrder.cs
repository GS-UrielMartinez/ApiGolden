using ApiGoldenstarServices.Models.Goldenstar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiGoldenstarServices.Data.DataAccess.Goldenstar
{
    public interface IOrder
    {
        //main functions
        Task<OrderResponse> AddOrder(Order order);

        //other functions
        Task<bool> ValidateCustomer(string customerId);

        /// <summary>
        /// Obtiene el folio consecutivo de cada orden
        /// </summary>
        /// <returns></returns>
        Task<string> GenerateFolio();

        /// <summary>
        /// add header
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        Task<bool> AddHeaderOrder(Order order);
        
        /// <summary>
        /// AddOrder orederdetail
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        Task<bool> AddOrderDetail(Order order);

        
        /// <summary>
        /// Agregar direccion de envio
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        Task<bool> AddShippingAddress(Order order);
        
        /// <summary>
        /// Agregar encabezado 1 - agrega la orden a 
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        Task<bool> AddOrderToShippingTableHeader(Order order);
        
        /// <summary>
        /// agregar detallado 1
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        Task<bool> AddOrderToShippingTableDetail(Order order);
        
        /// <summary>
        /// agregar rollo consmo
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="customerId"></param>
        /// <param name="folio"></param>
        /// <returns></returns>
        Task<bool> AddRollUsage(string orderId, string customerId, string folio);
        
        /// <summary>
        /// agregar liberacion del pedido
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="folio"></param>
        /// <returns></returns>
        Task<bool> AddOrderRelease(string orderId, string folio);

    }
}
