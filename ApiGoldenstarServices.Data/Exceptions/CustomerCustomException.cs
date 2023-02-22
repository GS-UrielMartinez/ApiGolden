using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiGoldenstarServices.Data.Exceptions
{
    public class CustomerCustomException : Exception
    {
        public CustomerCustomException(Exception ex) : base () { }

        public CustomerCustomException(string message) : base(message) { }

        public CustomerCustomException(string message, params object[] args) : base(String.Format(CultureInfo.CurrentCulture,message, args))
        { 
        }

    }
}
