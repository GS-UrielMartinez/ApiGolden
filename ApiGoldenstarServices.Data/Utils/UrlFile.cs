using Microsoft.IdentityModel.Protocols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using Microsoft.Extensions.Configuration;

namespace ApiGoldenstarServices.Data.Utils
{
    public class UrlFile
    {
        private readonly IConfiguration _configuration;
        public UrlFile( IConfiguration configuration)
        {
           
            _configuration = configuration;

        }

        public static string EncodeUrl(string fileName)
        {
            RijndaelManaged rijndael = new RijndaelManaged();
            MD5CryptoServiceProvider mD5 = new MD5CryptoServiceProvider();
            
            var hash = mD5.ComputeHash(Encoding.ASCII.GetBytes("sdfg"));

            rijndael.Key = hash;
            rijndael.Mode = CipherMode.ECB;

            var cryptoTransform = rijndael.CreateEncryptor();
            var buffer = Encoding.ASCII.GetBytes(fileName);
            var bytes = cryptoTransform.TransformFinalBlock(buffer, 0, buffer.Length);
            
            var urlName = string.Empty;
            foreach (var item in bytes)
            {
                urlName += item.ToString("D3");
            }


            return urlName;

        }

        public static string DecodeUrl(string fileName)
        {
            var urlName = string.Empty;

            int numeroBytes = fileName.Length / 3;
            byte[] buffer = new byte[numeroBytes];
            for (int i = 0; i < numeroBytes; i++)
            {
                buffer[i] = byte.Parse(fileName.Substring(i * 3, 3));
            }

            RijndaelManaged rijndael = new RijndaelManaged();
            MD5CryptoServiceProvider mD5 = new MD5CryptoServiceProvider();

            var hash = mD5.ComputeHash(Encoding.ASCII.GetBytes("sdf"));

            rijndael.Key = hash;
            rijndael.Mode = CipherMode.ECB;

            var cryptoTransform = rijndael.CreateDecryptor();

            var nombre = Encoding.ASCII.GetString(cryptoTransform.TransformFinalBlock(buffer, 0, buffer.Length));

            return nombre;

            return urlName;

        }


    }
}
