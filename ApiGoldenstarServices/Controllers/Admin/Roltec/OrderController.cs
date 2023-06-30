
using ApiGoldenstarServices.Data.DataAccess.Goldenstar;
using ApiGoldenstarServices.Data.DataAccess.Roltec ;

using ApiGoldenstarServices.Models.Goldenstar;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace ApiGoldenstarServices.Controllers.Admin.Roltec
{
    [Authorize(Roles = "Roltec", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]//admin
    [Route("api/v1/Admin/roltec/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class OrderController : Controller
    {
        private readonly DAOrder _DAOrder; //private readonly IOrder _DAOrder;

        public OrderController(DAOrder dAOrder) //public OrderController(IOrder dAOrder) 
        {
            this._DAOrder = dAOrder; //this._DAOrder = dAOrder;
        }
        //public OrderController() {}

        //[HttpGet]
        //[Route("Get/{idOrder}")]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //public async Task<IActionResult> ([FromBody] string idOrder)  
        //public async Task<IActionResult> GetOrder(string idOrder) { }  

        [HttpPost]
        [Route("status/{orderid}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateOrder(string orderId)
        {
            return Ok();
        }

        //
    }
}
