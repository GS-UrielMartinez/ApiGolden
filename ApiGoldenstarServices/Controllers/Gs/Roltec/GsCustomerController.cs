using ApiGoldenstarServices.Data.DataAccess;
using ApiGoldenstarServices.HttpServices.ExternalServices.Roltec;
using ApiGoldenstarServices.Models.Roltec;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace ApiGoldenstarServices.Controllers.Gs.Roltec
{
    //[Authorize(Roles = "Goldenstar", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Authorize(Roles = "Roltec", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/v1/Gs/Roltec/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class GsCustomerController : Controller
    {
        private readonly IRoltecApi _roltecAPI;
        public GsCustomerController(IRoltecApi RoltecAPI)
        {
            _roltecAPI = RoltecAPI;
        }

        [HttpPost]
        [Route("Create")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateCustomer([FromBody] CustomerRoltec customer)
        {
            //make interfaces for hhttpservices
            var newCustomer = await _roltecAPI.AddCustomerToWeb(customer);


            return StatusCode(201, new { message = "Cliente Creado correctamente en la pagina", newCustomer });

        }
    }
}
