using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiGoldenstarServices.Utils
{
    public class GetValuesFromEnvFile
    {
        public GetValuesFromEnvFile(List<string> valueEnv)
        {
            HashKey = valueEnv[0];
            UrlFile = valueEnv[1];
            APIRoltec = valueEnv[2];
            UserAPI = valueEnv[3];
            PasswordAPI = valueEnv[4];
        }

        public List<string> ValueString { get; set; }

        public string HashKey { get; set; }
        public string UrlFile { get; set; }
        public string APIRoltec { get; set; }
        public string UserAPI { get; set; }
        public string PasswordAPI { get; set; }
    }


}
