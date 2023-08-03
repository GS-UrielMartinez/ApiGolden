using ApiGoldenstarServices.Controllers.Auth;

//using ApiGoldenstarServices.Data.DataAccess.Admin.Roltec; // BORRARLO CADA VEZ QUE SE AGREGUE AUTOMÁTICAMENTE
using ApiGoldenstarServices.Data.DataAccess.Roltec;

using ApiGoldenstarServices.Models.Goldenstar;
using ApiGoldenstarServices.Models.Goldenstar.BaseModels;
using Azure;
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
    public class CustomerController : Controller
    {
        //private readonly ILogger<CustomerController> _logger;

        private readonly DACustomer _DACustomer;
        //private readonly ICustomer _DACustomer; 
        //private readonly DACustomer _DACustomer; 

        public CustomerController(DACustomer dACustomer)
        //public CustomerController(ICustomer dACustomer) 
        //public CustomerController(DACustomer dACustomer) 
        {
            this._DACustomer = dACustomer;
        }

        #region " Servicios para Pruebas ( Ocultarlos ó Borrarlos ) " 

        [HttpGet]
        [Route("Test/Get/{idCustomer}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public
            async Task<IActionResult> GetCustomer(string idCustomer) //([FromBody] string idCustomer)  
        {
            Customer? customer = null;
            try
            {
                if (idCustomer == null) idCustomer = "";
                if (idCustomer.Trim() == "") throw new Exception("NO proporcionó ningún idCustomer.");

                customer =
                    await this._DACustomer.GetCustomerByIdAsync(idCustomer);
                if (customer == null)
                    throw new Exception("El Cliente con idCustomer " + idCustomer.Trim() + " NO existe.");

                return
                    this.StatusCode((int)StatusCodes.Status200OK, customer);
            }
            catch (Exception ex)
            {
                return this.BadRequest("NO fue posible obtener los datos del Cliente: " + ex.Message);
            }
        }

        #endregion

        #region " ShippingAddress " 

        [HttpGet]
        [Route("ShippingAddress/Get/{shippingAddressId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetShippingAddress(string shippingAddressId) 
        {
            try
            {
                ShippingAddress? shippingAddress =
                    await this._DACustomer.GetShippingAddressAsync(shippingAddressId);

                return
                    this.StatusCode((int)StatusCodes.Status200OK, shippingAddress);
            }
            catch (Exception ex)
            {
                return this.BadRequest("NO fue posible obtener los datos de la Dirección de Envío: " + ex.Message);
            }
        }

        [HttpGet]
        [Route("ShippingAddress/GetList/{customerKey}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetShippingAddressList(string customerKey)
        {
            try
            {
                List<ShippingAddress> shippingAddressList =
                    await this._DACustomer.GetShippingAddressListAsync(customerKey); 

                return
                    this.StatusCode((int)StatusCodes.Status200OK, shippingAddressList);
            }
            catch (Exception ex)
            {
                return this.BadRequest("NO fue posible obtener la lista de Direcciones de Envío: " + ex.Message);
            }
        }

        [HttpPost]
        [Route("ShippingAddress/Create")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateShippingAddress([FromBody] ShippingAddress shippingAddress) 
        {
            try
            {
                ShippingAddress? newShippingAddress =
                    await this._DACustomer.AddShippingAddressAsync(shippingAddress);

                return
                    this.StatusCode((int)StatusCodes.Status201Created, newShippingAddress);
            }
            catch (Exception ex)
            {
                return this.BadRequest("NO fue posible Agregar el registro de Dirección de Envío: " + ex.Message);
            }
        }

        [HttpPut]
        [Route("ShippingAddress/Update")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateShippingAddress([FromBody] ShippingAddress shippingAddress) 
        {
            try
            {
                ShippingAddress? updatedShippingAddress =
                    await this._DACustomer.UpdateShippingAddressAsync(shippingAddress);

                return
                    this.StatusCode((int)StatusCodes.Status200OK, updatedShippingAddress);
            }
            catch (Exception ex)
            {
                return this.BadRequest("NO fue posible Actualizar los datos de Dirección de Envío: " + ex.Message);
            }
        }

        #endregion 

        [HttpPost]
        [Route("Create")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateCustomer([FromBody] Customer customer) 
        {
            try
            {
                //await _DACustomer.AddCustomer(customer);
                CustomerResponse? newCustomer =
                    await this._DACustomer._AddCustomerAsync(customer);

                return
                    this.StatusCode((int)StatusCodes.Status201Created, newCustomer); 
            }
            catch (Exception ex)
            {
                return this.BadRequest("NO fue posible Agregar el registro del Cliente: " + ex.Message);
            }
        }


        [HttpPut]
        [Route("Update")]
        //[Route("Update/{idCustomer}")]
        //[Route("Update/{CustomerKey}")] 
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateCustomer([FromBody] Customer customer)
        //public async Task<IActionResult> UpdateCustomer([FromBody] Customer customer, string idCustomer) 
        //public async Task<IActionResult> UpdateCustomer([FromBody] Customer customer, string CustomerKey) 
        {
            try
            {
                //await _DACustomer.UpdateCustomer(customer);
                Customer updatedCustomer =
                    await this._DACustomer._UpdateCustomerAsync(customer);

                return
                    this.StatusCode((int)StatusCodes.Status200OK, updatedCustomer);
            }
            catch (Exception ex)
            {
                return this.BadRequest("NO fue posible Actualizar los datos del Cliente: " + ex.Message);
            }
        }


        [HttpPost]
        [Route("BillingAddress/Create")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateBillingAddress([FromBody] Customer billingCustomer) 
        {
            try
            {
                //await _DACustomer.AddCustomer(billingCustomer);
                CustomerResponse? newBillingCustomer = 
                    await this._DACustomer._AddBillingAddressAsync(billingCustomer);

                return
                    //StatusCode((int)StatusCodes.Status201Created, _message); 
                    //StatusCode((int)StatusCodes.Status201Created, newBillingCustomer); 
                    this.StatusCode((int)StatusCodes.Status201Created, newBillingCustomer); 
            }
            catch (Exception ex)
            {
                return 
                    this.BadRequest(
                        "NO fue posible Agregar el registro de Dirección de Facturación del Cliente: " 
                        + ex.Message
                    );
            }
        }


        [HttpPut]
        [Route("BillingAddress/Update")]
        //[Route("BillingAddress/Update/{IdBillingAddress}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateBillingAddress([FromBody] Customer billingCustomer)
        //public async Task<IActionResult> UpdateBillingAddress([FromBody] Customer billingCustomer, string IdBillingAddress) 
        {
            try
            {
                //await _DACustomer.UpdateCustomer(billingCustomer);
                Customer updatedBillingCustomer =
                    await this._DACustomer._UpdateBillingAddressAsync(billingCustomer); 

                return
                    //StatusCode(200, _message); 
                    this.StatusCode((int)StatusCodes.Status200OK, updatedBillingCustomer);
            }
            catch (Exception ex)
            {
                return this.BadRequest("NO fue posible Actualizar los datos de Facturación del Cliente: " + ex.Message);
            }

        }


        //
    }
}
