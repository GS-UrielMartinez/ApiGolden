using ApiGoldenstarServices.Data.Exceptions;
using ApiGoldenstarServices.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Protocols;
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

        public async Task<bool> AddCustomer(Customer customer)
        {
           
            var db = DbConnection();
            await db.OpenAsync();
            // Add paremeters to StoreProcedure
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("@cve_cliente", customer.idCliente.ToUpper(), (DbType?)SqlDbType.VarChar);
            parameters.Add("@cli_cvematriz", customer.cli_cvematriz, (DbType?)SqlDbType.VarChar);
            parameters.Add("@billingAddressId", customer.billing_address[0].IdBillingAddress, (DbType?)SqlDbType.VarChar);
            parameters.Add("@nombreCompra", customer.nombreCompra.ToUpper(), (DbType?)SqlDbType.VarChar);
            parameters.Add("@apellidoCompra", customer.apellidoCompra.ToUpper(), (DbType?)SqlDbType.VarChar);
            parameters.Add("@cli_email", customer.cli_email, (DbType?)SqlDbType.VarChar);
            parameters.Add("@Cli_ComprasCel", customer.Cli_ComprasCel, (DbType?)SqlDbType.VarChar);
            parameters.Add("@giro_cve", customer.giro_cve, (DbType?)SqlDbType.VarChar);
            parameters.Add("@cli_medio", customer.cli_medio, (DbType?)SqlDbType.VarChar);
            parameters.Add("@credito", customer.credito, (DbType?)SqlDbType.VarChar);
            parameters.Add("@credito_dias", int.Parse(customer.credito_dias), (DbType?)SqlDbType.VarChar);
            parameters.Add("@cli_nom", customer.billing_address[0].NombreCliente.ToUpper(), (DbType?)SqlDbType.VarChar);
            parameters.Add("@cli_apat", customer.billing_address[0].apellidoPaterno.ToUpper(), (DbType?)SqlDbType.VarChar);
            parameters.Add("@cli_amat", customer.billing_address[0].apellidoMaterno.ToUpper(), (DbType?)SqlDbType.VarChar);
            parameters.Add("@cli_nombre", customer.billing_address[0].cli_nombre.ToUpper(), (DbType?)SqlDbType.VarChar);
            parameters.Add("@rfc", customer.billing_address[0].rfc.ToUpper(), (DbType?)SqlDbType.VarChar);
            if (customer.billing_address[0].rfc.Length == 13)
            {
                parameters.Add("@cli_DenominacionSocial", customer.billing_address[0].cli_nombre.ToUpper(), (DbType?)SqlDbType.VarChar);
            }
            else
            {
                parameters.Add("@cli_DenominacionSocial", customer.billing_address[0].denominacionSocial, (DbType?)SqlDbType.VarChar);
            }

            parameters.Add("@CveRegimenFiscal", customer.billing_address[0].regimenFiscal, (DbType?)SqlDbType.VarChar);
            parameters.Add("@cve_metodo_pago", customer.billing_address[0].cve_metodo_pago, (DbType?)SqlDbType.VarChar);
            parameters.Add("@cve_forma_pago", customer.billing_address[0].cve_forma_pago, (DbType?)SqlDbType.VarChar);
            parameters.Add("@cve_uso_cfdi", customer.billing_address[0].cve_uso_cfdi, (DbType?)SqlDbType.VarChar);
            parameters.Add("@codigo_postal", customer.billing_address[0].codigo_postal, (DbType?)SqlDbType.VarChar);
            parameters.Add("@ciudad", customer.billing_address[0].ciudad.ToUpper(), (DbType?)SqlDbType.VarChar);
            parameters.Add("@estado", customer.CodigoEstado, (DbType?)SqlDbType.Int);
            parameters.Add("@calle", customer.billing_address[0].calle.ToUpper(), (DbType?)SqlDbType.VarChar);
            parameters.Add("@colonia", customer.billing_address[0].colonia.ToUpper(), (DbType?)SqlDbType.VarChar);
            parameters.Add("@telefono", customer.Cli_ComprasCel.ToString(), (DbType?)SqlDbType.VarChar);
            parameters.Add("@pais", "Mexico", (DbType?)SqlDbType.VarChar);

            try
            {

                var cust= await db.ExecuteAsync("Magento_Cliente_Agregar", parameters, commandType: CommandType.StoredProcedure);
                //message
            }
            catch (Exception ex)
            {
                await db.CloseAsync();
                throw new Exception(ex.Message);
            }


            return true;
            //var newCustomer = db.QuerySingleOrDefault<Customer>("GetCustomerById", parameters, commandType: CommandType.StoredProcedure);
            // To do: return zona,agente cli_clave

            
        }


        /// <summary>
        /// Obtener un cliente a traves de la clave externa
        /// </summary>
        /// <param name="idCustomer"></param>
        /// <returns>
        ///     {
        ///         "cve_cliente":"2334"
        ///         "cli_cvematriz":"123"
        ///         "claveAgente":"1234"
        ///     }
        /// </returns>
        /// <exception cref="Exception"></exception>
        public async Task<CustomerResponse> GetCustomerById(string idCustomer)
        {

            string queryString = "select cli_clave as cve_cliente,cli_cvematriz as cli_cvematriz,cli_agente as claveAgente from inctclie (nolock) where cli_claveExterna = '" + idCustomer + "'";
            

            SqlConnection sqlConnection = DbConnection();
            sqlConnection.Open();

            CustomerResponse? customerResponse = default;
            try
            {

                customerResponse = await sqlConnection.QueryFirstOrDefaultAsync<CustomerResponse>(queryString);

               
                return customerResponse;
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

        public Task<bool> UpdateCustomer(Customer customer)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Validar si el cliente ya existe en la base de datos con el RFC
        /// </summary>
        /// <param name="customer"></param>
        /// <returns>Primer cliente con el rfc dado</returns>
        /// <exception cref="Exception"></exception>
        public async Task ValidateBillingCustomer(Customer customer)
        {

            var dbconnection = DbConnection();
            
            string consulta = @$"SELECT top 1 CLI_RFC as rfc FROM INCTCLIE (nolock) WHERE rtrim(ltrim(CLI_RFC))='{customer.billing_address[0].rfc.Trim()}'";
            try
            {
                var CustomerExist = await dbconnection.QueryFirstOrDefaultAsync<BillingAddress>(consulta);
               if(CustomerExist != null)
                {
                    if (CustomerExist.rfc.Trim() != "XAXX010101000")
                    {
                        throw new CustomerCustomException("El RFC del cliente ya esta registrado");
                    }  
                }
            }
            catch (Exception ex)
            {
                //logger
                throw new Exception(ex.ToString());
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
