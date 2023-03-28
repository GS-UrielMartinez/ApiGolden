using ApiGoldenstarServices.Data.DataAccess;
using ApiGoldenstarServices.HttpServices.ExternalServices.Roltec;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace ApiGoldenstarServices.Controllers.Admin.Roltec
{
    //[Authorize(Roles = "Roltec", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/v1/Admin/roltec/[controller]")]
    //[ApiController]
    //[Produces("application/json")]
    public class CustomerController : Controller
    {
        private readonly IRoltecApi _roltecApi;

        public CustomerController(IRoltecApi roltecApi)
        {
            _roltecApi = roltecApi;
        }

        [HttpPost]
        [Route("Create")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddCustomer()
        {
            await _roltecApi.AddCustomerToWeb();
            return Ok();
        }
    }
}
