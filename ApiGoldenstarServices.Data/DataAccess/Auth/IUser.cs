using ApiGoldenstarServices.Models.Goldenstar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiGoldenstarServices.Data.DataAccess.Auth
{
    public interface IUser
    {
        Task<User> ReadByEmail(string email);

    }
}
