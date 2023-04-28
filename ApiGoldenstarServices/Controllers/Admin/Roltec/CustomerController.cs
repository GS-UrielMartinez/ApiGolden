using ApiGoldenstarServices.Data.DataAccess.Roltec;
using ApiGoldenstarServices.HttpServices.ExternalServices.Roltec;
using ApiGoldenstarServices.Models.RoltecApi;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace ApiGoldenstarServices.Controllers.Admin.Roltec
{
    //add roles for this endpoint
    [Authorize(Roles = "AdminGolden", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/v1/Admin/roltec/[controller]")]
    //[ApiController]
    //[Produces("application/json")]
    public class CustomerController : Controller
    {
        private readonly IRoltecApi _roltecApi;
        private readonly ICustomer _DACustomer;

        public CustomerController(IRoltecApi roltecApi, ICustomer dACustomer)
        {
            _roltecApi = roltecApi;
            _DACustomer = dACustomer;
        }

        [HttpPost]
        [Route("UpdatetoToWeb/{IdCustomer}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddCustomer(string IdCustomer)
        {
            // get customer from DB
            var customer = await _DACustomer.GetCustomerById(IdCustomer);
            
            //redifinir la clase completa depues de arreglar la query
            if (customer == null) return NotFound();

            var customerRoltec = new CustomerRoltec();
            customerRoltec.id = customer.IdCustomer;
            customerRoltec.name = customer.ShoppingName;

            try
            {   
                //Request to WEB
               CustomerRoltec newcustomer = await _roltecApi.AddCustomerToWeb(customerRoltec);

                return StatusCode(200, newcustomer);

            }catch (Exception  ex){

            }
            return Ok();
        }
    }
}
