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

    //api gs
    public class UserApi
    {
        public string Email { get; set; }

        public string Password { get; set; }
    }

    public class UserApi2
    {
        public string Username { get; set; }

        public string Password { get; set; }
    }

    public class TokenResponse
    {
        public string User { get; set; }

        public string Rol { get; set; }

        public string Token { get; set; }

        public string RefreshToken { get; set; }
    }
}
