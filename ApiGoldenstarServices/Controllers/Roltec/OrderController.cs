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
        private readonly IOrder _DAOrder;

        public OrderController(IOrder dAOrder)
        {
            _DAOrder = dAOrder;
        }

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
