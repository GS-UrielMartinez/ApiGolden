using ApiGoldenstarServices.Data.Exceptions;
using ApiGoldenstarServices.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiGoldenstarServices.Data.DataAccess
{
    public class DACustomer : ICustomer
    {
        private SqlConfiguration _conectionString;
        

        public DACustomer(SqlConfiguration connectionString)
        {
            _conectionString = connectionString;
        }

        // Get connection to Database
        public SqlConnection DbConnection()
        {
             
            return new SqlConnection(_conectionString.ConnetionString);
        }

        public async Task<Customer> AddCustomer(Customer customer)
        {
            //To do: validate user
            try
            {
              await ValidateDuplicatedCustomer(customer);
            }
            catch (Exception ex)
            {
                throw new CustomerCustomException(ex.Message);
            }
            //if(isCustomerDuplicated == true)
            //{
            //    
                
            //}
            var db = DbConnection();
            await db.OpenAsync();
            // Add paremeters to StoreProcedure
            DynamicParameters parameters = new DynamicParameters();


            //var newCustomer = db.QuerySingleOrDefault<Customer>("GetCustomerById", parameters, commandType: CommandType.StoredProcedure);

            var newCustomer = await db.QuerySingleOrDefaultAsync<Customer>("Magento_Cliente_Agregar", parameters, commandType: CommandType.StoredProcedure);


            // To do: return zona,agente cli_clave

            throw new NotImplementedException();
        }

        public Task<Customer> GetCustomerById(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Customer>> GetCustomersList()
        {
            var db = DbConnection();
            await db.OpenAsync();
            // se utiliza el as para poder mapear el resultado a la definicion de la clase
             var strQuery = @"select top 20
           
                cli_clave as cve_cliente
                ,cli_email
                ,cli_rfc as rfc
                ,cli_nombre
                ,cli_compra as nombreCompra
                ,cli_DenominacionSocial as denominacionSocial
                ,CveRegimenFiscal as regimenFiscal
                ,cli_nom as NombreCliente
                ,cli_apat as apellidoPaterno
                ,cli_amat as apellidoMaterno
                from inctclie 
			    where cli_claveExterna <> ''";

            var CustomerList = await db.QueryAsync<Customer>(strQuery, new {});

            await db.CloseAsync();

           

            return CustomerList;
            
        }

        public Task<Customer> UpdateCustomer(Customer customer)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Validar si el cliente ya existe en la base de datos con el RFC
        /// </summary>
        /// <param name="customer"></param>
        /// <returns>Primer cliente con el rfc dado</returns>
        /// <exception cref="Exception"></exception>
        public async Task ValidateDuplicatedCustomer(Customer customer)
        {
            

            if (customer == null) throw new CustomerCustomException("Cliente vacio");

            var dbconnection = DbConnection();
            await dbconnection.OpenAsync();
            string consulta = @$"SELECT top 1 CLI_RFC FROM INCTCLIE (nolock) WHERE rtrim(ltrim(CLI_RFC))='{customer.rfc.Trim()}'";
            try
            {

                var CustomerExist = await dbconnection.QuerySingleOrDefaultAsync<Customer>(consulta);
               if (CustomerExist.rfc.Trim() != "XAXX010101000")
                {
                    //make custom exceptions
                    throw new CustomerCustomException("El RFC del cliente ya esta registrado");
                    
                }
                
            }
            catch (Exception ex)
            {
                
                //logger
                throw new Exception(ex.ToString());
            }
            finally
            {
                dbconnection.Close();
            }

        }

        public Task<ShippingAddress> AddShippingAddressToCustomer(ShippingAddress shippingAddress)
        {
            throw new NotImplementedException();
        }

        public Task<BillingAddress> AddBillingAddressToCustomer(BillingAddress billingAddress)
        {
            throw new NotImplementedException();
        }
    }
}
