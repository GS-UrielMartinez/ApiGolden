using ApiGoldenstarServices.Data.DataAccess.Admin.Roltec;
using ApiGoldenstarServices.HttpServices.ExternalServices.Roltec;
using ApiGoldenstarServices.Models.Goldenstar;
using ApiGoldenstarServices.Models.Goldenstar.BaseModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Data;

namespace ApiGoldenstarServices.Controllers.Admin.Roltec
{

    [Authorize(Roles = "Roltec", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/v1/roltec/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class InvoiceController : Controller
    {
        private readonly IRoltecApi _roltecApi;
        private readonly IInvoice _invoice;

        public InvoiceController(IInvoice invoice,IRoltecApi roltecApi)
        {
            _invoice = invoice;
            _roltecApi = roltecApi;
        }

        [HttpPut]
        [Route("Update/{folio}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateInvoicetoWeb(string folio)
        {
            try
            {
                var invoice = await _invoice.UpdateInvoiceToWebAsync(folio);

                var response = new CustomResponse { 
                    Message = "Factura actualizada Correctamente", 
                    Reponse = invoice
                };

                return StatusCode(200, response);

            }
            catch (Exception ex)
            {
                return StatusCode(400,ex);
            }
        }
    }
}
