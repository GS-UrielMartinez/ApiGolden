
using ApiGoldenstarServices.Data.DataAccess; 
using ApiGoldenstarServices.Data.DataAccess.Admin.Roltec;
using ApiGoldenstarServices.Data.DataAccess.Goldenstar;
using ApiGoldenstarServices.Data.Exceptions;
using ApiGoldenstarServices.Models.Goldenstar;
using ApiGoldenstarServices.Models.Goldenstar.BaseModels;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Protocols;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace ApiGoldenstarServices.Data.DataAccess.Roltec
{
    public class DACustomer : DABase  // : ICustomer
    {
        //private SqlConfiguration _SqlConfiguration;
        //private EjecutorSql _EjecutorSql;


        public DACustomer(SqlConfiguration sqlConfiguration) : base(sqlConfiguration)
        {
            //
        }
        public DACustomer(SqlConnection sqlConnection) : base(sqlConnection)
        {
            //
        }

        // Get connection to Database
        private SqlConnection DbConnection()
        {
            Exception ex =
                new Exception(
                    "Los Métodos que implementan " 
                    + this.GetType().Name + ".DbConnection(),  ya NO deben ni requieren usarse."
                );
            if (ex != null) throw ex;

            //return new SqlConnection(_SqlConfiguration.ConnectionString);
            return 
                base.DbConnectionBase();
        }

        #region " Add ( Customer Or BillingAddress ) "

        public async Task<CustomerResponse?> _AddCustomerAsync(Customer customer) 
        {
            if (customer == null)
                throw new Exception("El Servicio Web NO recibió ningún dato para crear el registro del Cliente.");

            SqlConnection? dbConn = await this._EjecutorSql.OpenDbConnectionOrNullIfAlreadyAsync();
            SqlTransaction? dbTransaction = this._EjecutorSql.BeginTransactionOrNullIfCurrent();

            try
            {
                //CustomerResponse buscarCustomer = await this.GetCustomerResponseById(customer.IdCustomer);

                bool _FindCustomerById = 
                    await this.FindCustomerByIdAsync(customer.IdCustomer); 
                if (_FindCustomerById)
                    throw new Exception("Ya existe un cliente con el IdCustomer " + customer.IdCustomer);


                // El valor de  ParentCustomerKey  debe nacer vacío
                // para indicar que aún NO se registrará formalmente la primer  BillingAddress 
                customer.ParentCustomerKey = "";

                // Asegurar que .BillingAddress  NO quede nulo, 
                // NO importa que se pierdan los valores que pudiera contener de NO haber sido nula previamente
                // porque éstos serán ignorados al no haber un valor de .ParentCustomerKey,
                // pero el objeto es necesario para evitar alguna 'NullReferenceException'
                customer.BillingAddress = new BillingAddress();


                CustomerResponse? newCustomerResponse =
                    await this._AddCustomerOrBillingAddressAsync(customer);


                if (dbTransaction != null) { await dbTransaction.RollbackAsync(); }
                //if (dbTransaction != null) { await dbTransaction.CommitAsync(); }
                if (dbConn != null) { await dbConn.CloseAsync(); }

                return
                    newCustomerResponse;
            }
            catch (Exception ex)
            {
                if (dbTransaction != null) { await dbTransaction.RollbackAsync(); }
                if (dbConn != null) { await dbConn.CloseAsync(); }

                throw new Exception(ex.Message);
            }
        }
        public async Task<CustomerResponse?> _AddBillingAddressAsync(Customer customer) 
        {
            if (customer == null)
                throw new Exception("El Servicio Web NO recibió ningún dato para registrar la Dirección de Facturación.");
            if (customer.BillingAddress == null)
            {
                throw
                    new Exception(
                        "El Servicio Web NO recibió los datos necesarios para registrar la Dirección de Facturación"
                        + " (" + nameof(customer.BillingAddress) + " = null)."
                    );
            }

            SqlConnection? dbConn = await this._EjecutorSql.OpenDbConnectionOrNullIfAlreadyAsync();
            SqlTransaction? dbTransaction = this._EjecutorSql.BeginTransactionOrNullIfCurrent();

            try
            {
                if (customer.BillingAddress.Rfc == null) 
                    customer.BillingAddress.Rfc = "";
                bool _FindCustomerByRfc =
                    await this.FindCustomerByRfcAsync(customer.BillingAddress.Rfc);
                if (_FindCustomerByRfc)
                    throw new Exception("Ya existe un cliente registrado con el Rfc " + customer.BillingAddress.Rfc);


                if (customer.ParentCustomerKey == null)
                    customer.ParentCustomerKey = "";
                string _customerKeyBuscar = 
                    (customer.ParentCustomerKey.Trim() != "")
                    ? customer.ParentCustomerKey
                    : customer.CustomerKey
                    ;

                Customer _ParentCustomer =
                    await this._GetCustomerAsync(_customerKeyBuscar, customer.IdCustomer);
                if (_ParentCustomer == null)
                {
                    throw
                        new Exception("NO se encontró ningún Cliente"
                        + " con el CustomerKey " + customer.ParentCustomerKey.Trim()
                        + " y el IdCustomer " + customer.IdCustomer.Trim() + ".");
                }


                // Integrar los datos Nuevos de BillingAddress, con los datos básicos originales del Cliente, 
                // para asegurar mantener congruentes estos últimos
                _ParentCustomer.BillingAddress = customer.BillingAddress;


                CustomerResponse? customerResponse = null;

                if (_ParentCustomer.ParentCustomerKey == null) _ParentCustomer.ParentCustomerKey = "";
                if (_ParentCustomer.ParentCustomerKey.Trim() == "")
                {
                    #region " Actualizar los datos de la 1er  BillingAddress, en el registro del ParentCustomer " 

                    // .ParentCustomerKey  recibe el mismo valor de .CustomerKey
                    // para que deje de estar vacío, e indicar así que estará registrada ahí 
                    // la primer  BillingAddress 
                    _ParentCustomer.ParentCustomerKey =
                        _ParentCustomer.CustomerKey.Trim();

                    // Actualizar los datos de la 1er  BillingAddress, en el registro del ParentCustomer
                    Customer updatedCustomer =
                        await this._UpdateBillingAddressAsync(_ParentCustomer);

                    customerResponse = 
                        this.ToCustomerResponse(
                            updatedCustomer,
                            "La Dirección de Facturación del Cliente se registró exitosamente."
                        );

                    #endregion
                }
                else
                {
                    #region " Agregar una nueva Dirección de Facturación adicional. " 

                    //_ParentCustomer.ParentCustomerKey = _ParentCustomer.CustomerKey.Trim();
                    // -> Esto provocaba que una Dirección Adicional pudiera quedar como Parent, 
                    // El .ParentCustomerKey  que se haya obtenido en la búsqueda será el correcto,
                    // no necesita ni debe cambiarse. 


                    _ParentCustomer.CustomerKey = ""; // El Método generará un .CustomerKey  nuevo

                    // Agregar un nuevo registro físico de Cliente, 
                    // para usarlo como Dirección de Facturación adicional. 
                    customerResponse =
                        await this._AddCustomerOrBillingAddressAsync(_ParentCustomer);

                    #endregion
                }


                if (dbTransaction != null) { await dbTransaction.RollbackAsync(); }
                //if (dbTransaction != null) { await dbTransaction.CommitAsync(); }
                if (dbConn != null) { await dbConn.CloseAsync(); }

                return
                    customerResponse;
            }
            catch (Exception ex)
            {
                if (dbTransaction != null) { await dbTransaction.RollbackAsync(); }
                if (dbConn != null) { await dbConn.CloseAsync(); }

                throw new Exception(ex.Message);
            }
        }
        private
            async Task<CustomerResponse?> _AddCustomerOrBillingAddressAsync(Customer customer) 
        {
            return
                //await this._AddCustomerOrBillingAddressAsync_SinSP(customer); 
                //await this._AddCustomerOrBillingAddressAsync_ConSP(customer); 
                await this._AddCustomerOrBillingAddressAsync_SinSP(customer);
        }
        private async Task<CustomerResponse?> _AddCustomerOrBillingAddressAsync_ConSP(Customer customer) 
        {
            if (customer == null)
                throw new Exception("El Servicio Web NO recibió ningún dato para registrar al Cliente.");

            SqlConnection? dbConn = await this._EjecutorSql.OpenDbConnectionOrNullIfAlreadyAsync();
            SqlTransaction? dbTransaction = this._EjecutorSql.BeginTransactionOrNullIfCurrent();

            try
            {
                if (customer.ParentCustomerKey == null)
                    customer.ParentCustomerKey = "";

                string successMessage =
                    (customer.ParentCustomerKey.Trim() == "")
                    ? "Se registraron exitosamente los datos del Cliente."
                    : "La Dirección de Facturación del Cliente se registró exitosamente."
                    ;


                bool _addCustomer = await this.AddCustomer(customer);


                Customer newCustomer =
                    await this.GetCustomerByIdAsync(customer.IdCustomer);
                CustomerResponse? newCustomerResponse =
                    this.ToCustomerResponse(newCustomer, successMessage);


                if (dbTransaction != null) { await dbTransaction.CommitAsync(); }
                if (dbConn != null) { await dbConn.CloseAsync(); }

                return
                    newCustomerResponse;
            }
            catch (Exception ex)
            {
                if (dbTransaction != null) { await dbTransaction.RollbackAsync(); }
                if (dbConn != null) { await dbConn.CloseAsync(); }

                throw new Exception(ex.Message);
            }
        }
        private async Task<CustomerResponse?> _AddCustomerOrBillingAddressAsync_SinSP(Customer customer) 
        {
            if (customer == null)
                throw new Exception("El Servicio Web NO recibió ningún dato para registrar al Cliente.");

            SqlConnection? dbConn = await this._EjecutorSql.OpenDbConnectionOrNullIfAlreadyAsync();
            SqlTransaction? dbTransaction = this._EjecutorSql.BeginTransactionOrNullIfCurrent();

            try
            {
                #region " Obtener PrimaryKey, Zona y AgenteWeb "

                string strQueryFolio = ""
                    + " select max(cli_clave + 1) as 'cli_clave'  from inctclie (nolock) "
                    + " where cli_clave between  @folio_minimo  and  @folio_maximo "
                    ;
                strQueryFolio =
                    " select coalesce( (" + strQueryFolio + "), @folio_minimo ) "
                    ;
                string strQueryZona = " select coalesce( (select top 1  zona  from tbestados (nolock)  where edo_cve = @estado), '' ) ";
                string strQueryAgente = " select coalesce( (select top 1  AgenteWeb  from inctvend (nolock)  where ven_cla = @ZONA), '' ) ";


                DynamicParameters paramsGet = new DynamicParameters();

                paramsGet.Add("@folio_minimo", "20000", base.ToDbType(SqlDbType.VarChar));
                paramsGet.Add("@folio_maximo", "50000", base.ToDbType(SqlDbType.VarChar));
                string newCustomerKey =
                    await this._EjecutorSql.QuerySingleOrDefaultAsync<string>(strQueryFolio, paramsGet, commandType: CommandType.Text);

                // Obtener la  Zona  que corresponde al  Estado
                paramsGet.Add("@estado", customer.BillingAddress.State, base.ToDbType(SqlDbType.Int)); // SqlDbType.Int
                string claveZona =
                    await this._EjecutorSql.QuerySingleOrDefaultAsync<string>(strQueryZona, paramsGet, commandType: CommandType.Text);

                // Obtener el  AgenteWeb  que corresponde a la  Zona
                paramsGet.Add("@ZONA", claveZona, base.ToDbType(SqlDbType.VarChar));
                string agenteWeb =
                    await this._EjecutorSql.QuerySingleOrDefaultAsync<string>(strQueryAgente, paramsGet, commandType: CommandType.Text);


                // Rellenar con Ceros hasta después de Obtener el  AgenteWeb  que corresponde a la  Zona
                claveZona = claveZona.Trim().PadLeft(4, '0');
                agenteWeb = agenteWeb.Trim().PadLeft(4, '0');

                #endregion 


                Int16 diasCredito =
                    customer.CreditDays;
                Int16 diasCartera =
                    ((Int16) ((diasCredito == 0) ? 5 : (diasCredito + 15)));
                string cveCredito =
                    (diasCredito == 30) ? "03" : "0";

                decimal tasaIVA = (decimal) 0.16;


                DynamicParameters paramsInsert = new DynamicParameters();

                #region " Parámetros con Valores  para el Query " 

                // (DbType?) SqlDbType.VarChar  base.ToDbType(SqlDbType.VarChar)
                paramsInsert.Add("@cli_clave", newCustomerKey, base.ToDbType(SqlDbType.VarChar));
                paramsInsert.Add("@cli_claveExterna", customer.IdCustomer, base.ToDbType(SqlDbType.VarChar));
                paramsInsert.Add("@cli_cvematriz", customer.ParentCustomerKey, base.ToDbType(SqlDbType.VarChar));

                // Valores originalmente calculados por el  SqlStoredProcedure
                paramsInsert.Add("@cli_dias", diasCredito, base.ToDbType(SqlDbType.SmallInt)); // SqlDbType.SmallInt
                paramsInsert.Add("@dcartera", diasCartera, base.ToDbType(SqlDbType.SmallInt)); // SqlDbType.SmallInt
                paramsInsert.Add("@cli_cvecred", cveCredito, base.ToDbType(SqlDbType.VarChar));
                paramsInsert.Add("@tasa", tasaIVA, base.ToDbType(SqlDbType.Decimal)); // SqlDbType.Decimal
                paramsInsert.Add("@cli_ncar", claveZona, base.ToDbType(SqlDbType.VarChar));
                paramsInsert.Add("@cli_agente", agenteWeb, base.ToDbType(SqlDbType.VarChar));

                // Valores Hard-Codeados
                paramsInsert.Add("@cli_autcredito", "S", base.ToDbType(SqlDbType.VarChar));
                paramsInsert.Add("@cli_montoautorizado", 0, base.ToDbType(SqlDbType.Decimal)); // SqlDbType.Decimal
                paramsInsert.Add("@cli_pob", "0", base.ToDbType(SqlDbType.VarChar));
                paramsInsert.Add("@creditoWeb", "", base.ToDbType(SqlDbType.VarChar));
                paramsInsert.Add("@cli_id_contacto", 1, base.ToDbType(SqlDbType.Int)); // SqlDbType.Int

                // Email
                paramsInsert.Add("@cli_email", customer.Email, base.ToDbType(SqlDbType.VarChar));
                paramsInsert.Add("@correo_notificaciones", customer.Email, base.ToDbType(SqlDbType.VarChar));
                paramsInsert.Add("@cli_REPmail", customer.Email, base.ToDbType(SqlDbType.VarChar));

                // Se agregó una propiedad  BillingAddress.Email  que apunta a ésta columna 
                // para que forme parte de los datos de Facturación, y ya NO de los datos básicos.
                //paramsInsert.Add("@Cli_FacturaMail", customer.Email, base.ToDbType(SqlDbType.VarChar));

                paramsInsert.Add("@cli_compra", customer.ShoppingName.ToUpper(), base.ToDbType(SqlDbType.VarChar));
                paramsInsert.Add("@Cli_CompraApellido", customer.ShoppingFirstName.ToUpper(), base.ToDbType(SqlDbType.VarChar));
                paramsInsert.Add("@cli_giro", customer.KeyTurn, base.ToDbType(SqlDbType.TinyInt));
                paramsInsert.Add("@Cli_Medio", customer.MeansOfContact, base.ToDbType(SqlDbType.TinyInt));

                paramsInsert.Add("@cli_tel", customer.ShoppingPhoneNumber, base.ToDbType(SqlDbType.VarChar));
                paramsInsert.Add("@cli_tel1", customer.ShoppingPhoneNumber, base.ToDbType(SqlDbType.VarChar));
                paramsInsert.Add("@Cli_ComprasCel", customer.ShoppingPhoneNumber, base.ToDbType(SqlDbType.VarChar));

                #endregion  

                string successMessage = "Se registraron exitosamente los datos del Cliente.";

                if (customer.ParentCustomerKey == null)
                    customer.ParentCustomerKey = "";

                if (customer.ParentCustomerKey.Trim() == "")
                {
                    // Los parámetros de BillingAddress NO se pudieron omitir 
                    // en caso de NO requerir asignarles ningún valor, porque algunas columnas son NOT NULL, 
                    // y en lugar de eso se optó por re-instanciar todo el objeto .BillingAddress 
                    // cuando corresponda, dejando todas sus Propiedades en blanco.
                    customer.BillingAddress = this._ClearBillingAddress(); 
                }
                else
                {
                    successMessage = "La Dirección de Facturación del Cliente se registró exitosamente.";

                    if (customer.BillingAddress == null)
                    {
                        throw
                            new Exception(
                                "El Servicio Web NO recibió ningún dato referente a Dirección de Facturación"
                                + " (" + nameof(customer.BillingAddress) + " = null)."
                            );
                    }
                }

                // Los parámetros de BillingAddress NO se pudieron omitir 
                // en caso de NO requerir asignarles ningún valor, porque algunas columnas son NOT NULL, 
                // y en lugar de eso se optó por re-instanciar todo el objeto .BillingAddress 
                // cuando corresponda, dejando todas sus Propiedades en blanco.
                #region " Parámetros con Valores de  BillingAddress  para el Query " 

                BillingAddress _billAds =
                    customer.BillingAddress;

                // (DbType?) SqlDbType.VarChar  base.ToDbType(SqlDbType.VarChar)
                paramsInsert.Add("@idBillingAddress", _billAds.IdBillingAddress, base.ToDbType(SqlDbType.VarChar));

                paramsInsert.Add("@cli_rfc", _billAds.Rfc, base.ToDbType(SqlDbType.VarChar));
                string strDenominacionSocial =
                    (_billAds.Rfc.Length == 13)
                    ? _billAds.FullName.ToUpper()
                    : _billAds.CompanyName
                    ;
                paramsInsert.Add("@cli_DenominacionSocial", strDenominacionSocial, base.ToDbType(SqlDbType.VarChar));

                paramsInsert.Add("@cli_nom", _billAds.Name.ToUpper(), base.ToDbType(SqlDbType.VarChar)); 
                paramsInsert.Add("@cli_apat", _billAds.FirstName.ToUpper(), base.ToDbType(SqlDbType.VarChar));
                paramsInsert.Add("@cli_amat", _billAds.LastName.ToUpper(), base.ToDbType(SqlDbType.VarChar));

                paramsInsert.Add("@cli_nombre", _billAds.FullName.ToUpper(), base.ToDbType(SqlDbType.VarChar));
                paramsInsert.Add("@cli_nomComercial", _billAds.FullName.ToUpper(), base.ToDbType(SqlDbType.VarChar));

                paramsInsert.Add("@Cli_FacturaMail", _billAds.Email, base.ToDbType(SqlDbType.VarChar));

                paramsInsert.Add("@cli_direc", _billAds.Street.ToUpper(), base.ToDbType(SqlDbType.VarChar));  // varchar(80) 
                paramsInsert.Add("@cli_colonia", _billAds.Colony.ToUpper(), base.ToDbType(SqlDbType.VarChar));  // varchar(50) 
                paramsInsert.Add("@cli_cp", _billAds.ZipCode, base.ToDbType(SqlDbType.VarChar));

                paramsInsert.Add("@cveUsoCFDI", _billAds.CfdiUsageKey, base.ToDbType(SqlDbType.VarChar));  // varchar(3) 
                paramsInsert.Add("@cveFormaPago", _billAds.PaymentTypeKey, base.ToDbType(SqlDbType.VarChar));  // varchar(2) 
                paramsInsert.Add("@cveMetodoPago", _billAds.PaymentMethodKey, base.ToDbType(SqlDbType.VarChar));  // varchar(3) 
                paramsInsert.Add("@CveRegimenFiscal", _billAds.TaxRegime, base.ToDbType(SqlDbType.VarChar));  // varchar(3) 

                #endregion


                List<string> _ParameterNames =
                    paramsInsert.ParameterNames.Select(x => "@" + x).ToList();
                string strParametrosConcatenados =
                    string.Join(", ", _ParameterNames);

                string strQueryInsertParametrizado = ""
                    + " insert into inctclie ( " + strParametrosConcatenados.Replace("@", "") + " ) "
                    + " values ( " + strParametrosConcatenados + " ) "
                    ;

                int executeInsertCustomer =
                    await this._EjecutorSql.ExecuteAsync(strQueryInsertParametrizado, paramsInsert, commandType: CommandType.Text);


                //CustomerResponse newCustomer = await this.GetCustomerResponseById(customer.IdCustomer);
                Customer insertedCustomer = 
                    await this._GetCustomerAsync(newCustomerKey, customer.IdCustomer); 
                if (insertedCustomer == null)
                    throw new Exception("NO se encontró ningún registro recién creado del Cliente con idCustomer " + customer.IdCustomer.Trim() + ".");

                //newCustomer.Message = successMessage;
                CustomerResponse? customerResponse = 
                    this.ToCustomerResponse(insertedCustomer, successMessage);


                if (dbTransaction != null) { await dbTransaction.CommitAsync(); }
                if (dbConn != null) { await dbConn.CloseAsync(); }

                return
                    //newCustomer; 
                    //customerResponse; 
                    customerResponse; 
            }
            catch (Exception ex)
            {
                if (dbTransaction != null) { await dbTransaction.RollbackAsync(); }
                if (dbConn != null) { await dbConn.CloseAsync(); }

                throw new Exception(ex.Message);
            }
        }

        private BillingAddress _ClearBillingAddress() 
        {
            return
                new BillingAddress()
                {
                    CfdiUsageKey = "",
                    City = "",
                    CityKey = "",
                    Colony = "",
                    CompanyName = "",
                    CreditDays = new short(),
                    Email = "",

                    Name = "",
                    FirstName = "",
                    LastName = "",
                    FullName = "",

                    IdBillingAddress = "",
                    PaymentMethodKey = "",
                    PaymentTypeKey = "",
                    Rfc = "",
                    State = new int(),
                    Street = "",
                    TaxRegime = "",
                    ZipCode = "",
                    //
                };
        }

        public CustomerResponse? ToCustomerResponse(Customer? customer, string message = "") 
        {
            if (customer == null) return null;

            return 
                new CustomerResponse() 
                {
                    CustomerKey = customer.CustomerKey,
                    IdCustomer = customer.IdCustomer,
                    ParentCustomerKey = customer.ParentCustomerKey,
                    AgentKey = customer.AgentKey,
                    //
                    Message = message,
                }; 
        }

        #endregion


        #region " Update Customer " 

        public async Task<Customer> _UpdateCustomerAsync(Customer customer) 
        {
            this._UpdateCustomerPreValidar(customer);

            SqlConnection? dbConn = await this._EjecutorSql.OpenDbConnectionOrNullIfAlreadyAsync();
            SqlTransaction? dbTransaction = this._EjecutorSql.BeginTransactionOrNullIfCurrent();

            try
            {
                Customer? buscarCustomer =
                    await this._GetCustomerAsync(customer.CustomerKey, customer.IdCustomer);

                // Restaurar Valor Original de Campos que el método NO permite modificar 
                customer.CreditDays = buscarCustomer.CreditDays;
                customer.ParentCustomerKey = buscarCustomer.ParentCustomerKey;
                customer.BillingAddress =
                    //buscarCustomer.BillingAddress; 
                    //await this.GetBillingAddressAsync(customer.CustomerKey, customer.IdCustomer);
                    await this.GetBillingAddressAsync(customer.CustomerKey, customer.IdCustomer);

                Customer updatedCustomer =
                    await this._UpdateCustomerAllAccessAsync(customer);


                if (dbTransaction != null) { await dbTransaction.RollbackAsync(); }
                //if (dbTransaction != null) { await dbTransaction.CommitAsync(); }
                if (dbConn != null) { await dbConn.CloseAsync(); }

                return
                    updatedCustomer;
            }
            catch (Exception ex)
            {
                if (dbTransaction != null) { await dbTransaction.RollbackAsync(); }
                if (dbConn != null) { await dbConn.CloseAsync(); }

                throw new Exception(ex.Message);
            }
        }
        private
            async Task<Customer> _UpdateCustomerAllAccessAsync(Customer customer)
        // Privado  mientras se define SI ALGUIEN lo va a requerir 
        {
            return
                //await this._UpdateCustomerAllAccessAsync_ConSP(customer); 
                //await this._UpdateCustomerAllAccessAsync_SinSP(customer); 
                await this._UpdateCustomerAllAccessAsync_SinSP(customer);
        }
        private void _UpdateCustomerPreValidar(Customer customer) 
        {
            if (customer == null)
                throw new Exception("El Servicio Web NO recibió ningún dato del Cliente para ser Actualizado.");
            if (customer.CustomerKey == null) customer.CustomerKey = "";
            //if (customer.ParentCustomerKey == null) customer.ParentCustomerKey = "";
            if (customer.CustomerKey.Trim() == "")
            {
                throw
                    new Exception("Para identificar a un Cliente se requiere un "
                    + nameof(customer.CustomerKey) + " que NO esté en blanco.");
            }
        }
        private async Task<Customer> _UpdateCustomerAllAccessAsync_ConSP(Customer customer) 
        {
            this._UpdateCustomerPreValidar(customer);

            SqlConnection? dbConn = await this._EjecutorSql.OpenDbConnectionOrNullIfAlreadyAsync();
            SqlTransaction? dbTransaction = this._EjecutorSql.BeginTransactionOrNullIfCurrent();

            try
            {
                // Buscar el Cliente sólo para validar que exista, el método lanza una Excepción si no lo encuentra. 
                Customer? buscarCustomer =
                    await this._GetCustomerAsync(customer.CustomerKey, customer.IdCustomer);

                // Recuperar el valor original de .ParentCustomerKey,
                // para evitar que se le pudiera haber asignado algún valor arbitrario distinto,
                // ya que será usado como filtro 
                customer.ParentCustomerKey =
                    buscarCustomer.ParentCustomerKey;
                if (customer.ParentCustomerKey == null)
                    customer.ParentCustomerKey = "";


                // La Actualización aplica para todos los registros de Direcciones Adicionales 
                // que tengan el mismo .ParentCustomerKey, así como para el Cliente al que apuntan todos éstos, 
                // por esta razón si .ParentCustomerKey está vacío, se le asigna el mismo valor que .CustomerKey, 
                // ya que si el StoredProcedure 'spRoltecUpdateCustomer' NO recibe un valor de .ParentCustomerKey, 
                // entonces ejecutaría Actualización de  BillingAddress, y NO de Datos Básicos del Cliente.
                // ( El valor de .ParentCustomerKey  NO se Actualiza, únicamente se usa como filtro. )
                if (customer.ParentCustomerKey.Trim() == "")
                    customer.ParentCustomerKey = customer.CustomerKey;


                bool _updateCustomer = await this.UpdateCustomer(customer);

                Customer updatedCustomer =
                    await this._GetCustomerAsync(customer.CustomerKey, customer.IdCustomer);


                if (dbTransaction != null) { await dbTransaction.CommitAsync(); }
                if (dbConn != null) { await dbConn.CloseAsync(); }

                return
                    updatedCustomer;
            }
            catch (Exception ex)
            {
                if (dbTransaction != null) { await dbTransaction.RollbackAsync(); }
                if (dbConn != null) { await dbConn.CloseAsync(); }

                throw new Exception(ex.Message);
            }
        }
        private async Task<Customer> _UpdateCustomerAllAccessAsync_SinSP(Customer customer) 
        {
            this._UpdateCustomerPreValidar(customer);

            SqlConnection? dbConn = await this._EjecutorSql.OpenDbConnectionOrNullIfAlreadyAsync();
            SqlTransaction? dbTransaction = this._EjecutorSql.BeginTransactionOrNullIfCurrent();

            try
            {
                // Buscar el Cliente sólo para validar que exista, el método lanza una Excepción si no lo encuentra. 
                Customer? buscarCustomer =
                    await this._GetCustomerAsync(customer.CustomerKey, customer.IdCustomer);

                // Recuperar el valor original de .ParentCustomerKey,
                // para evitar que se le pudiera haber asignado algún valor arbitrario distinto,
                // ya que será usado como filtro 
                customer.ParentCustomerKey =
                    buscarCustomer.ParentCustomerKey;
                if (customer.ParentCustomerKey == null)
                    customer.ParentCustomerKey = "";

                string _valorCustomerKeyFiltro =
                    (customer.ParentCustomerKey.Trim() != "")
                    ? customer.ParentCustomerKey
                    : customer.CustomerKey
                    ;


                // Estos Valores únicamente se podrán actualizar internamente, NO desde la WEB
                Int16 diasCredito =
                    customer.CreditDays;
                Int16 diasCartera =
                    ((Int16) ((diasCredito == 0) ? 5 : (diasCredito + 15)));
                string cveCredito =
                    (diasCredito == 30) ? "03" : "0";
                //decimal tasaIVA = (decimal) 0.16;


                DynamicParameters paramsUpdate = new DynamicParameters();

                #region " Parámetros con Valores  para el Query " 

                // (DbType?) SqlDbType.VarChar  base.ToDbType(SqlDbType.VarChar)

                //paramsUpdate.Add("@cli_clave", newCustomerKey, base.ToDbType(SqlDbType.VarChar));
                //paramsUpdate.Add("@cli_claveExterna", customer.IdCustomer, base.ToDbType(SqlDbType.VarChar));
                //paramsUpdate.Add("@cli_cvematriz", customer.ParentCustomerKey, base.ToDbType(SqlDbType.VarChar));

                // Valores originalmente calculados por el  SqlStoredProcedure
                //  ( Estos Valores únicamente se podrán actualizar internamente, NO desde la WEB )
                paramsUpdate.Add("@cli_dias", diasCredito, base.ToDbType(SqlDbType.SmallInt)); // SqlDbType.SmallInt
                paramsUpdate.Add("@dcartera", diasCartera, base.ToDbType(SqlDbType.SmallInt)); // SqlDbType.SmallInt
                paramsUpdate.Add("@cli_cvecred", cveCredito, base.ToDbType(SqlDbType.VarChar));
                //paramsUpdate.Add("@tasa", tasaIVA, base.ToDbType(SqlDbType.Decimal)); // SqlDbType.Decimal
                //paramsUpdate.Add("@cli_ncar", claveZona, base.ToDbType(SqlDbType.VarChar));
                //paramsUpdate.Add("@cli_agente", agenteWeb, base.ToDbType(SqlDbType.VarChar));

                // Valores Hard-Codeados
                //paramsUpdate.Add("@cli_autcredito", "S", base.ToDbType(SqlDbType.VarChar));
                paramsUpdate.Add("@cli_montoautorizado", 0, base.ToDbType(SqlDbType.Decimal));
                //paramsUpdate.Add("@cli_pob", "0", base.ToDbType(SqlDbType.VarChar));
                //paramsUpdate.Add("@creditoWeb", "", base.ToDbType(SqlDbType.VarChar));
                //paramsUpdate.Add("@cli_id_contacto", 1, base.ToDbType(SqlDbType.Int)); // SqlDbType.Int

                // Email
                paramsUpdate.Add("@cli_email", customer.Email, base.ToDbType(SqlDbType.VarChar));
                paramsUpdate.Add("@correo_notificaciones", customer.Email, base.ToDbType(SqlDbType.VarChar));
                paramsUpdate.Add("@cli_REPmail", customer.Email, base.ToDbType(SqlDbType.VarChar));

                // Se agregó una propiedad  BillingAddress.Email  que apunta a ésta columna 
                // para que forme parte de los datos de Facturación, y ya NO de los datos básicos.
                ////paramsUpdate.Add("@Cli_FacturaMail", customer.Email, base.ToDbType(SqlDbType.VarChar));

                //paramsUpdate.Add("@cli_compra", customer.ShoppingName.ToUpper(), base.ToDbType(SqlDbType.VarChar));  
                //paramsUpdate.Add("@Cli_CompraApellido", customer.ShoppingFirstName.ToUpper(), base.ToDbType(SqlDbType.VarChar)); 
                paramsUpdate.Add("@cli_giro", customer.KeyTurn, base.ToDbType(SqlDbType.TinyInt));
                //paramsUpdate.Add("@Cli_Medio", customer.MeansOfContact, base.ToDbType(SqlDbType.TinyInt));

                //paramsUpdate.Add("@cli_tel", customer.ShoppingPhoneNumber, base.ToDbType(SqlDbType.VarChar));
                //paramsUpdate.Add("@cli_tel1", customer.ShoppingPhoneNumber, base.ToDbType(SqlDbType.VarChar));
                //paramsUpdate.Add("@Cli_ComprasCel", customer.ShoppingPhoneNumber, base.ToDbType(SqlDbType.VarChar));

                #endregion

                #region " Ensamblar Query " 

                List<string> listaFiltros =
                    new List<string>();
                if (customer.CustomerKey.Trim() != "") listaFiltros.Add("cli_clave = '" + _valorCustomerKeyFiltro + "'");
                if (customer.CustomerKey.Trim() != "") listaFiltros.Add("cli_cvematriz = '" + _valorCustomerKeyFiltro + "'");
                //if (customer.IdCustomer.Trim() != "") listaFiltros.Add("cli_claveExterna = '" + customer.IdCustomer.Trim() + "'");
                string strFiltro =
                    //string.Join(" AND ", listaFiltros.ToArray()); 
                    //string.Join(" OR ", listaFiltros.ToArray()); 
                    string.Join(" OR ", listaFiltros.ToArray());


                List<string> _ParameterUpdateExpressions =
                    paramsUpdate.ParameterNames.Select(x => x.Trim() + " = @" + x.Trim()).ToList();

                string strQueryUpdateParametrizado = ""
                    + " update inctclie  set "
                    + string.Join(", ", _ParameterUpdateExpressions)

                    + " where ( " + strFiltro + " ) "
                    ;

                #endregion  

                int executeUpdateCustomer =
                    await this._EjecutorSql.ExecuteAsync(strQueryUpdateParametrizado, paramsUpdate, commandType: CommandType.Text);

                Customer updatedCustomer =
                    await this._GetCustomerAsync(customer.CustomerKey, customer.IdCustomer);


                if (dbTransaction != null) { await dbTransaction.CommitAsync(); }
                if (dbConn != null) { await dbConn.CloseAsync(); }

                return
                    updatedCustomer;
            }
            catch (Exception ex)
            {
                if (dbTransaction != null) { await dbTransaction.RollbackAsync(); }
                if (dbConn != null) { await dbConn.CloseAsync(); }

                throw new Exception(ex.Message);
            }
        }

        #endregion 

        #region " Update BillingAddress "

        // Privado  mientras se define quiénes lo van a requerir
        public
            async Task<Customer> _UpdateBillingAddressAsync(Customer customer) 
        {
            return
                //await this._UpdateBillingAddressAsync_SinSP(customer); 
                //await this._UpdateBillingAddressAsync_ConSP(customer); 
                await this._UpdateBillingAddressAsync_SinSP(customer);
        }
        private void _UpdateBillingAddressPreValidar(Customer customer) 
        {
            if (customer == null)
                throw new Exception("El Servicio Web NO recibió ningún dato del Cliente para Actualizar.");
            if (customer.BillingAddress == null)
            {
                throw
                    new Exception(
                        "El Servicio Web NO recibió ningún dato referente a Dirección de Facturación"
                        + " para Actualizar (" + nameof(customer.BillingAddress) + " = null)."
                    );
            }

            if (customer.CustomerKey == null) customer.CustomerKey = "";
            if (customer.ParentCustomerKey == null) customer.ParentCustomerKey = "";
            if (customer.CustomerKey.Trim() == ""
                //|| customer.ParentCustomerKey.Trim() == ""
                )
            {
                throw
                    new Exception(
                        ""
                        //+ "Para identificar una Dirección de Facturación se requiere que los datos "
                        + "Para identificar una Dirección de Facturación se requiere un valor de "
                        + nameof(customer.CustomerKey) 
                        //+ " y " + nameof(customer.ParentCustomerKey)
                        //+ ", NO estén en blanco."
                    );
            }
        }
        private async Task<Customer> _UpdateBillingAddressAsync_ConSP(Customer customer) 
        {
            this._UpdateBillingAddressPreValidar(customer);

            SqlConnection? dbConn = await this._EjecutorSql.OpenDbConnectionOrNullIfAlreadyAsync();
            SqlTransaction? dbTransaction = this._EjecutorSql.BeginTransactionOrNullIfCurrent();

            try
            {
                // Si el Registro NO existe, el método lanzará una Excepción, 
                Customer buscarBillingAddress =
                    //await this._GetCustomerAsync(customer.CustomerKey, customer.IdCustomer); 
                    //await this.GetBillingAddressAsync(customer.CustomerKey, customer.IdCustomer); 
                    await this._GetCustomerAsync(customer.CustomerKey, customer.IdCustomer);


                if (customer.BillingAddress.Rfc == null)
                    customer.BillingAddress.Rfc = "";
                if (buscarBillingAddress.BillingAddress.Rfc == null)
                    buscarBillingAddress.BillingAddress.Rfc = "";
                if (customer.BillingAddress.Rfc.Trim().ToUpper()
                    != buscarBillingAddress.BillingAddress.Rfc.Trim().ToUpper())
                {
                    bool _FindCustomerByRfc =
                        await this.FindCustomerByRfcAsync(customer.BillingAddress.Rfc);
                    if (_FindCustomerByRfc)
                        throw new Exception("Ya existe un cliente registrado con el Rfc " + customer.BillingAddress.Rfc);
                }


                // El StoredProcedure 'spRoltecUpdateCustomer' requiere un .ParentCustomerKey  vacío 
                // para indicarle que ejecute Actualización de datos de  BillingAddress,
                // y no de Datos Básicos del Cliente, ...esto no vulnera el Valor de .ParentCustomerKey  en BD 
                customer.ParentCustomerKey = "";


                bool _updateCustomer = await this.UpdateCustomer(customer);

                Customer updatedCustomer =
                    await this._GetCustomerAsync(customer.CustomerKey, customer.IdCustomer);


                if (dbTransaction != null) { await dbTransaction.CommitAsync(); }
                if (dbConn != null) { await dbConn.CloseAsync(); }

                return
                    updatedCustomer;
            }
            catch (Exception ex)
            {
                if (dbTransaction != null) { await dbTransaction.RollbackAsync(); }
                if (dbConn != null) { await dbConn.CloseAsync(); }

                throw new Exception(ex.Message);
            }
        }
        private async Task<Customer> _UpdateBillingAddressAsync_SinSP(Customer customer) 
        {
            this._UpdateBillingAddressPreValidar(customer);

            SqlConnection? dbConn = await this._EjecutorSql.OpenDbConnectionOrNullIfAlreadyAsync();
            SqlTransaction? dbTransaction = this._EjecutorSql.BeginTransactionOrNullIfCurrent();

            try
            {
                // Si el Registro NO existe, el método lanzará una Excepción. 
                Customer buscarBillingAddress =
                    //await this._GetCustomerAsync(customer.CustomerKey, customer.IdCustomer); 
                    //await this.GetBillingAddressAsync(customer.CustomerKey, customer.IdCustomer); 
                    await this._GetCustomerAsync(customer.CustomerKey, customer.IdCustomer);


                if (customer.BillingAddress.Rfc == null)
                    customer.BillingAddress.Rfc = "";
                if (buscarBillingAddress.BillingAddress.Rfc == null)
                    buscarBillingAddress.BillingAddress.Rfc = "";
                if (customer.BillingAddress.Rfc.Trim().ToUpper() 
                    != buscarBillingAddress.BillingAddress.Rfc.Trim().ToUpper() ) 
                {
                    bool _FindCustomerByRfc =
                        await this.FindCustomerByRfcAsync(customer.BillingAddress.Rfc);
                    if (_FindCustomerByRfc)
                        throw new Exception("Ya existe un cliente registrado con el Rfc " + customer.BillingAddress.Rfc);
                }


                if (buscarBillingAddress.ParentCustomerKey == null)
                    buscarBillingAddress.ParentCustomerKey = "";

                // Antes de ejecutar la Actualización, Restaurar el valor Original de .ParentCustomerKey 
                // para evitar que sea modificado arbitrariamente.
                customer.ParentCustomerKey =
                    buscarBillingAddress.ParentCustomerKey;

                DynamicParameters paramsUpdate = new DynamicParameters();

                if (buscarBillingAddress.ParentCustomerKey.Trim() == "")
                {
                    // Cuando .ParentCustomerKey está vacío, entonces ésta será la 1era Dirección de Facturación,
                    // por lo tanto será  ParentCustomer  de sí mismo. 
                    customer.ParentCustomerKey = customer.CustomerKey;

                    // Se requiere Actualizar el valor de .ParentCustomerKey
                    // cuando se registran los datos de la primer .BillingAddress
                    paramsUpdate.Add("@cli_cvematriz", customer.ParentCustomerKey, base.ToDbType(SqlDbType.VarChar));
                }

                #region " Parámetros con Valores de  BillingAddress  para el Query " 

                BillingAddress _billAds =
                    customer.BillingAddress;

                // (DbType?) SqlDbType.VarChar  base.ToDbType(SqlDbType.VarChar)

                //paramsInsert.Add("@idBillingAddress", _billAds.IdBillingAddress, base.ToDbType(SqlDbType.VarChar));
                paramsUpdate.Add("@Cli_FacturaMail", _billAds.Email, base.ToDbType(SqlDbType.VarChar));
                paramsUpdate.Add("@cli_rfc", _billAds.Rfc, base.ToDbType(SqlDbType.VarChar));

                paramsUpdate.Add("@cli_nom", _billAds.Name.ToUpper(), base.ToDbType(SqlDbType.VarChar)); 
                paramsUpdate.Add("@cli_apat", _billAds.FirstName.ToUpper(), base.ToDbType(SqlDbType.VarChar));
                paramsUpdate.Add("@cli_amat", _billAds.LastName.ToUpper(), base.ToDbType(SqlDbType.VarChar));

                paramsUpdate.Add("@cli_nombre", _billAds.FullName.ToUpper(), base.ToDbType(SqlDbType.VarChar)); 
                paramsUpdate.Add("@cli_nomComercial", _billAds.FullName.ToUpper(), base.ToDbType(SqlDbType.VarChar)); 

                paramsUpdate.Add("@cli_direc", _billAds.Street.ToUpper(), base.ToDbType(SqlDbType.VarChar));  // varchar(80) 
                paramsUpdate.Add("@cli_colonia", _billAds.Colony.ToUpper(), base.ToDbType(SqlDbType.VarChar));  // varchar(50) 
                paramsUpdate.Add("@cli_cp", _billAds.ZipCode, base.ToDbType(SqlDbType.VarChar));

                //paramsUpdate.Add("@cveUsoCFDI", _billAds.CfdiUsageKey, base.ToDbType(SqlDbType.VarChar));  // varchar(3) 
                //paramsUpdate.Add("@cveFormaPago", _billAds.PaymentTypeKey, base.ToDbType(SqlDbType.VarChar));  // varchar(2) 
                //paramsUpdate.Add("@cveMetodoPago", _billAds.PaymentMethodKey, base.ToDbType(SqlDbType.VarChar));  // varchar(3) 
                //paramsUpdate.Add("@CveRegimenFiscal", _billAds.TaxRegime, base.ToDbType(SqlDbType.VarChar));  // varchar(3) 

                #endregion

                #region " Ensamblar Query " 

                List<string> listaFiltros =
                    new List<string>();
                if (customer.CustomerKey.Trim() != "") listaFiltros.Add("cli_clave = '" + customer.CustomerKey.Trim() + "'");
                if (customer.IdCustomer.Trim() != "") listaFiltros.Add("cli_claveExterna = '" + customer.IdCustomer.Trim() + "'");
                string strFiltro =
                    string.Join(" and ", listaFiltros.ToArray());


                List<string> _ParameterUpdateExpressions =
                    paramsUpdate.ParameterNames.Select(x => x.Trim() + " = @" + x.Trim()).ToList();

                string strQueryUpdateParametrizado = ""
                    + " update inctclie  set "
                    + string.Join(", ", _ParameterUpdateExpressions)

                    + " where " + strFiltro
                    ;

                #endregion  

                int executeUpdateCustomer =
                    await this._EjecutorSql.ExecuteAsync(strQueryUpdateParametrizado, paramsUpdate, commandType: CommandType.Text);

                Customer updatedCustomer =
                    await this._GetCustomerAsync(customer.CustomerKey, customer.IdCustomer);


                if (dbTransaction != null) { await dbTransaction.RollbackAsync(); }
                //if (dbTransaction != null) { await dbTransaction.CommitAsync(); }
                if (dbConn != null) { await dbConn.CloseAsync(); }

                return
                    updatedCustomer;
            }
            catch (Exception ex)
            {
                if (dbTransaction != null) { await dbTransaction.RollbackAsync(); }
                if (dbConn != null) { await dbConn.CloseAsync(); }

                throw new Exception(ex.Message);
            }
        }

        #endregion 


        #region " Búsquedas para Validación " 

        public async Task<bool> FindCustomerByIdAsync(string idCustomer) 
        {
            if (idCustomer == null) idCustomer = "";
            if (idCustomer.Trim() == "") throw new Exception("NO proporcionó ningún idCustomer");

            SqlConnection? dbConn = await this._EjecutorSql.OpenDbConnectionOrNullIfAlreadyAsync();
            SqlTransaction? dbTransaction = this._EjecutorSql.BeginTransactionOrNullIfCurrent();

            try
            {
                CustomerResponse? customerResponse = null;

                #region " Query Auto-Mappeado " 

                var aliasColumnPairs =
                    new Dictionary<string, string>();

                aliasColumnPairs[nameof(customerResponse.CustomerKey)] = "cli_clave";
                aliasColumnPairs[nameof(customerResponse.IdCustomer)] = "cli_claveExterna";
                aliasColumnPairs[nameof(customerResponse.ParentCustomerKey)] = "cli_cvematriz";
                //aliasColumnPairs[nameof(customerResponse.AgentKey)] = "cli_agente";

                string[] arrColumnasMappeadas =
                    aliasColumnPairs
                    .Select(x => x.Value + " as [" + x.Key + "]")
                    .ToArray()
                    ;

                string strQueryAutoEnsamblado = ""
                    + " select "
                    + string.Join(", ", arrColumnasMappeadas)
                    + " from inctclie (nolock) "
                    + " where cli_claveExterna = '" + idCustomer.Trim() + "' "
                    ;

                #endregion

                customerResponse =
                    await this._EjecutorSql.QueryFirstOrDefaultAsync<CustomerResponse>(strQueryAutoEnsamblado);


                if (dbTransaction != null) { await dbTransaction.CommitAsync(); }
                if (dbConn != null) { await dbConn.CloseAsync(); }

                return
                    (customerResponse != null);
            }
            catch (Exception ex)
            {
                if (dbTransaction != null) { await dbTransaction.RollbackAsync(); }
                if (dbConn != null) { await dbConn.CloseAsync(); }

                throw new Exception(ex.Message);
            }
        }

        public async Task<bool> FindCustomerByRfcAsync(string rfc) 
        {
            if (rfc == null) rfc = "";
            if (rfc.Trim() == "") throw new Exception("NO proporcionó ningún RFC.");

            SqlConnection? dbConn = await this._EjecutorSql.OpenDbConnectionOrNullIfAlreadyAsync();
            SqlTransaction? dbTransaction = this._EjecutorSql.BeginTransactionOrNullIfCurrent();

            try
            {
                string defaultIgnorarRfc = "XAXX010101000";

                CustomerResponse? customerResponse = null;

                #region " Query Auto-Mappeado " 

                var aliasColumnPairs =
                    new Dictionary<string, string>();

                aliasColumnPairs[nameof(customerResponse.CustomerKey)] = "cli_clave";
                aliasColumnPairs[nameof(customerResponse.IdCustomer)] = "cli_claveExterna";
                aliasColumnPairs[nameof(customerResponse.ParentCustomerKey)] = "cli_cvematriz";
                //aliasColumnPairs[nameof(customerResponse.AgentKey)] = "cli_agente";

                string[] arrColumnasMappeadas =
                    aliasColumnPairs
                    .Select(x => x.Value + " as [" + x.Key + "]")
                    .ToArray()
                    ;

                string strQueryAutoEnsamblado = ""
                    + " select "
                    + string.Join(", ", arrColumnasMappeadas)
                    + " FROM inctclie (nolock) "
                    + " WHERE  UPPER(trim(cli_rfc)) = '" + rfc.Trim().ToUpper() + "' "
                    + " AND  UPPER(trim(cli_rfc)) <> '" + defaultIgnorarRfc.Trim().ToUpper() + "' "
                    ;

                #endregion

                customerResponse =
                    await this._EjecutorSql.QueryFirstOrDefaultAsync<CustomerResponse>(strQueryAutoEnsamblado);


                if (dbTransaction != null) { await dbTransaction.CommitAsync(); }
                if (dbConn != null) { await dbConn.CloseAsync(); }

                return
                    (customerResponse != null);
            }
            catch (Exception ex)
            {
                if (dbTransaction != null) { await dbTransaction.RollbackAsync(); }
                if (dbConn != null) { await dbConn.CloseAsync(); }

                throw new Exception(ex.Message);
            }
        }

        #endregion  

        #region " Get ( Customer y BillingAddress ), con SobreCargas " 

        //private async Task<BillingAddress> GetBillingAddressByCustomerKeyAsync(string customerKey) { return await this.GetBillingAddressAsync(customerKey, ""); }
        //private async Task<BillingAddress> GetBillingAddressByIdCustomerAsync(string idCustomer) { return await this.GetBillingAddressAsync("", idCustomer); }
        private async Task<BillingAddress> GetBillingAddressAsync(string customerKey, string idCustomer) 
        {
            if (idCustomer == null) idCustomer = "";
            if (customerKey == null) customerKey = "";
            if (idCustomer.Trim() == "" && customerKey.Trim() == "")
                throw new Exception("NO proporcionó ninguna Clave ni Id de Cliente.");

            SqlConnection? dbConn = await this._EjecutorSql.OpenDbConnectionOrNullIfAlreadyAsync();
            SqlTransaction? dbTransaction = this._EjecutorSql.BeginTransactionOrNullIfCurrent();

            try
            {
                BillingAddress? billingAddress = new BillingAddress();

                #region " Query Auto-Mappeado " 

                string columnaQueryCompanyName =
                    "(case  when (len(trim(cli_rfc)) = 13)  then ''  else cli_DenominacionSocial  end)";
                string columnaQueryFullName =
                    //"(case  when (len(trim(cli_rfc)) = 13)  then cli_DenominacionSocial  else ''  end)"; 
                    "(case  when (trim(isnull(cli_nombre, '')) = '')  then cli_nomComercial  else cli_nombre  end)";

                var aliasColumnPairs =
                    new Dictionary<string, string>();

                aliasColumnPairs[nameof(billingAddress.IdBillingAddress)] = "idBillingAddress";

                aliasColumnPairs[nameof(billingAddress.Name)] = "cli_nom";
                aliasColumnPairs[nameof(billingAddress.FirstName)] = "cli_apat";
                aliasColumnPairs[nameof(billingAddress.LastName)] = "cli_amat";

                aliasColumnPairs[nameof(billingAddress.Street)] = "cli_direc";
                aliasColumnPairs[nameof(billingAddress.Colony)] = "cli_colonia";
                aliasColumnPairs[nameof(billingAddress.ZipCode)] = "cli_cp";

                aliasColumnPairs[nameof(billingAddress.CfdiUsageKey)] = "cveUsoCFDI";
                aliasColumnPairs[nameof(billingAddress.PaymentTypeKey)] = "cveFormaPago";
                aliasColumnPairs[nameof(billingAddress.PaymentMethodKey)] = "cveMetodoPago";
                aliasColumnPairs[nameof(billingAddress.TaxRegime)] = "CveRegimenFiscal";

                aliasColumnPairs[nameof(billingAddress.Rfc)] = "cli_rfc";
                //aliasColumnPairs[nameof(billingAddress.CompanyName)] = "cli_DenominacionSocial";
                aliasColumnPairs[nameof(billingAddress.CompanyName)] = columnaQueryCompanyName;
                //aliasColumnPairs[nameof(billingAddress.FullName)] = "cli_nombre";
                //aliasColumnPairs[nameof(billingAddress.FullName)] = "cli_nomComercial";
                //aliasColumnPairs[nameof(billingAddress.FullName)] = "cli_DenominacionSocial";
                aliasColumnPairs[nameof(billingAddress.FullName)] = columnaQueryFullName;


                List<string> listaFiltros =
                    new List<string>();
                if (customerKey.Trim() != "") listaFiltros.Add("cli_clave = '" + customerKey.Trim() + "'");
                if (idCustomer.Trim() != "") listaFiltros.Add("cli_claveExterna = '" + idCustomer.Trim() + "'");
                string strFiltro =
                    string.Join(" and ", listaFiltros.ToArray());


                string[] arrColumnasMappeadas =
                    aliasColumnPairs
                    .Select(x => x.Value + " as [" + x.Key + "]")
                    .ToArray()
                    ;

                string strQueryAutoEnsamblado = ""
                    + " select "
                    + string.Join(", ", arrColumnasMappeadas)
                    + " from inctclie (nolock) "

                    + " where " + strFiltro
                    ;

                #endregion  

                billingAddress =
                    await this._EjecutorSql.QuerySingleOrDefaultAsync<BillingAddress>(strQueryAutoEnsamblado);
                if (billingAddress == null)
                {
                    throw
                        new Exception("NO se encontró ninguna Dirección de Facturación"
                        + " con el CustomerKey y/o IdCustomer especificado(s) (" + strFiltro + ").");
                }


                if (dbTransaction != null) { await dbTransaction.CommitAsync(); }
                if (dbConn != null) { await dbConn.CloseAsync(); }

                return
                    billingAddress;
            }
            catch (Exception ex)
            {
                if (dbTransaction != null) { await dbTransaction.RollbackAsync(); }
                if (dbConn != null) { await dbConn.CloseAsync(); }

                throw new Exception(ex.Message);
            }
        }

        public async Task<Customer> GetCustomerByIdAsync(string? idCustomer) 
        {
            if (idCustomer == null) idCustomer = "";
            if (idCustomer.Trim() == "") throw new Exception("NO proporcionó ningún idCustomer");

            return await this._GetCustomerAsync("", idCustomer);
        }
        public async Task<Customer> GetCustomerByCustomerKeyAsync(string? customerKey) 
        {
            if (customerKey == null) customerKey = "";
            if (customerKey.Trim() == "") throw new Exception("NO proporcionó ningún customerKey");

            return await this._GetCustomerAsync(customerKey, "");
        }
        public async Task<Customer> _GetCustomerAsync(string? customerKey, string? idCustomer) 
        {
            if (idCustomer == null) idCustomer = "";
            if (customerKey == null) customerKey = "";
            if (idCustomer.Trim() == "" && customerKey.Trim() == "")
                throw new Exception("NO proporcionó ninguna Clave ni Id de Cliente.");

            SqlConnection? dbConn = await this._EjecutorSql.OpenDbConnectionOrNullIfAlreadyAsync();
            SqlTransaction? dbTransaction = this._EjecutorSql.BeginTransactionOrNullIfCurrent();

            try
            {
                Customer? customer = new Customer();

                #region " Query Auto-Mappeado " 

                string strColumnaTelefono =
                    "(case when (trim(cli_tel1) = '') then Cli_ComprasCel else cli_tel1 end)";
                strColumnaTelefono =
                    //"(case when (trim(cli_tel) = '') then cli_tel1 else cli_tel end)"; 
                    "(case when (trim(cli_tel) = '') then " + strColumnaTelefono + " else cli_tel end)";

                var aliasColumnPairs =
                    new Dictionary<string, string>();

                aliasColumnPairs[nameof(customer.CustomerKey)] = "cli_clave";
                aliasColumnPairs[nameof(customer.IdCustomer)] = "cli_claveExterna";
                aliasColumnPairs[nameof(customer.ParentCustomerKey)] = "cli_cvematriz";

                aliasColumnPairs[nameof(customer.AgentKey)] = "cli_agente";
                aliasColumnPairs[nameof(customer.ZoneCode)] = "cli_ncar";

                aliasColumnPairs[nameof(customer.CreditDays)] = "cli_dias";
                aliasColumnPairs[nameof(customer.Email)] = "cli_email";
                aliasColumnPairs[nameof(customer.ShoppingName)] = "cli_compra";
                aliasColumnPairs[nameof(customer.ShoppingFirstName)] = "Cli_CompraApellido";
                aliasColumnPairs[nameof(customer.KeyTurn)] = "cli_giro";
                aliasColumnPairs[nameof(customer.MeansOfContact)] = "Cli_Medio";

                //aliasColumnPairs[nameof(customer.ShoppingPhoneNumber)] = "cli_tel";
                aliasColumnPairs[nameof(customer.ShoppingPhoneNumber)] = strColumnaTelefono;


                List<string> listaFiltros =
                    new List<string>();
                string strFiltroCustomerKey =
                    (customerKey.Trim() == "")
                    ? "( trim( cli_cvematriz ) = '' or trim( cli_cvematriz ) = trim( cli_clave ) )"
                    : ("cli_clave = '" + customerKey.Trim() + "'")
                    ;
                listaFiltros.Add(strFiltroCustomerKey);
                if (idCustomer.Trim() != "") listaFiltros.Add("cli_claveExterna = '" + idCustomer.Trim() + "'");
                string strFiltro =
                    string.Join(" and ", listaFiltros.ToArray());


                string[] arrColumnasMappeadas =
                    aliasColumnPairs
                    .Select(x => x.Value + " as [" + x.Key + "]")
                    .ToArray()
                    ;

                string strQueryAutoEnsamblado = ""
                    + " select "
                    + string.Join(", ", arrColumnasMappeadas)
                    + " from inctclie (nolock) "

                    //+ " where cli_claveExterna = '" + idCustomer.Trim() + "' "
                    + " where " + strFiltro
                    ;

                #endregion  

                customer =
                    await this._EjecutorSql.QuerySingleOrDefaultAsync<Customer>(strQueryAutoEnsamblado);
                if (customer == null)
                {
                    strFiltro = strFiltro
                        .Replace(aliasColumnPairs[nameof(customer.ParentCustomerKey)], nameof(customer.ParentCustomerKey))
                        .Replace(aliasColumnPairs[nameof(customer.IdCustomer)], nameof(customer.IdCustomer))
                        .Replace(aliasColumnPairs[nameof(customer.CustomerKey)], nameof(customer.CustomerKey))
                        ;
                    throw
                        new Exception("NO se encontró ningún Cliente ó Dirección de Facturación"
                        + " con el CustomerKey y/o IdCustomer especificado(s) (" + strFiltro + ").");
                }

                //customer.BillingAddress = new BillingAddress();
                customer.BillingAddress = await this.GetBillingAddressAsync(customer.CustomerKey, customer.IdCustomer);
                //customer.ShippingAddress = new ShippingAddress();


                if (dbTransaction != null) { await dbTransaction.CommitAsync(); }
                if (dbConn != null) { await dbConn.CloseAsync(); }

                return
                    customer;
            }
            catch (Exception ex)
            {
                if (dbTransaction != null) { await dbTransaction.RollbackAsync(); }
                if (dbConn != null) { await dbConn.CloseAsync(); }

                throw new Exception(ex.Message);
            }
        }

        #endregion 


        #region " ShippingAddress " 

        public async Task<ShippingAddress> GetShippingAddressAsync(string shippingAddressId) 
        {
            SqlConnection? dbConn = await this._EjecutorSql.OpenDbConnectionOrNullIfAlreadyAsync();
            SqlTransaction? dbTransaction = this._EjecutorSql.BeginTransactionOrNullIfCurrent();

            try
            {
                ShippingAddress shippingAddress = 
                    await this._GetShippingAddressOrNullAsync(shippingAddressId);
                if (shippingAddress == null)
                {
                    throw
                        new Exception("NO se encontró ninguna Dirección de Envío con el "
                        + nameof(shippingAddressId) + " '" + shippingAddressId.Trim() + "'.");
                }
                if (shippingAddress.Street == null) shippingAddress.Street = "";
                if (shippingAddress.Colony == null) shippingAddress.Colony = "";
                if (shippingAddress.FullAddress == null) shippingAddress.FullAddress = "";


                if (dbTransaction != null) { await dbTransaction.CommitAsync(); }
                if (dbConn != null) { await dbConn.CloseAsync(); }

                return
                    shippingAddress;
            }
            catch (Exception ex)
            {
                if (dbTransaction != null) { await dbTransaction.RollbackAsync(); }
                if (dbConn != null) { await dbConn.CloseAsync(); }

                throw new Exception(ex.Message);
            }
        }
        private async Task<ShippingAddress> _GetShippingAddressOrNullAsync(string shippingAddressId) 
        {
            if (shippingAddressId == null) shippingAddressId = "";
            if (shippingAddressId.Trim() == "") throw new Exception("NO proporcionó ningún " + nameof(shippingAddressId));

            SqlConnection? dbConn = await this._EjecutorSql.OpenDbConnectionOrNullIfAlreadyAsync();
            SqlTransaction? dbTransaction = this._EjecutorSql.BeginTransactionOrNullIfCurrent();

            try
            {
                ShippingAddress shippingAddress = new ShippingAddress();

                #region " Query Auto-Mappeado " 

                var aliasColumnPairs =
                    new Dictionary<string, string>();

                aliasColumnPairs[nameof(shippingAddress.CustomerKey)] = "Cliente";
                aliasColumnPairs[nameof(shippingAddress.ShippingAddressId)] = "IDSuc";
                aliasColumnPairs[nameof(shippingAddress.Alias)] = "NombreSuc";

                aliasColumnPairs[nameof(shippingAddress.FullAddress)] = "Domicilio";
                aliasColumnPairs[nameof(shippingAddress.Phone)] = "tel1";
                aliasColumnPairs[nameof(shippingAddress.ZipCode)] = "CP";
                aliasColumnPairs[nameof(shippingAddress.CityKey)] = "Ciudad";

                string[] arrColumnasMappeadas =
                    aliasColumnPairs
                    .Select(x => x.Value + " as [" + x.Key + "]")
                    .ToArray()
                    ;

                string strQueryAutoEnsamblado = ""
                    + " select "
                    + string.Join(", ", arrColumnasMappeadas)
                    + " from Sucs_Domicilios (nolock) "
                    + " where IDSuc = '" + shippingAddressId.Trim() + "' "
                    ;

                #endregion

                shippingAddress =
                    await this._EjecutorSql.QuerySingleOrDefaultAsync<ShippingAddress>(strQueryAutoEnsamblado);


                if (dbTransaction != null) { await dbTransaction.CommitAsync(); }
                if (dbConn != null) { await dbConn.CloseAsync(); }

                return
                    shippingAddress;
            }
            catch (Exception ex)
            {
                if (dbTransaction != null) { await dbTransaction.RollbackAsync(); }
                if (dbConn != null) { await dbConn.CloseAsync(); }

                throw new Exception(ex.Message);
            }
        }

        public async Task<List<ShippingAddress>> GetShippingAddressListAsync(string? customerKey) 
        {
            if (customerKey == null) customerKey = "";
            if (customerKey.Trim() == "") throw new Exception("NO proporcionó ningún " + nameof(customerKey));

            SqlConnection? dbConn = await this._EjecutorSql.OpenDbConnectionOrNullIfAlreadyAsync();
            SqlTransaction? dbTransaction = this._EjecutorSql.BeginTransactionOrNullIfCurrent();

            try
            {
                ShippingAddress shippingAddress = new ShippingAddress();
                List<ShippingAddress> shippingAddressList = new List<ShippingAddress>();

                #region " Query Auto-Mappeado " 

                var aliasColumnPairs =
                    new Dictionary<string, string>();

                aliasColumnPairs[nameof(shippingAddress.CustomerKey)] = "Cliente";
                aliasColumnPairs[nameof(shippingAddress.ShippingAddressId)] = "IDSuc";
                aliasColumnPairs[nameof(shippingAddress.Alias)] = "NombreSuc";

                aliasColumnPairs[nameof(shippingAddress.FullAddress)] = "Domicilio";
                aliasColumnPairs[nameof(shippingAddress.Phone)] = "tel1";
                aliasColumnPairs[nameof(shippingAddress.ZipCode)] = "CP";
                aliasColumnPairs[nameof(shippingAddress.CityKey)] = "Ciudad";

                string[] arrColumnasMappeadas =
                    aliasColumnPairs
                    .Select(x => x.Value + " as [" + x.Key + "]")
                    .ToArray()
                    ;

                string strQueryAutoEnsamblado = ""
                    + " select "
                    + string.Join(", ", arrColumnasMappeadas)
                    + " from Sucs_Domicilios (nolock) "

                    + " where trim(Cliente) = '" + customerKey.Trim() + "' "
                    ;

                #endregion

                IEnumerable<ShippingAddress> _IEnumerableShpAds = //shippingAddressList = 
                    await this._EjecutorSql.QueryAsync<ShippingAddress>(strQueryAutoEnsamblado);

                if (_IEnumerableShpAds != null)
                    shippingAddressList = _IEnumerableShpAds.ToList();
                if (shippingAddressList == null)
                    shippingAddressList = new List<ShippingAddress>();


                if (dbTransaction != null) { await dbTransaction.CommitAsync(); }
                if (dbConn != null) { await dbConn.CloseAsync(); }

                return
                    shippingAddressList;
            }
            catch (Exception ex)
            {
                if (dbTransaction != null) { await dbTransaction.RollbackAsync(); }
                if (dbConn != null) { await dbConn.CloseAsync(); }

                throw new Exception(ex.Message);
            }
        }

        public async Task<ShippingAddress> AddShippingAddressAsync(ShippingAddress shippingAddress) 
        {
            if (shippingAddress == null)
                throw new Exception("El Servicio Web NO recibió los datos necesarios para registrar la Dirección de Envío");

            SqlConnection? dbConn = await this._EjecutorSql.OpenDbConnectionOrNullIfAlreadyAsync();
            SqlTransaction? dbTransaction = this._EjecutorSql.BeginTransactionOrNullIfCurrent();

            try
            {
                ShippingAddress buscarShippingAddress =
                    await this._GetShippingAddressOrNullAsync(shippingAddress.ShippingAddressId);
                if (buscarShippingAddress != null)
                {
                    throw
                        new Exception(
                            "Ya existe una Dirección de Envío con el " + nameof(shippingAddress.ShippingAddressId)
                            + " '" + shippingAddress.ShippingAddressId.Trim() + "'"
                        );
                }

                Customer buscarCustomer =
                    await this.GetCustomerByCustomerKeyAsync(shippingAddress.CustomerKey);

                // Evitar que una ShippingAddress quede ligada al registro de una BillingAddress adicional 
                // en vez de quedar ligada al registro de un Cliente
                if (buscarCustomer.ParentCustomerKey == null)
                    buscarCustomer.ParentCustomerKey = "";
                if (buscarCustomer.ParentCustomerKey.Trim() == "")
                    shippingAddress.CustomerKey = buscarCustomer.ParentCustomerKey;

                ShippingAddress insertedShippingAddress =
                    await this._InsertShippingAddressAsync(shippingAddress);


                if (dbTransaction != null) { await dbTransaction.RollbackAsync(); }
                //if (dbTransaction != null) { await dbTransaction.CommitAsync(); }
                if (dbConn != null) { await dbConn.CloseAsync(); }

                return
                    insertedShippingAddress;
            }
            catch (Exception ex)
            {
                if (dbTransaction != null) { await dbTransaction.RollbackAsync(); }
                if (dbConn != null) { await dbConn.CloseAsync(); }

                throw new Exception(ex.Message);
            }
        }
        private
            async Task<ShippingAddress> _InsertShippingAddressAsync(ShippingAddress shippingAddress) 
        {
            return
                //await this._InsertShippingAddressAsync_ConSP(shippingAddress); 
                //await this._InsertShippingAddressAsync_SinSP(shippingAddress); 
                await this._InsertShippingAddressAsync_SinSP(shippingAddress); 
        }
        private async Task<ShippingAddress> _InsertShippingAddressAsync_SinSP(ShippingAddress shippingAddress) 
        {
            if (shippingAddress == null)
                throw new Exception("El Servicio Web NO recibió los datos necesarios para registrar la Dirección de Envío");

            SqlConnection? dbConn = await this._EjecutorSql.OpenDbConnectionOrNullIfAlreadyAsync();
            SqlTransaction? dbTransaction = this._EjecutorSql.BeginTransactionOrNullIfCurrent();

            try
            {
                #region " Query parametrizado ( PARA Insert ) "

                DynamicParameters paramsInsert = new DynamicParameters();

                // (DbType?) SqlDbType.VarChar  base.ToDbType(SqlDbType.VarChar)

                paramsInsert.Add("@Cliente", shippingAddress.CustomerKey.Trim(), base.ToDbType(SqlDbType.VarChar));
                paramsInsert.Add("@IDSuc", shippingAddress.ShippingAddressId, base.ToDbType(SqlDbType.VarChar));//id que genera la pagina
                paramsInsert.Add("@NombreSuc", shippingAddress.Alias, base.ToDbType(SqlDbType.VarChar));//nombre de la sucursal

                if (shippingAddress.Street == null) shippingAddress.Street = "";
                if (shippingAddress.Colony == null) shippingAddress.Colony = "";
                if (shippingAddress.FullAddress == null) shippingAddress.FullAddress = "";

                string _FullAddress = ""
                          + shippingAddress.Street.Trim() + " "
                          + shippingAddress.Colony.Trim()
                          ;
                if (shippingAddress.FullAddress == null)
                    shippingAddress.FullAddress = "";
                if (_FullAddress.Trim() == "") _FullAddress += shippingAddress.FullAddress.Trim();
                shippingAddress.FullAddress = _FullAddress.Trim();

                paramsInsert.Add("@Domicilio", shippingAddress.FullAddress, base.ToDbType(SqlDbType.VarChar));

                paramsInsert.Add("@tel1", shippingAddress.Phone, base.ToDbType(SqlDbType.VarChar));
                paramsInsert.Add("@CP", shippingAddress.ZipCode, base.ToDbType(SqlDbType.VarChar));
                paramsInsert.Add("@Ciudad", shippingAddress.CityKey, base.ToDbType(SqlDbType.VarChar));

                /* 
                CREATE TABLE [dbo].[Sucs_Domicilios](
                    [Cliente] [varchar](5) NOT NULL,
                        [REGION] [varchar](3) NOT NULL,
                        [Consecutivo] [smallint] NOT NULL,
                    [IDSuc] [varchar](5) NOT NULL,
                    [NombreSuc] [varchar](140) NOT NULL,
                    [Domicilio] [varchar](120) NOT NULL,
                    [Ciudad] [varchar](4) NOT NULL,
                    [CP] [varchar](5) NOT NULL,
                    [Tel1] [varchar](20) NOT NULL,
                        [Tel2] [varchar](20) NOT NULL,
                        [Status] [char](1) NOT NULL
                ) ON [PRIMARY]

                // */

                List<string> _ParameterNames =
                    paramsInsert.ParameterNames.Select(x => "@" + x).ToList();
                //_ParameterNames.Add("@ord_magento");
                string strParametrosConcatenados =
                    string.Join(", ", _ParameterNames);

                string strQueryInsertParametrizado = ""
                    + " insert into Sucs_Domicilios ( " + strParametrosConcatenados.Replace("@", "") + " ) "
                    + " values ( " + strParametrosConcatenados + " ) "
                    ;

                #endregion

                int executeInsertShpAds =
                    await this._EjecutorSql.ExecuteAsync(strQueryInsertParametrizado, paramsInsert, commandType: CommandType.Text);

                ShippingAddress insertedShippingAddress =
                    await this.GetShippingAddressAsync(shippingAddress.ShippingAddressId);


                if (dbTransaction != null) { await dbTransaction.RollbackAsync(); }
                //if (dbTransaction != null) { await dbTransaction.CommitAsync(); }
                if (dbConn != null) { await dbConn.CloseAsync(); }

                return
                    insertedShippingAddress;
            }
            catch (Exception ex)
            {
                if (dbTransaction != null) { await dbTransaction.RollbackAsync(); }
                if (dbConn != null) { await dbConn.CloseAsync(); }

                throw new Exception(ex.Message);
            }
        }

        public async Task<ShippingAddress> UpdateShippingAddressAsync(ShippingAddress shippingAddress) 
        {
            if (shippingAddress == null)
                throw new Exception("El Servicio Web NO recibió ningún dato de la Dirección de Envío para ser Actualizado."); 

            SqlConnection? dbConn = await this._EjecutorSql.OpenDbConnectionOrNullIfAlreadyAsync();
            SqlTransaction? dbTransaction = this._EjecutorSql.BeginTransactionOrNullIfCurrent();

            try
            {
                //Si el .ShippingAddressId  NO existe, el método 'GetShippingAddressAsync()' lanza una Excepción. 
                ShippingAddress buscarShippingAddress =
                    await this.GetShippingAddressAsync(shippingAddress.ShippingAddressId);

                // Proteger Campos que NO puedan ser Modificados 
                //shippingAddress.CustomerKey = buscarShippingAddress.CustomerKey;

                Customer buscarCustomer =
                    await this.GetCustomerByCustomerKeyAsync(shippingAddress.CustomerKey);

                // Evitar que una ShippingAddress quede ligada al registro de una BillingAddress adicional 
                // en vez de quedar ligada al registro de un Cliente
                if (buscarCustomer.ParentCustomerKey == null)
                    buscarCustomer.ParentCustomerKey = "";
                if (buscarCustomer.ParentCustomerKey.Trim() == "")
                    shippingAddress.CustomerKey = buscarCustomer.ParentCustomerKey;

                #region " Query parametrizado ( PARA Update ) "

                DynamicParameters paramsUpdate = new DynamicParameters();

                // (DbType?) SqlDbType.VarChar  base.ToDbType(SqlDbType.VarChar)

                //paramsUpdate.Add("@IDSuc", shippingAddress.ShippingAddressId, base.ToDbType(SqlDbType.VarChar)); 
                paramsUpdate.Add("@Cliente", shippingAddress.CustomerKey.Trim(), base.ToDbType(SqlDbType.VarChar));
                paramsUpdate.Add("@NombreSuc", shippingAddress.Alias, base.ToDbType(SqlDbType.VarChar));//nombre de la sucursal

                if (shippingAddress.Street == null) shippingAddress.Street = "";
                if (shippingAddress.Colony == null) shippingAddress.Colony = "";
                if (shippingAddress.FullAddress == null) shippingAddress.FullAddress = "";

                string _FullAddress = ""
                          + shippingAddress.Street.Trim() + " "
                          + shippingAddress.Colony.Trim()
                          ;
                if (shippingAddress.FullAddress == null)
                    shippingAddress.FullAddress = "";
                if (_FullAddress.Trim() == "") _FullAddress += shippingAddress.FullAddress.Trim();
                shippingAddress.FullAddress = _FullAddress.Trim();

                paramsUpdate.Add("@Domicilio", shippingAddress.FullAddress, base.ToDbType(SqlDbType.VarChar));

                paramsUpdate.Add("@tel1", shippingAddress.Phone, base.ToDbType(SqlDbType.VarChar));
                paramsUpdate.Add("@CP", shippingAddress.ZipCode, base.ToDbType(SqlDbType.VarChar));
                paramsUpdate.Add("@Ciudad", shippingAddress.CityKey, base.ToDbType(SqlDbType.VarChar));


                List<string> _ParameterUpdateExpressions =
                    paramsUpdate.ParameterNames.Select(x => x.Trim() + " = @" + x.Trim()).ToList();

                string strQueryUpdateParametrizado = ""
                    + " update Sucs_Domicilios  set "
                    + string.Join(", ", _ParameterUpdateExpressions)
                    + " where IDSuc = '" + shippingAddress.ShippingAddressId.Trim() + "'"
                    ;

                #endregion 

                int executeUpdateShpAds =
                    await this._EjecutorSql.ExecuteAsync(strQueryUpdateParametrizado, paramsUpdate, commandType: CommandType.Text);

                ShippingAddress updatedShippingAddress = 
                    await this.GetShippingAddressAsync(shippingAddress.ShippingAddressId);


                if (dbTransaction != null) { await dbTransaction.RollbackAsync(); }
                //if (dbTransaction != null) { await dbTransaction.CommitAsync(); }
                if (dbConn != null) { await dbConn.CloseAsync(); }

                return
                    updatedShippingAddress;
            }
            catch (Exception ex)
            {
                if (dbTransaction != null) { await dbTransaction.RollbackAsync(); }
                if (dbConn != null) { await dbConn.CloseAsync(); }

                throw new Exception(ex.Message);
            }
        }

        private ShippingAddress? ToShippingAddress(BillingAddress? billingAddress) 
        {
            if (billingAddress == null) return null;

            if (billingAddress.FullName == null) billingAddress.FullName = "";
            if (billingAddress.CompanyName == null) billingAddress.CompanyName = "";

            return
                new ShippingAddress() 
                {
                    CustomerKey = "Customer.CustomerKey",
                    ShippingAddressId = "PENDIENTE",
                    Phone = "PENDIENTE",

                    Alias = (
                        billingAddress.CompanyName.Trim()
                        + " " + billingAddress.FullName.Trim()
                    ).Trim(),

                    Street = billingAddress.Street,
                    Colony = billingAddress.Colony,

                    CityKey = billingAddress.CityKey,
                    ZipCode = billingAddress.ZipCode,
                };
        }

        private async Task _TryCatchAsync(Task task) 
        {
            SqlConnection? dbConn = await this._EjecutorSql.OpenDbConnectionOrNullIfAlreadyAsync();
            SqlTransaction? dbTransaction = this._EjecutorSql.BeginTransactionOrNullIfCurrent();

            try
            {
                await task;
                //T _awaitTask = await task;

                if (dbTransaction != null) { await dbTransaction.CommitAsync(); }
                if (dbConn != null) { await dbConn.CloseAsync(); }

                //return _awaitTask;
            }
            catch (Exception ex)
            {
                if (dbTransaction != null) { await dbTransaction.RollbackAsync(); }
                if (dbConn != null) { await dbConn.CloseAsync(); }

                throw new Exception(ex.Message);
            }
        }
        private async Task<T> _TryCatchAsync<T>(Task<T> task) 
        {
            SqlConnection? dbConn = await this._EjecutorSql.OpenDbConnectionOrNullIfAlreadyAsync();
            SqlTransaction? dbTransaction = this._EjecutorSql.BeginTransactionOrNullIfCurrent();

            try
            {
                T _awaitTask = 
                    await task; 

                if (dbTransaction != null) { await dbTransaction.CommitAsync(); }
                if (dbConn != null) { await dbConn.CloseAsync(); }

                return 
                    _awaitTask;
            }
            catch (Exception ex)
            {
                if (dbTransaction != null) { await dbTransaction.RollbackAsync(); }
                if (dbConn != null) { await dbConn.CloseAsync(); }

                throw new Exception(ex.Message);
            }
        }

        #endregion 

        //-------------------------------------------------------


        private async Task<bool> AddCustomer(Customer customer) 
        {
            if (customer == null)
                throw new Exception("El Servicio Web NO recibió ningún dato para registrar al Cliente.");

            SqlConnection? dbConn = await this._EjecutorSql.OpenDbConnectionOrNullIfAlreadyAsync();
            SqlTransaction? dbTransaction = this._EjecutorSql.BeginTransactionOrNullIfCurrent();

            try
            {
                DynamicParameters parameters = new DynamicParameters();

                // (DbType?) SqlDbType.VarChar  base.ToDbType(SqlDbType.VarChar)

                #region " Parámetros del StoredProcedure  ( valores de Datos Básicos ) " 

                parameters.Add("@cve_cliente", customer.IdCustomer, base.ToDbType(SqlDbType.VarChar));
                parameters.Add("@cli_cvematriz", customer.ParentCustomerKey, base.ToDbType(SqlDbType.VarChar));

                parameters.Add("@nombreCompra", customer.ShoppingName.ToUpper(), base.ToDbType(SqlDbType.VarChar));  // cli_compra
                parameters.Add("@apellidoCompra", customer.ShoppingFirstName.ToUpper(), base.ToDbType(SqlDbType.VarChar)); // Cli_CompraApellido
                parameters.Add("@telefono", customer.ShoppingPhoneNumber, base.ToDbType(SqlDbType.VarChar));
                parameters.Add("@Cli_ComprasCel", customer.ShoppingPhoneNumber, base.ToDbType(SqlDbType.VarChar));
                parameters.Add("@pais", "Mexico", base.ToDbType(SqlDbType.VarChar));

                parameters.Add("@cli_email", customer.Email, base.ToDbType(SqlDbType.VarChar));
                parameters.Add("@giro_cve", customer.KeyTurn, base.ToDbType(SqlDbType.TinyInt));
                parameters.Add("@cli_medio", customer.MeansOfContact, base.ToDbType(SqlDbType.Int)); //TODO: Cambiar a  DbType.Byte 
                parameters.Add("@credito", customer.Credit, base.ToDbType(SqlDbType.VarChar)); //TODO: el SP 'RoltecAddCustomer' recibe Este parámetro pero no lo utiliza para nada 
                parameters.Add("@credito_dias", customer.CreditDays, base.ToDbType(SqlDbType.Int)); //TODO: Cambiar a  DbType.Int16 

                #endregion 

                #region " Parámetros del StoredProcedure  ( valores de BillingAddress ) " 

                parameters.Add("@billingAddressId", customer.BillingAddress.IdBillingAddress, base.ToDbType(SqlDbType.VarChar));
                //parameters.Add("@Cli_FacturaMail", customer.BillingAddress.Email, base.ToDbType(SqlDbType.VarChar)); 
                parameters.Add("@cli_nom", customer.BillingAddress.Name.ToUpper(), base.ToDbType(SqlDbType.VarChar)); // cli_nom 
                parameters.Add("@cli_apat", customer.BillingAddress.FirstName.ToUpper(), base.ToDbType(SqlDbType.VarChar));
                parameters.Add("@cli_amat", customer.BillingAddress.LastName.ToUpper(), base.ToDbType(SqlDbType.VarChar));
                parameters.Add("@cli_nombre", customer.BillingAddress.FullName.ToUpper(), base.ToDbType(SqlDbType.VarChar)); // cli_nomComercial, cli_nombre

                parameters.Add("@rfc", customer.BillingAddress.Rfc, base.ToDbType(SqlDbType.VarChar));
                string strDenominacionSocial =
                    (customer.BillingAddress.Rfc.Length == 13)
                    ? customer.BillingAddress.FullName.ToUpper()
                    : customer.BillingAddress.CompanyName
                    ;
                parameters.Add("@cli_DenominacionSocial", strDenominacionSocial, base.ToDbType(SqlDbType.VarChar));

                parameters.Add("@CveRegimenFiscal", customer.BillingAddress.TaxRegime, base.ToDbType(SqlDbType.VarChar));
                parameters.Add("@cve_metodo_pago", customer.BillingAddress.PaymentMethodKey, base.ToDbType(SqlDbType.VarChar));
                parameters.Add("@cve_forma_pago", customer.BillingAddress.PaymentTypeKey, base.ToDbType(SqlDbType.VarChar));
                parameters.Add("@cve_uso_cfdi", customer.BillingAddress.CfdiUsageKey, base.ToDbType(SqlDbType.VarChar));
                parameters.Add("@codigo_postal", customer.BillingAddress.ZipCode, base.ToDbType(SqlDbType.VarChar));
                parameters.Add("@ciudad", customer.BillingAddress.City, base.ToDbType(SqlDbType.VarChar)); //TODO: el SP 'RoltecAddCustomer' recibe Este parámetro pero no lo utiliza para nada 
                parameters.Add("@estado", customer.BillingAddress.State, base.ToDbType(SqlDbType.Int));
                parameters.Add("@calle", customer.BillingAddress.Street.ToUpper(), base.ToDbType(SqlDbType.VarChar));
                parameters.Add("@colonia", customer.BillingAddress.Colony.ToUpper(), base.ToDbType(SqlDbType.VarChar));

                #endregion

                var cust =
                    await this._EjecutorSql.ExecuteAsync(
                        "spRoltecAddCustomer", //"spRoltecAddCustomer", 
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );
                //message


                if (dbTransaction != null) { await dbTransaction.CommitAsync(); }
                if (dbConn != null) { await dbConn.CloseAsync(); }

                return
                    true;
            }
            catch (Exception ex)
            {
                if (dbTransaction != null) { await dbTransaction.RollbackAsync(); }
                if (dbConn != null) { await dbConn.CloseAsync(); }

                throw new Exception(ex.Message);
            }
        }

        #region " Métodos Comentados que YA NO tenían referencias "

        /* 

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
        private async Task<CustomerResponse> GetCustomerResponseById(string idCustomer) //TODO: YA NO tiene referencias, -> DESCONTINUARLO 
        {
            if (idCustomer == null) idCustomer = "";
            if (idCustomer.Trim() == "") throw new Exception("NO proporcionó ningún idCustomer");

            SqlConnection? dbConn = await this._EjecutorSql.OpenDbConnectionOrNullIfAlreadyAsync();
            SqlTransaction? dbTransaction = this._EjecutorSql.BeginTransactionOrNullIfCurrent();

            //CustomerResponse? customerResponse = default;
            try
            {
                CustomerResponse? customerResponse = new CustomerResponse();

                #region " Query Auto-Mappeado " 

                var aliasColumnPairs =
                    new Dictionary<string, string>();

                aliasColumnPairs[nameof(customerResponse.CustomerKey)] = "cli_clave";
                aliasColumnPairs[nameof(customerResponse.IdCustomer)] = "cli_claveExterna";
                aliasColumnPairs[nameof(customerResponse.ParentCustomerKey)] = "cli_cvematriz";
                aliasColumnPairs[nameof(customerResponse.AgentKey)] = "cli_agente";

                string[] arrColumnasMappeadas =
                    aliasColumnPairs
                    .Select(x => x.Value + " as [" + x.Key + "]")
                    .ToArray()
                    ;

                string strQueryAutoEnsamblado = ""
                    + " select "
                    + string.Join(", ", arrColumnasMappeadas)
                    + " from inctclie (nolock) "
                    + " where cli_claveExterna = '" + idCustomer.Trim() + "' "
                    ;

                #endregion  

                customerResponse =
                    await this._EjecutorSql.QuerySingleOrDefaultAsync<CustomerResponse>(strQueryAutoEnsamblado);


                #region " Este Query está correcto pero YA NO SE USA, porque se sustituyó por una versión Auto-Mappeada "

                // Este Query está correcto pero YA NO SE USA, porque se sustituyó por una versión Auto-Mappeada 
                string queryString = ""
                    + " select "
                    + " cli_clave as [CustomerKey], "
                    + " cli_cvematriz as [ParentCustomerKey], "
                    + " cli_agente as [AgentKey], "

                    + " cli_claveExterna as [IdCustomer] "
                    + " from inctclie (nolock) "
                    + " where cli_claveExterna = '" + idCustomer.Trim() + "' "
                    ;
                //customerResponse = await sqlConnection.QueryFirstOrDefaultAsync<CustomerResponse>(queryString);

                #endregion 


                if (dbTransaction != null) { await dbTransaction.CommitAsync(); }
                if (dbConn != null) { await dbConn.CloseAsync(); }

                return 
                    customerResponse;
            }
            catch (Exception ex)
            {
                if (dbTransaction != null) { await dbTransaction.RollbackAsync(); }
                if (dbConn != null) { await dbConn.CloseAsync(); }

                throw new Exception(ex.Message);
            }
        }

        public async Task<IEnumerable<Customer>> GetCustomersList() // SIN REFERENCIAS 
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
        private async Task<bool> UpdateCustomer(Customer customer) // para usar desde DashBoard 
        {
            SqlConnection? dbConn = await this._EjecutorSql.OpenDbConnectionOrNullIfAlreadyAsync();
            SqlTransaction? dbTransaction = this._EjecutorSql.BeginTransactionOrNullIfCurrent();

            try
            {
                DynamicParameters parameters = new DynamicParameters();

                parameters.Add("@cli_email", customer.Email, (DbType?)SqlDbType.VarChar);
                //parameters.Add("@Cli_FacturaMail", customer.BillingAddress.Email, (DbType?)SqlDbType.VarChar); 

                #region " Parámetros del StoredProcedure  ( valores de Datos Básicos ) " 

                parameters.Add("@customerKey", customer.CustomerKey, (DbType?)SqlDbType.VarChar);
                parameters.Add("@cve_cliente", customer.IdCustomer, (DbType?)SqlDbType.VarChar);
                //parameters.Add("@cli_cvematriz", customer.ParentCustomerKey, (DbType?)SqlDbType.VarChar);  // Este parámetro es sólo para  AddCustomer()
                parameters.Add("@nombreCompra", customer.ShoppingName.ToUpper(), (DbType?)SqlDbType.VarChar);
                parameters.Add("@apellidoCompra", customer.ShoppingFirstName.ToUpper(), (DbType?)SqlDbType.VarChar);

                parameters.Add("@Cli_ComprasCel", customer.ShoppingPhoneNumber, (DbType?)SqlDbType.VarChar);
                parameters.Add("@telefono", customer.ShoppingPhoneNumber, (DbType?)SqlDbType.VarChar);
                parameters.Add("@pais", "Mexico", (DbType?)SqlDbType.VarChar);

                parameters.Add("@giro_cve", customer.KeyTurn, DbType.Byte);
                parameters.Add("@cli_medio", customer.MeansOfContact, (DbType?)SqlDbType.Int);
                parameters.Add("@credito", customer.Credit, (DbType?)SqlDbType.VarChar);  //TODO: el SP 'RoltecUpdateCustomer' recibe Este parámetro pero no lo utiliza para nada
                parameters.Add("@credito_dias", customer.CreditDays, (DbType?)SqlDbType.Int);

                #endregion

                #region " Parámetros del StoredProcedure  ( valores de BillingAddress ) " 

                parameters.Add("@billingAddressId", customer.BillingAddress.IdBillingAddress, (DbType?)SqlDbType.VarChar);  //TODO: el SP 'RoltecUpdateCustomer' recibe Este parámetro pero no lo utiliza para nada
                parameters.Add("@cli_nom", customer.BillingAddress.Name.ToUpper(), (DbType?)SqlDbType.VarChar);
                parameters.Add("@cli_apat", customer.BillingAddress.FirstName.ToUpper(), (DbType?)SqlDbType.VarChar);
                parameters.Add("@cli_amat", customer.BillingAddress.LastName.ToUpper(), (DbType?)SqlDbType.VarChar);
                parameters.Add("@cli_nombre", customer.BillingAddress.FullName.ToUpper(), (DbType?)SqlDbType.VarChar);

                parameters.Add("@rfc", customer.BillingAddress.Rfc, (DbType?)SqlDbType.VarChar);
                string strDenominacionSocial =
                    (customer.BillingAddress.Rfc.Length == 13)
                    ? customer.BillingAddress.FullName.ToUpper()
                    : customer.BillingAddress.CompanyName
                    ;
                parameters.Add("@cli_DenominacionSocial", strDenominacionSocial, (DbType?)SqlDbType.VarChar);

                parameters.Add("@CveRegimenFiscal", customer.BillingAddress.TaxRegime, (DbType?)SqlDbType.VarChar);
                parameters.Add("@cve_metodo_pago", customer.BillingAddress.PaymentMethodKey, (DbType?)SqlDbType.VarChar);
                parameters.Add("@cve_forma_pago", customer.BillingAddress.PaymentTypeKey, (DbType?)SqlDbType.VarChar);
                parameters.Add("@cve_uso_cfdi", customer.BillingAddress.CfdiUsageKey, (DbType?)SqlDbType.VarChar);
                parameters.Add("@codigo_postal", customer.BillingAddress.ZipCode, (DbType?)SqlDbType.VarChar);
                parameters.Add("@ciudad", customer.BillingAddress.City, (DbType?)SqlDbType.VarChar); //TODO: el SP 'RoltecUpdateCustomer' recibe Este parámetro pero no lo utiliza para nada
                parameters.Add("@estado", customer.BillingAddress.State, (DbType?)SqlDbType.Int);
                parameters.Add("@calle", customer.BillingAddress.Street.ToUpper(), (DbType?)SqlDbType.VarChar);
                parameters.Add("@colonia", customer.BillingAddress.Colony.ToUpper(), (DbType?)SqlDbType.VarChar);

                #endregion

                int cust =
                    await this._EjecutorSql.ExecuteAsync(
                        "spRoltecUpdateCustomer", //"spRoltecUpdateCustomer", 
                        parameters, 
                        commandType: CommandType.StoredProcedure
                    );
                //message


                if (dbTransaction != null) { await dbTransaction.CommitAsync(); }
                if (dbConn != null) { await dbConn.CloseAsync(); }

                return
                    true;
            }
            catch (Exception ex)
            {
                if (dbTransaction != null) { await dbTransaction.RollbackAsync(); }
                if (dbConn != null) { await dbConn.CloseAsync(); }

                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Validar si el cliente ya existe en la base de datos con el RFC
        /// </summary>
        /// <param name="customer"></param>
        /// <returns>Primer cliente con el rfc dado</returns>
        /// <exception cref="Exception"></exception>
        private async Task ValidateBillingCustomer(Customer customer) // Ya NO tiene referencias. 
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
                        throw new CustomerCustomException("El RFC del cliente ya esta registrado.");
                    }
                }
            }
            catch (CustomerCustomException customerCustomEx)
            {
                throw customerCustomEx;
            }
            catch (Exception ex)
            {
                //logger
                //throw new Exception(ex.ToString());
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Agregar o actualizar una direccion de envio del cliente
        /// </summary>
        /// <param name="shippingAddress"></param>
        /// <param name="customerId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private async Task<bool> AddShippingAddressToCustomer(ShippingAddress shippingAddress, string customerId)
            // Ya NO tiene referencias. 
        {
            var db = DbConnection();
            await db.OpenAsync();
            // Add paremeters to StoreProcedure
            DynamicParameters parameters = new DynamicParameters();

            // El parámetro 'customerId' es innecesario porque se puede usar  ShippingAddress.IdCustomer, 
            // y además lo correcto sería crear y usar  ShippingAddress.CustomerKey 
            parameters.Add("@cve_cliente", customerId, (DbType?)SqlDbType.VarChar);

            // Ningún parámetro es nombre de alguna columna de [inctclie], es indispensable revisar los SP
            // Ningún parámetro es nombre de alguna columna de [Sucs_Domicilios], es indispensable revisar los SP
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

        // Su única referencia está en 'AddShippingAddressToCustomer()', el cuál Ya NO tiene referencias. 
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

        private async Task<bool> GetCustomerByCustumerKey(string customerKey) // Ya NO tiene referencias. 
        {
            string queryString = ""
                + " select "
                + " cli_clave as [CustomerKey], "
                + " cli_cvematriz as [ParentCustomerKey], "
                + " cli_agente as [AgentKey], "

                + " cli_claveExterna as [IdCustomer] "
                + " from inctclie (nolock) "
                + " where cli_clave = '" + customerKey.Trim() + "' "
                ;

            SqlConnection sqlConnection = DbConnection();


            var customerExist = await sqlConnection.QueryFirstOrDefaultAsync<CustomerResponse>(queryString);

            if (customerExist != null)
            {
                return true;
            }
            return false;

        }

        public async Task<bool> GetBillingCustomerById(string IdBillingAddress) // Ya NO tiene referencias. 
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

        // */ 

        #endregion 

        //
    }
}
