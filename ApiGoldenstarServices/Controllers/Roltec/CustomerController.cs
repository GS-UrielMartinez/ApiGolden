using ApiGoldenstarServices.Controllers.Auth;
using ApiGoldenstarServices.Data.DataAccess.Roltec;
using ApiGoldenstarServices.Models.Goldenstar;
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
        private readonly ICustomer _DACustomer;

        public CustomerController(ICustomer dACustomer)
        {
            _DACustomer = dACustomer;



        }



        /// <summary>
        /// Create a customer
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST api/v1/roltec/customer/create
        ///     {
        ///        "idCliente": "12343",
        ///        "cli_cvematriz": "12343",
        ///        "cve_cliente": "",
        ///        "nombreCompra": "Publico en general",
        ///        "apellidoCompra": "pG",
        ///        "cli_email": "xpartida@roltec.mx",
        ///        "Cli_ComprasCel": "6655332211",
        ///        "CodigoEstado": "25",
        ///        "giro_cve": "03",
        ///        "cli_medio": "1",
        ///        "claveAgente": "",
        ///        "credito": "0", 
        ///        "credito_dias": "0", 
        ///        "billing_address": [
        ///            {
        ///                "IdBillingAddress": "1",
        ///                "NombreCliente": "Publico en general",
        ///                "apellidoPaterno": "Partida",
        ///                "apellidoMaterno": "Arenas",
        ///                "cli_nombre": "Prueba",
        ///                "rfc": "XAXX010101000",
        ///                "denominacionSocial": "PRUEBA Publico",
        ///                "regimenFiscal": "601",
        ///                "cve_forma_pago": "03",
        ///                "cve_metodo_pago": "PUE",
        ///                "dias_credito": 0,
        ///                "cve_uso_cfdi": "G03",
        ///                "codigo_postal": "83600",
        ///                "ciudad": "CABORCA, SONORA.",
        ///                "cve_ciudad": "string",
        ///                "estado": "Sinaloa",
        ///                "calle": "JOSE CLEMENTE VANEGAS OESTE 171",
        ///                "colonia": "Col centro"
        ///            }
        ///        ],
        ///        "shipping_address": [
        ///            {
        ///                "cve_sucursal": "",
        ///                "sucursal": "",
        ///                "calle": "",
        ///                "colonia": "",
        ///                "ciudad": "",
        ///                "codigo_postal": "",
        ///                "cve_ciudad": "",
        ///                "telefono": ""
        ///            }
        ///        ]
        ///    }
        ///
        /// </remarks>
        /// <response code="201">Returns the newly created user</response>
        /// <response code="400">If the item is null</response>
        [HttpPost]
        [Route("Create")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateCustomer([FromBody] Customer customer)
        {
            var newCustomerResponse = new
            {
              Message = "Datos incompletos"

            };
            var _Message = "";
            if (customer == null) return BadRequest(newCustomerResponse);

            //To Do :validate if Customer Exist

            try
            {
                await _DACustomer.AddCustomer(customer);

                _Message = "Cliente Creado Correctamente";
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }

            //get new customer
            var newCustomer = await _DACustomer.GetCustomerResponseById(customer.IdCustomer);

            newCustomer.Message= _Message;
            
            return StatusCode(201, newCustomer);

        }

        //update
        [HttpPut]
        [Route("Update/{CustomerKey}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateCustomer([FromBody] Customer customer, string CustomerKey)
        {
            var newCustomerResponse = new
            {
                Message = "Datos incompletos"

            };
            var _Message = "";
            if (customer == null) return BadRequest(newCustomerResponse);

            //To Do :validate if Customer Exist
            var customerExist =await _DACustomer.GetCustomerByCustumerKey(CustomerKey);
            if (customerExist == false)
            {
                var customerResponse = new
                {
                    Message = $@"El cliente con clave {CustomerKey} no existe"
                };
                return BadRequest(customerResponse);
            }
            try
            {
                await _DACustomer.UpdateCustomer(customer);

                _Message = "Cliente actualizado correctamente correctamente";
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }

            //get new customer
            var newCustomer = await _DACustomer.GetCustomerResponseById(customer.IdCustomer);

            newCustomer.Message = _Message;

            return StatusCode(201, newCustomer);

        }


        /// <summary>
        /// Agrega una nueva direccion de facturacion a los clientes
        /// </summary>
        /// <param name="billingAddress"></param>
        /// <remarks>
        ///     POST /roltec/customer/billingaddress/create
        ///     {
        ///     "idCliente": "12552",
        ///     "cli_cvematriz": "12552",
        ///     "cve_cliente": "",
        ///     "nombreCompra": "Prueba cliente fisico",
        ///     "apellidoCompra": "pG",
        ///     "cli_email": "xpartida@roltec.mx",
        ///     "Cli_ComprasCel": "6655332211",
        ///     "CodigoEstado": "25",
        ///     "giro_cve": "03",
        ///     "cli_medio": "1",
        ///     "claveAgente": "",
        ///     "credito": "0", 
        ///     "credito_dias": "0", 
        ///     "CreditAvilable": false,
        ///     "billing_address": [
        ///         {
        ///             "IdBillingAddress": "2",
        ///             "NombreCliente": "Publico en general",
        ///             "apellidoPaterno": "Hernandez",
        ///             "apellidoMaterno": "Lopez",
        ///             "cli_nombre": "laura",
        ///             "rfc": "LHE000107UW6",
        ///             "denominacionSocial": "laura lopez",
        ///             "regimenFiscal": "612",
        ///             "cve_forma_pago": "03",
        ///             "cve_metodo_pago": "PUE",
        ///             "dias_credito": "0",
        ///             "cve_uso_cfdi": "G03",
        ///             "codigo_postal": "80300",
        ///             "ciudad": "Culiacan, Sinaloa.",
        ///             "cve_ciudad": "",
        ///             "estado": "Sinaloa",
        ///             "calle": "JOSE CLEMENTE VANEGAS OESTE 171",
        ///             "colonia": "Col centro"
        ///         }
        ///     ],
        ///     "shipping_address": [
        ///         {
        ///             "cve_sucursal": "",
        ///             "sucursal": "",
        ///             "calle": "",
        ///             "colonia": "",
        ///             "ciudad": "",
        ///             "codigo_postal": "",
        ///             "cve_ciudad": "",
        ///             "telefono": ""
        ///         }
        ///     ]
        /// }    
        /// 
        /// 
        /// </remarks>
        /// <response code="201">Returns the newly created billing address</response>
        /// <response code="400">If the item is null</response>
        [HttpPost]
        [Route("BillingAddress/Create")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateBillingAddress([FromBody] Customer billingCustomer)
        {
            var response = new
            {
                message = "Datos incompletos o peticion vacia"
            };

            var _message = "";
            if (billingCustomer == null) return BadRequest(response);

            try
            {
                await _DACustomer.ValidateBillingCustomer(billingCustomer);

            }catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            try
            {
                // usa la misma funcion de agregar cliente
                await _DACustomer.AddCustomer(billingCustomer);

                _message = "SE creo una nueva direccion de facturacion";

                return StatusCode(201, _message);
            }
            catch (Exception ex)
            {
                
                return BadRequest(ex.Message);
            }

        }



        [HttpPut]
        [Route("BillingAddress/Update/{IdBillingAddress}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateBillingAddress([FromBody] Customer billingCustomer, string IdBillingAddress)
        {
            var response = new
            {
                message = "Datos incompletos o peticion vacia"
            };

            var _message = "";
            if (billingCustomer == null) return BadRequest(response);

            try
            {
                await _DACustomer.GetBillingCustomerById(IdBillingAddress);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            try
            {
                // usa la misma funcion de actualizar cliente
                await _DACustomer.UpdateCustomer(billingCustomer);

                _message = @$"SE Actualizo la direccion de facturacion con id {IdBillingAddress}";

                return StatusCode(200, _message);
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }

        }


    }
}
