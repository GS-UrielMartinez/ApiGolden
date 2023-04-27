using ApiGoldenstarServices.Models.Goldenstar;
using Dapper;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiGoldenstarServices.Data.DataAccess.Admin.Roltec
{
    public class DACustomer
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

        /// <summary>
        /// Ubtiene toda la estructura y datos del cliente para actualizarlo en la web de Roltec.mx
        /// </summary>
        /// <param name="idCustomer"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<Customer> GetCustomerById(string idCustomer)
        {
            //ToDo: agregar todas las propiedades del cliente
            string queryString = $@"select 
                                    cli_clave as CustomerKey,
                                    cli_cvematriz as ParentCustomerKey,
                                    cli_agente as AgentKey 




                                    from inctclie (nolock) 
                                    where cli_claveExterna = '{idCustomer}'";

            SqlConnection sqlConnection = DbConnection();
            sqlConnection.Open();

            Customer? customer = default;
            try
            {

                customer = await sqlConnection.QueryFirstOrDefaultAsync<Customer>(queryString);


                return customer;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                sqlConnection.Close();
            }
        }
    }
}
