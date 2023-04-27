using ApiGoldenstarServices.Models.Roltec;
using ApiGoldenstarServices.Models.RoltecApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiGoldenstarServices.HttpServices.Utils
{
    public interface ITokenServices
    {
       Task<UserApiRoltec> GetCurrentUser();
       Task<string> GetTokenAsync();
    }
}
