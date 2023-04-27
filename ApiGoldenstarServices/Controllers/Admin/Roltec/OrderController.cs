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
        public OrderController()
        {

        }

        [HttpPost]
        [Route("status/{orderid}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateOrder(string orderId)
        {
            return Ok();
        }
    }
}
