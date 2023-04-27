using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using Newtonsoft.Json;
using ApiGoldenstarServices.Models.Goldenstar;
using ApiGoldenstarServices.Data.DataAccess.Auth;

namespace ApiGoldenstarServices.Controllers.Auth
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class LoginController : Controller
    {
        private readonly ILogger<LoginController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IUser _user;
        public LoginController(IConfiguration configuration, ILogger<LoginController> logger, IUser user)
        {
            _configuration = configuration;
            _logger = logger;
            _user = user;
        }
        //[Authorize(Roles = "Administrador,Operador,Supervisor", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        //[HttpGet]
        //[Route("myProfile")]
        //public async Task<IActionResult> GetProfile()
        //{
        //    var email = string.Empty;
        //    if (HttpContext.User.Identity is ClaimsIdentity identity)
        //    {
        //        email = identity.FindFirst(ClaimTypes.Email).Value;
        //    }
        //    var usuario = await _user.GetCurrentUser(email);
        //    return Ok(usuario);
        //}


        /// <summary>
        /// Login de los usuarios
        /// </summary>
        /// <param name="user"></param>
        /// <remarks>
        ///     {
        ///     "Email" : "Admin@goldenstar.com.mx",
        ///     "Password" : "1234"
        ///     
        ///     }
        /// </remarks>
        /// <returns>
        /// </returns>
        [HttpPost]
        [Route("authenticate")]
        public async Task<ActionResult<TokenResponse>> Token([FromBody] UserApi user)
        {
            _logger.LogInformation("Token Controller Post <> Peticion: " + JsonConvert.SerializeObject(user));

            var getUser = await _user.ReadByEmail(user.Email);

            if (getUser.Password.Trim() != user.Password)
            {
                _logger.LogInformation("Token Controller Post <> Respuesta: 404 - Usuario o contrasena no validos");
                return StatusCode(404, "Usuario o contrasena no validos");
            }

            if (getUser == null)
            {
                _logger.LogInformation("Token Controller Post <> Respuesta: 500 - usuario no valido.");
                return StatusCode(500, "No se ha logrado validar el usuario.");
            }

            string token = BuildToken(getUser.Name.Trim(), getUser.Email.Trim(), getUser.Rol.Trim());

            if (token == null)
            {
                _logger.LogError("Token Controller Post <> Respuesta: 500 - Ha ocurrido un error al generar el token");
                return StatusCode(500, "Ha ocurrido un error al generar el token");
            }

            var tokenUsuarioAPI = new TokenResponse() { Token = token.Trim(), RefreshToken = "", User = getUser.Name.Trim(), Rol = getUser.Rol.Trim() };

            _logger.LogInformation("Token Controller Post <> Respuesta: 200 - " + JsonConvert.SerializeObject(tokenUsuarioAPI));
            return StatusCode(200, tokenUsuarioAPI);
        }

        private string BuildToken(string nombre, string correo, string rol)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetValue<string>("SecretKey")));
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                    new Claim(ClaimTypes.Name, nombre),
                    new Claim(ClaimTypes.Email, correo),
                    new Claim(ClaimTypes.Role, rol)
                    }),
                    SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature),
                    IssuedAt = DateTime.UtcNow
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);
                return tokenHandler.WriteToken(token);
            }
            catch (Exception ex)
            {
                _logger.LogError("Token Controller Post <> " + ex.Message);
                return null;
            }
        }
    }
}
