using ApiGoldenstarServices.Data.DataAccess.Goldenstar;
using ApiGoldenstarServices.Models.Goldenstar;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace ApiGoldenstarServices.Controllers.Roltec
{
    [Authorize(Roles = "Roltec", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/v1/roltec/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class OrderController : Controller
    {
        private readonly DAOrder _DAOrder;
        //private readonly IOrder _DAOrder; 
        //private readonly DAOrder _DAOrder; 

        public OrderController(DAOrder dAOrder)
        //public OrderController(IOrder dAOrder) 
        //public OrderController(DAOrder dAOrder) 
        {
            this._DAOrder = dAOrder; 
        }

        #region " Ejemplos de Base: Get( SELECT ), Post( INSERT ), Put( UPDATE ) "

        [HttpGet]
        [Route("Header/Get/{idOrder}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status302Found)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetOrderHeader(string idOrder) //([FromBody] string idOrder)  
        {
            Order order = null;
            try
            {
                if (idOrder == null) idOrder = "";
                if (idOrder.Trim() == "") throw new Exception("NO proporcionó ningún idOrder");

                order =
                    await this._DAOrder.GetOrderByIdAsync(idOrder);
                if (order == null)
                    throw new Exception("La Orden con idOrder " + idOrder.Trim() + " NO existe.");

                return
                    this.StatusCode((int)StatusCodes.Status200OK, order);
            }
            catch (Exception ex)
            {
                return this.BadRequest("NO fue posible obtener los datos de la Orden: " + ex.Message);
            }
        }

        [HttpPost]  //INSERT, 
        [Route("Header/Create")]
        [ProducesResponseType(StatusCodes.Status201Created)]  //INSERT, [HttpPost]
        //[HttpPut] //UPDATE, 
        //[Route("Header/Update/{idOrder}")]  
        //[ProducesResponseType(StatusCodes.Status200OK)]  //UPDATE, [HttpPut]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        private 
            async Task<IActionResult> CreateOrderHeader([FromBody] Order order)  //INSERT, [HttpPost] 
        //public async Task<IActionResult> UpdateOrderHeader([FromBody] Order order, string idOrder)  //UPDATE, [HttpPut]
        {
            if (order == null)
                //return BadRequest("El Servicio Web NO recibió ningún dato para crear la Orden.");  //INSERT, [HttpPost]
                //return BadRequest("El Servicio Web NO recibió ningún dato de la Orden para ser Actualizado.");  //UPDATE, [HttpPut]
                return BadRequest("El Servicio Web NO recibió ningún dato para crear la Orden.");
            try
            {
                //Order orderCreated =  //INSERT, [HttpPost]
                //Order orderUpdated =  //UPDATE, [HttpPut]
                Order orderCreated =
                    //await _DAOrder.InsertOrderHeaderAsync(order);  //INSERT, [HttpPost]
                    //await _DAOrder.UpdateOrderHeaderAsync(order);  //UPDATE, [HttpPut]
                    await _DAOrder.InsertOrderHeaderAsync(order);

                return
                    //this.StatusCode((int)StatusCodes.Status201Created, orderCreated);  //INSERT, [HttpPost]
                    //this.StatusCode((int)StatusCodes.Status200OK, orderUpdated);  //UPDATE, [HttpPut]
                    this.StatusCode((int)StatusCodes.Status201Created, orderCreated);
            }
            catch (Exception ex)
            {
                //return this.BadRequest("NO fue posible Insertar el registro: " + ex.Message);  //INSERT, [HttpPost]
                //return this.BadRequest("NO fue posible Actualizar el registro: " + ex.Message);  //UPDATE, [HttpPut]
                return this.BadRequest("NO fue posible Insertar el registro: " + ex.Message);
            }
        }

        [HttpPut] //UPDATE, 
        [Route("Header/Update/{idOrder}")]
        [ProducesResponseType(StatusCodes.Status200OK)]  //UPDATE, [HttpPut]
        //[HttpPost]  //INSERT, 
        //[Route("Header/Create")]
        //[ProducesResponseType(StatusCodes.Status201Created)]  //INSERT, [HttpPost]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        private
            async Task<IActionResult> UpdateOrderHeader([FromBody] Order order, string idOrder)  //UPDATE, [HttpPut] 
        {
            if (order == null)
                return BadRequest("El Servicio Web NO recibió ningún dato de la Orden para ser Actualizado.");  //UPDATE, [HttpPut]
            try
            {
                if (idOrder == null) idOrder = "";
                if (idOrder.Trim() == "") throw new Exception("NO proporcionó ningún idOrder");
                if (idOrder.Trim() != order.IdOrder.Trim())
                    throw new Exception("El idOrder especificado NO coincide con el de los datos recibidos.");


                Order orderBeforeUpdate =
                    await this._DAOrder.GetOrderByIdAsync(idOrder);
                if (orderBeforeUpdate == null)
                    throw new Exception("La Orden con idOrder " + idOrder.Trim() + " NO existe.");


                Order orderUpdated =  //UPDATE, [HttpPut]
                    await _DAOrder.UpdateOrderHeaderAsync(order);  //UPDATE, [HttpPut]

                return
                    this.StatusCode((int)StatusCodes.Status200OK, orderUpdated);  //UPDATE, [HttpPut]
            }
            catch (Exception ex)
            {
                return this.BadRequest("NO fue posible Actualizar el registro: " + ex.Message);  //UPDATE, [HttpPut]
            }
        }

        [HttpPut] //UPDATE, 
        [Route("Header/EditList/{customerId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]  //UPDATE, [HttpPut]
        //[HttpPost]  //INSERT, 
        //[Route("Header/Create")]
        //[ProducesResponseType(StatusCodes.Status201Created)]  //INSERT, [HttpPost]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        private
            async Task<IActionResult> EditOrderHeadersByCustomerId(string customerId)  //UPDATE, [HttpPut] 
        // Ejemplo para probar Manejo de Transacción
        {
            try
            {
                if (customerId == null) customerId = "";
                if (customerId.Trim() == "") throw new Exception("NO proporcionó ningún customerId");


                List<Order> listOrdersEdited = 
                    await _DAOrder.EditOrderListByCustomerIdAsync(customerId); 

                return
                    this.StatusCode((int)StatusCodes.Status200OK, listOrdersEdited);  //UPDATE, [HttpPut]
            }
            catch (Exception ex)
            {
                return this.BadRequest("NO fue posible Actualizar el registro: " + ex.Message);  //UPDATE, [HttpPut]
            }
        }

        #endregion 


        /// <summary>
        /// Crear una nueva orden en el sistema interno de goldenstar
        /// </summary>
        /// <param name="order"></param>
        /// <returns>
        ///      {
        ///         "cve_orden": "08055",
        ///         "Status": "OK",
        ///         "Message": "Orden creada correctamente."
        ///      }
        /// </returns>
        /// <remarks>
        /// </remarks>
        [HttpPost]
        [Route("Create")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateOrder([FromBody] Order order)
        {

            if (order == null) return BadRequest("No se pasaron todos los elementos");
            try
            {
                var newOrder = await _DAOrder.AddOrder(order);
            
                return StatusCode(201, newOrder);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
                
        }
    }
}
