using ApiGoldenstarServices.Data.Exceptions;
using ApiGoldenstarServices.Models.Goldenstar;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Protocols;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiGoldenstarServices.Data.DataAccess.Roltec
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
            parameters.Add("@cve_cliente", customer.IdCustomer, (DbType?)SqlDbType.VarChar);
            parameters.Add("@cli_cvematriz", customer.ParentCustomerKey, (DbType?)SqlDbType.VarChar);
            parameters.Add("@billingAddressId", customer.BillingAddress.IdBillingAddress, (DbType?)SqlDbType.VarChar);
            parameters.Add("@nombreCompra", customer.ShoppingName.ToUpper(), (DbType?)SqlDbType.VarChar);
            parameters.Add("@apellidoCompra", customer.ShoppingFirstName.ToUpper(), (DbType?)SqlDbType.VarChar);
            parameters.Add("@cli_email", customer.Email, (DbType?)SqlDbType.VarChar);
            parameters.Add("@Cli_ComprasCel", customer.ShoppingPhoneNumber, (DbType?)SqlDbType.VarChar);
            parameters.Add("@giro_cve", customer.KeyTurn, (DbType?)SqlDbType.VarChar);
            parameters.Add("@cli_medio", customer.MeansOfContact, (DbType?)SqlDbType.VarChar);
            parameters.Add("@credito", customer.Credit, (DbType?)SqlDbType.VarChar);
            parameters.Add("@credito_dias", int.Parse(customer.CreditDays), (DbType?)SqlDbType.VarChar);
            parameters.Add("@cli_nom", customer.BillingAddress.Name.ToUpper(), (DbType?)SqlDbType.VarChar);
            parameters.Add("@cli_apat", customer.BillingAddress.FirstName.ToUpper(), (DbType?)SqlDbType.VarChar);
            parameters.Add("@cli_amat", customer.BillingAddress.LastName.ToUpper(), (DbType?)SqlDbType.VarChar);
            parameters.Add("@cli_nombre", customer.BillingAddress.FullName.ToUpper(), (DbType?)SqlDbType.VarChar);
            parameters.Add("@rfc", customer.BillingAddress.Rfc, (DbType?)SqlDbType.VarChar);
            if (customer.BillingAddress.Rfc.Length == 13)
            {
                parameters.Add("@cli_DenominacionSocial", customer.BillingAddress.FullName.ToUpper(), (DbType?)SqlDbType.VarChar);
            }
            else
            {
                parameters.Add("@cli_DenominacionSocial", customer.BillingAddress.CompanyName, (DbType?)SqlDbType.VarChar);
            }

            parameters.Add("@CveRegimenFiscal", customer.BillingAddress.TaxRegime, (DbType?)SqlDbType.VarChar);
            parameters.Add("@cve_metodo_pago", customer.BillingAddress.PaymentMethodKey, (DbType?)SqlDbType.VarChar);
            parameters.Add("@cve_forma_pago", customer.BillingAddress.PaymentTypeKey, (DbType?)SqlDbType.VarChar);
            parameters.Add("@cve_uso_cfdi", customer.BillingAddress.CfdiUsageKey, (DbType?)SqlDbType.VarChar);
            parameters.Add("@codigo_postal", customer.BillingAddress.ZipCode, (DbType?)SqlDbType.VarChar);
            parameters.Add("@ciudad", customer.BillingAddress.City, (DbType?)SqlDbType.VarChar);
            parameters.Add("@estado", customer.BillingAddress.State, (DbType?)SqlDbType.Int);
            parameters.Add("@calle", customer.BillingAddress.Street.ToUpper(), (DbType?)SqlDbType.VarChar);
            parameters.Add("@colonia", customer.BillingAddress.Colony.ToUpper(), (DbType?)SqlDbType.VarChar);
            parameters.Add("@telefono", customer.ShoppingPhoneNumber, (DbType?)SqlDbType.VarChar);
            parameters.Add("@pais", "Mexico", (DbType?)SqlDbType.VarChar);

            try
            {

                var cust = await db.ExecuteAsync("RoltecAddCustomer", parameters, commandType: CommandType.StoredProcedure);
                //message

                await db.CloseAsync();

                return true;
            }
            catch (Exception ex)
            {
                await db.CloseAsync();

                throw new Exception(ex.Message);
            }

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
        public async Task<CustomerResponse> GetCustomerResponseById(string idCustomer)
        {

            string queryString = $@"select cli_clave as CustomerKey,cli_cvematriz as ParentCustomerKey,cli_agente as AgentKey from inctclie (nolock) where cli_claveExterna = '{idCustomer}'";

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
                cli_clave as CustomerKey
                ,cli_email as Email
                ,cli_rfc as Rfc
                ,cli_compra as ShoppingName
                from inctclie 
			    where cli_claveExterna <> ''";
            // agregar mas campos
            // 
            var CustomerList = await db.QueryAsync<Customer>(strQuery, new { });

            await db.CloseAsync();



            return CustomerList;

        }

        /// <summary>
        /// Actualizar los datos de un cliente
        /// </summary>
        /// <param name="customer"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> UpdateCustomer(Customer customer)
        {
            var db = DbConnection();
            await db.OpenAsync();
            // Add paremeters to StoreProcedure
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("@customerKey", customer.CustomerKey, (DbType?)SqlDbType.VarChar);
            parameters.Add("@cve_cliente", customer.IdCustomer, (DbType?)SqlDbType.VarChar);
            //parameters.Add("@cli_cvematriz", customer.ParentCustomerKey, (DbType?)SqlDbType.VarChar);
            parameters.Add("@billingAddressId", customer.BillingAddress.IdBillingAddress, (DbType?)SqlDbType.VarChar);
            parameters.Add("@nombreCompra", customer.ShoppingName.ToUpper(), (DbType?)SqlDbType.VarChar);
            parameters.Add("@apellidoCompra", customer.ShoppingFirstName.ToUpper(), (DbType?)SqlDbType.VarChar);
            parameters.Add("@cli_email", customer.Email, (DbType?)SqlDbType.VarChar);
            parameters.Add("@Cli_ComprasCel", customer.ShoppingPhoneNumber, (DbType?)SqlDbType.VarChar);
            parameters.Add("@giro_cve", customer.KeyTurn, (DbType?)SqlDbType.VarChar);
            parameters.Add("@cli_medio", customer.MeansOfContact, (DbType?)SqlDbType.VarChar);
            parameters.Add("@credito", customer.Credit, (DbType?)SqlDbType.VarChar);
            parameters.Add("@credito_dias", int.Parse(customer.CreditDays), (DbType?)SqlDbType.VarChar);
            parameters.Add("@cli_nom", customer.BillingAddress.Name.ToUpper(), (DbType?)SqlDbType.VarChar);
            parameters.Add("@cli_apat", customer.BillingAddress.FirstName.ToUpper(), (DbType?)SqlDbType.VarChar);
            parameters.Add("@cli_amat", customer.BillingAddress.LastName.ToUpper(), (DbType?)SqlDbType.VarChar);
            parameters.Add("@cli_nombre", customer.BillingAddress.FullName.ToUpper(), (DbType?)SqlDbType.VarChar);
            parameters.Add("@rfc", customer.BillingAddress.Rfc, (DbType?)SqlDbType.VarChar);
            if (customer.BillingAddress.Rfc.Length == 13)
            {
                parameters.Add("@cli_DenominacionSocial", customer.BillingAddress.FullName.ToUpper(), (DbType?)SqlDbType.VarChar);
            }
            else
            {
                parameters.Add("@cli_DenominacionSocial", customer.BillingAddress.CompanyName, (DbType?)SqlDbType.VarChar);
            }

            parameters.Add("@CveRegimenFiscal", customer.BillingAddress.TaxRegime, (DbType?)SqlDbType.VarChar);
            parameters.Add("@cve_metodo_pago", customer.BillingAddress.PaymentMethodKey, (DbType?)SqlDbType.VarChar);
            parameters.Add("@cve_forma_pago", customer.BillingAddress.PaymentTypeKey, (DbType?)SqlDbType.VarChar);
            parameters.Add("@cve_uso_cfdi", customer.BillingAddress.CfdiUsageKey, (DbType?)SqlDbType.VarChar);
            parameters.Add("@codigo_postal", customer.BillingAddress.ZipCode, (DbType?)SqlDbType.VarChar);
            parameters.Add("@ciudad", customer.BillingAddress.City, (DbType?)SqlDbType.VarChar);
            parameters.Add("@estado", customer.BillingAddress.State, (DbType?)SqlDbType.Int);
            parameters.Add("@calle", customer.BillingAddress.Street.ToUpper(), (DbType?)SqlDbType.VarChar);
            parameters.Add("@colonia", customer.BillingAddress.Colony.ToUpper(), (DbType?)SqlDbType.VarChar);
            parameters.Add("@telefono", customer.ShoppingPhoneNumber, (DbType?)SqlDbType.VarChar);
            parameters.Add("@pais", "Mexico", (DbType?)SqlDbType.VarChar);

            try
            {

                var cust = await db.ExecuteAsync("RoltecUpdateCustomer", parameters, commandType: CommandType.StoredProcedure);
                //message

                await db.CloseAsync();

                return true;
            }
            catch (Exception ex)
            {
                await db.CloseAsync();

                throw new Exception(ex.Message);
            }

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

            string consulta = @$"SELECT top 1 CLI_RFC as Rfc FROM INCTCLIE (nolock) WHERE rtrim(ltrim(CLI_RFC))='{customer.BillingAddress.Rfc.Trim()}'";
            try
            {
                var CustomerExist = await dbconnection.QueryFirstOrDefaultAsync<BillingAddress>(consulta);
                if (CustomerExist != null)
                {
                    if (CustomerExist.Rfc.Trim() != "XAXX010101000")
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

        /// <summary>
        /// Agregar o actualizar una direccion de envio del cliente
        /// </summary>
        /// <param name="shippingAddress"></param>
        /// <param name="customerId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> AddShippingAddressToCustomer(ShippingAddress shippingAddress, string customerId)
        {
            var db = DbConnection();
            await db.OpenAsync();
            // Add paremeters to StoreProcedure
            DynamicParameters parameters = new DynamicParameters();

            parameters.Add("@cve_cliente", customerId, (DbType?)SqlDbType.VarChar);
            parameters.Add("@cve_sucursal", shippingAddress.ShippingAddressId, (DbType?)SqlDbType.VarChar);//id que genera la pagina
            parameters.Add("@sucursal", shippingAddress.Alias, (DbType?)SqlDbType.VarChar);//nombre de la sucursal
            parameters.Add("@calle", shippingAddress.Street, (DbType?)SqlDbType.VarChar);
            parameters.Add("@colonia", shippingAddress.Colony, (DbType?)SqlDbType.VarChar);
            parameters.Add("@ciudad", shippingAddress.City, (DbType?)SqlDbType.VarChar);
            parameters.Add("@telefono", shippingAddress.Phone, (DbType?)SqlDbType.VarChar);
            parameters.Add("@codigo_postal", shippingAddress.ZipCode, (DbType?)SqlDbType.VarChar);
            parameters.Add("@cve_ciudad", shippingAddress.CityKey, (DbType?)SqlDbType.VarChar);

            try
            {
                // validar si existe para aplicar el update o el add
                var shipppingAddresExist = await ShippingAddressExist(customerId, shippingAddress.ShippingAddressId.ToString());
                if (shipppingAddresExist == true)
                {
                    await db.ExecuteAsync("RoltecUpdateShippingAddress", parameters, commandType: CommandType.StoredProcedure);
                }
                else
                {

                    await db.ExecuteAsync("RoltecAddShippingAddress", parameters, commandType: CommandType.StoredProcedure);
                }
                //message

                await db.CloseAsync();

                return true;
            }
            catch (Exception ex)
            {
                await db.CloseAsync();

                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Validar si la direccion de envio ya existe
        /// </summary>
        /// <param name="idCustomer"></param>
        /// <param name="shippingAddressId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private async Task<bool> ShippingAddressExist(string idCustomer, string shippingAddressId)
        {
            string queryString = $@"select cliente as CustomerId,IDSuc as ShippingAddressId  from from SUCS_DOMICILIOS (nolock) where cliente='{idCustomer}' and IDSuc='{shippingAddressId}'";

            SqlConnection sqlConnection = DbConnection();
            sqlConnection.Open();

            ShippingAddress? shippingAddress = default;
            try
            {

                shippingAddress = await sqlConnection.QueryFirstOrDefaultAsync<ShippingAddress>(queryString);

                if (shippingAddress == null) { return false; }

                return true;
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

        /// <summary>
        /// Validar si existe el cliente con el customerkey
        /// </summary>
        /// <param name="customerKey"></param>
        /// <returns></returns>
        public async Task<bool> GetCustomerByCustumerKey(string customerKey)
        {
            string queryString = $@"select cli_clave as CustomerKey,cli_cvematriz as ParentCustomerKey,cli_agente as AgentKey from inctclie (nolock) where cli_clave = '{customerKey}'";

            SqlConnection sqlConnection = DbConnection();


            var customerExist = await sqlConnection.QueryFirstOrDefaultAsync<CustomerResponse>(queryString);

            if (customerExist != null)
            {
                return true;
            }
            return false;

        }

        public async Task<bool> GetBillingCustomerById(string IdBillingAddress)
        {
            string queryString = $@"select idBillingAddress as IdBillingAddress, from inctclie (nolock) where cli_clave = '{IdBillingAddress}'";

            SqlConnection sqlConnection = DbConnection();


            var customerExist = await sqlConnection.QueryFirstOrDefaultAsync<BillingAddress>(queryString);

            if (customerExist != null)
            {
                return true;
            }
            return false;

        }

        public Task<BillingAddress> AddBillingAddressToCustomer(BillingAddress billingAddress)
        {
            throw new NotImplementedException();
        }

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
