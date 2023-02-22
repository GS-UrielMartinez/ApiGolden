using ApiGoldenstarServices.Data.DataAccess;
using ApiGoldenstarServices.Models;
using Microsoft.AspNetCore.Mvc;

namespace ApiGoldenstarServices.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ClienteController : ControllerBase
    {
        private readonly ICustomer _customer;
        //initialize class
        public ClienteController(ICustomer customer)
        {
            _customer = customer;
        }

        /// <summary>
        /// Creates a TodoItem.
        /// </summary>
        /// <param name="item"></param>
        /// <returns>A newly created TodoItem</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     Get api/v1/Cliente
        ///     {
        ///    
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Returns the newly created item</response>
        /// <response code="400">If the item is null</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAllCustomers()
        {
            var customerList= await _customer.GetCustomersList();


            return Ok(customerList);

        }


        /// <summary>
        /// Crear un cliente.
        /// </summary>
        /// <param name="cve_clinte"></param>
        /// <returns>Se ha creado un nuevo cliente</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     Post api/v1/Cliente/create
        ///     {
        ///    
        ///     }
        ///
        /// </remarks>
        /// <response code="201">Reegresa un nuevo cliente creado</response>
        /// <response code="400">If the customer is null</response>
        [HttpPost]
        [Route("create")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateCustomer([FromBody] Customer customer)
        {
            if (customer == null) return BadRequest();
            if (ModelState.IsValid)
            {
                try
                {

                    var newCustomer = await _customer.AddCustomer(customer);
                    return Created("New customer",newCustomer);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

    }
}
