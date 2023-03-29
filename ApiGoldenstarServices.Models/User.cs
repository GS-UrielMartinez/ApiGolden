using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiGoldenstarServices.Models
{
    public class User
    {
        public string Name { get; set; }
        
        public string Email { get; set; }

        public string Password { get; set; }

        public string Rol { get; set; }

    }

    //Estructura para el inicio de sesion par ala api principal
    public class UserApi
    {
        public string Email { get; set; }

        public string Password { get; set; }
    }

    // Estructura para la api de roltec
    public class UserApiRoltec
    {
        public string email { get; set; }

        public string password { get; set; }
    }

    public class TokenResponse
    {
        public string User { get; set; }

        public string Rol { get; set; }

        public string Token { get; set; }

        public string RefreshToken { get; set; }
    }
}
