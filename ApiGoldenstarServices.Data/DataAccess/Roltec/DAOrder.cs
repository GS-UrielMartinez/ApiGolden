using ApiGoldenstarServices.Models.Goldenstar;
using Dapper;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ApiGoldenstarServices.Data.DataAccess.Goldenstar
{
    /// <summary>
    /// Acceso a datos para agregar, actualizar las ordenes provenientes de la pagina de roltec.mx
    /// </summary>
    public class DAOrder // : IOrder
    {
        private SqlConfiguration _SqlConfiguration;
        private EjecutorSql _EjecutorSql;

        public DAOrder(SqlConfiguration sqlConfiguration) 
        {
            if (sqlConfiguration == null)
            {
                throw
                    new ArgumentNullException(
                        nameof(sqlConfiguration),
                        "NO se puede obtener ConnectionString de un objeto Nulo de tipo 'SqlConfiguration'."
                        );
            }

            this._EjecutorSql = new EjecutorSql(sqlConfiguration.ConnectionString);
            this._SqlConfiguration = sqlConfiguration;
        }
        public DAOrder(SqlConnection sqlConnection) 
        {
            if (sqlConnection == null) 
            {
                throw
                    new ArgumentNullException(
                        nameof(sqlConnection),
                        "Se requiere una conexión de Base de Datos para instanciar un objeto de Acceso a Datos de Órdenes."
                        );
            }

            this._EjecutorSql = new EjecutorSql(sqlConnection);
            this._SqlConfiguration = new SqlConfiguration(sqlConnection.ConnectionString); 
        }

        // Get connection to Database
        public SqlConnection DbConnection()
        //TODO: Revisar cada vez que se pueda, y Cuando esta función no tenga ninguna referencia implementada, borrarla
        //TODO: Antes de borrar esta función, Insertar la misma Excepción y comentarios en la función de las demás Clases 
        {
            Exception ex = 
                new Exception("Falta Sustituir la implementación de  DAOrder.DbConnection(),  por las funciones de 'EjecutorSql'.");
            //if (ex != null) throw ex;

            return new SqlConnection(this._SqlConfiguration.ConnectionString);
        }


        #region " Ejemplos de Base ( Get, Insert, Update ) "

        public async Task<List<Order>> EditOrderListByCustomerIdAsync(string customerId)
        // Método para probar el manejo de Transacción, Y DESPUÉS BORRARLO 
        {
            SqlConnection? dbConn = await this._EjecutorSql.OpenDbConnectionOrNullIfAlreadyAsync();
            SqlTransaction? dbTransaction = this._EjecutorSql.BeginTransactionOrNullIfCurrent();

            try 
            {
                List<Order> listOrders = 
                    await this.GetOrderListByCustomerIdAsync(customerId); 
                foreach (Order order in listOrders) 
                {
                    order.CustomerName =
                        order.CustomerName
                        .Replace("Helodia", "Elodia")
                        .Replace("FRANCHELA", "FRANXHELA")
                        ;
                    order.Notes =
                        ("[" + DateTime.Now.ToShortDateString() + "] " + order.Notes);

                    if (order.CustomerName.Contains("doña")) 
                    {
                        Order orderCreated = await this.InsertOrderHeaderAsync(order);
                        //Order orderUpdated = await this.UpdateOrderHeaderAsync(order);  
                    }
                    else
                    {
                        //Order orderCreated = await this.InsertOrderHeaderAsync(order);
                        Order orderUpdated = await this.UpdateOrderHeaderAsync(order);
                    }

                    //
                }

                List<Order> listOrdersEdited =
                    await this.GetOrderListByCustomerIdAsync(customerId);

                if (dbTransaction != null) { await dbTransaction.RollbackAsync(); }
                //if (dbTransaction != null) { await dbTransaction.RollbackAsync(); }
                //if (dbTransaction != null) { await dbTransaction.CommitAsync(); }
                if (dbConn != null) { await dbConn.CloseAsync(); }  


                return 
                    listOrdersEdited;
            }
            catch (Exception ex)
            {
                if (dbTransaction != null) { await dbTransaction.RollbackAsync(); }
                if (dbConn != null) { await dbConn.CloseAsync(); }  

                throw new Exception(ex.Message);
            }
        }
        public async Task<List<Order>> GetOrderListByCustomerIdAsync(string customerId)
        // Método para probar el manejo de Transacción, Y DESPUÉS BORRARLO 
        {
            SqlConnection? dbConn = await this._EjecutorSql.OpenDbConnectionOrNullIfAlreadyAsync();
            SqlTransaction? dbTransaction = this._EjecutorSql.BeginTransactionOrNullIfCurrent();

            try 
            {
                Order order =
                    new Order();
                var aliasColumnPairs =
                    new Dictionary<string, string>();

                aliasColumnPairs[nameof(order.IdOrder)] = "ord_magento";
                aliasColumnPairs[nameof(order.CustomerId)] = "ord_clie";
                aliasColumnPairs[nameof(order.CustomerName)] = "trim( cli_nombre )"; //La columna puede incluir funciones de SQL
                aliasColumnPairs[nameof(order.Cupon)] = "ord_cupon";
                aliasColumnPairs[nameof(order.Discount)] = "descuentoimporte";
                aliasColumnPairs[nameof(order.Subtotal)] = "subtotal";
                aliasColumnPairs[nameof(order.Vat)] = "iva";
                aliasColumnPairs[nameof(order.VatRate)] = "tasa_iva";
                aliasColumnPairs[nameof(order.PaymentMethod)] = "metodo_pago_magento";

                //aliasColumnPairs[nameof(order.Notes)] = "trim( notas )"; //La columna puede incluir funciones de SQL
                aliasColumnPairs[nameof(order.Notes)] = "isnull( trim( notas ), '' )"; //La columna puede incluir funciones de SQL

                string[] arrColumnasMappeadas =
                    aliasColumnPairs
                    .Select(x => x.Value + " as [" + x.Key + "]")
                    .ToArray()
                    ;

                string strQueryAutoEnsamblado = ""
                    + " select "
                    + string.Join(", ", arrColumnasMappeadas)
                    + " from Magento_H (nolock) "
                    + " where ord_clie = '" + customerId.Trim() + "' "
                    ;

                List<Order> listOrders = new List<Order>();

                /* 
                var _Reader =
                    this._EjecutorSql.ExecuteReader(strQueryAutoEnsamblado);
                while (_Reader.Read())
                {
                    Order itemOrder = new Order();

                    itemOrder.IdOrder = _Reader.GetString(_Reader.GetOrdinal(nameof(itemOrder.IdOrder)));
                    itemOrder.CustomerId = _Reader.GetString(_Reader.GetOrdinal(nameof(itemOrder.CustomerId)));
                    itemOrder.CustomerName = _Reader.GetString(_Reader.GetOrdinal(nameof(itemOrder.CustomerName)));

                    itemOrder.Cupon = "";
                    itemOrder.Discount = "";
                    itemOrder.Subtotal = "";
                    itemOrder.Vat = "";
                    itemOrder.VatRate = "";
                    itemOrder.PaymentMethod = "";

                    itemOrder.Notes = _Reader.GetString(_Reader.GetOrdinal(nameof(itemOrder.Notes)));

                    listOrders.Add(itemOrder);
                }
                _Reader.Close();
                // */

                IEnumerable<Order> queriedOrders =
                    await this._EjecutorSql.QueryAsync<Order>(strQueryAutoEnsamblado, commandType: CommandType.Text); 
                listOrders = 
                    queriedOrders.ToList(); 


                if (dbTransaction != null) { await dbTransaction.CommitAsync(); }
                if (dbConn != null) { await dbConn.CloseAsync(); }  

                return
                    listOrders;
            }
            catch (Exception ex)
            {
                if (dbTransaction != null) { await dbTransaction.RollbackAsync(); }
                if (dbConn != null) { await dbConn.CloseAsync(); }  

                throw new Exception(ex.Message);
            }
        }

        public async Task<Order> GetOrderByIdAsync(string idOrder) 
        {
            if (idOrder == null) idOrder = "";
            if (idOrder.Trim() == "") throw new Exception("NO proporcionó ningún idOrder");

            SqlConnection? dbConn = await this._EjecutorSql.OpenDbConnectionOrNullIfAlreadyAsync();
            SqlTransaction? dbTransaction = this._EjecutorSql.BeginTransactionOrNullIfCurrent();

            try
            {
                Order? order =
                    new Order();
                var aliasColumnPairs =
                    new Dictionary<string, string>();

                #region " Query Mappeado a Patín ( funciona, pero YA NO SE VA A USAR ) "

                string strQueryMappeado = ""
                    + "select "
                    + " ord_clie as [" + nameof(order.CustomerId) + "], "
                    + " cli_nombre as [" + nameof(order.CustomerName) + "], "
                    + " ord_cupon as [" + nameof(order.Cupon) + "], "
                    + " descuentoimporte as [" + nameof(order.Discount) + "], "
                    + " subtotal as [" + nameof(order.Subtotal) + "], "
                    + " iva as [" + nameof(order.Vat) + "], "
                    + " tasa_iva as [" + nameof(order.VatRate) + "], "
                    + " metodo_pago_magento as [" + nameof(order.PaymentMethod) + "], "
                    + " notas as [" + nameof(order.Notes) + "], "

                    //+ " ord_clie as [CustomerId], " 
                    //+ " cli_nombre as [CustomerName], " 
                    //+ " ord_cupon as [Cupon], " 
                    //+ " descuentoimporte as [Discount], " 
                    //+ " subtotal as [Subtotal], " 
                    //+ " iva as [Vat], " 
                    //+ " tasa_iva as [VatRate], " 
                    //+ " metodo_pago_magento as [PaymentMethod], " 
                    //+ " notas as [Notes], " 

                    + " ord_magento as [" + nameof(order.IdOrder) + "] "
                    //+ " ord_magento as [IdOrder] " 
                    + " from Magento_H (nolock) "
                    + " where ord_magento = '" + idOrder.Trim() + "' "
                    ;

                #endregion 

                aliasColumnPairs[nameof(order.IdOrder)] = "ord_magento";
                aliasColumnPairs[nameof(order.CustomerId)] = "ord_clie";
                aliasColumnPairs[nameof(order.CustomerName)] = "trim( cli_nombre )"; //La columna puede incluir funciones de SQL
                aliasColumnPairs[nameof(order.Cupon)] = "ord_cupon";
                aliasColumnPairs[nameof(order.Discount)] = "descuentoimporte";
                aliasColumnPairs[nameof(order.Subtotal)] = "subtotal";
                aliasColumnPairs[nameof(order.Vat)] = "iva";
                aliasColumnPairs[nameof(order.VatRate)] = "tasa_iva";
                aliasColumnPairs[nameof(order.PaymentMethod)] = "metodo_pago_magento";
                aliasColumnPairs[nameof(order.Notes)] = "trim( notas )"; //La columna puede incluir funciones de SQL

                string[] arrColumnasMappeadas =
                    aliasColumnPairs
                    .Select(x => x.Value + " as [" + x.Key + "]")
                    .ToArray()
                    ;

                string strQueryAutoEnsamblado = ""
                    + " select "
                    + string.Join(", ", arrColumnasMappeadas)
                    + " from Magento_H (nolock) "
                    + " where ord_magento = '" + idOrder.Trim() + "' "
                    ;

                order =
                    //await dbConn.QuerySingleOrDefaultAsync<Order>(strQueryMappeado); 
                    await this._EjecutorSql.QuerySingleOrDefaultAsync<Order>(strQueryAutoEnsamblado);
                if (order == null)
                    throw new Exception("La Orden con idOrder " + idOrder.Trim() + " NO existe.");


                if (dbTransaction != null) { await dbTransaction.CommitAsync(); }
                if (dbConn != null) { await dbConn.CloseAsync(); }  

                return order;
            }
            catch (Exception ex)
            {
                if (dbTransaction != null) { await dbTransaction.RollbackAsync(); }
                if (dbConn != null) { await dbConn.CloseAsync(); }  

                throw new Exception(ex.Message);
            }
        }

        public async Task<Order> InsertOrderHeaderAsync(Order order)
        //public async Task<Order> UpdateOrderHeaderAsync(Order order) 
        {
            if (order == null)
                //throw new Exception("El Servicio Web NO recibió ningún dato de la Orden para ser Actualizado.");  // UPDATE
                throw new Exception("El Servicio Web NO recibió ningún dato para crear la Orden.");

            SqlConnection? dbConn = await this._EjecutorSql.OpenDbConnectionOrNullIfAlreadyAsync();
            SqlTransaction? dbTransaction = this._EjecutorSql.BeginTransactionOrNullIfCurrent();

            try
            {
                #region " Obtener una Nueva PK válida "

                string strQueryMaxPrimaryKey = ""
                    + " select max(ord_magento) as [ord_magento]  from Magento_H (nolock) "
                    + " where  not( ord_magento like '%-%' ) "
                    ;
                string maxPrimaryKey =
                    await this._EjecutorSql.QuerySingleOrDefaultAsync<string>(strQueryMaxPrimaryKey);
                int nextPrimaryKey =
                    int.Parse(maxPrimaryKey.Trim()) + 1;
                string newPrimaryKey =
                    "0000000000" + nextPrimaryKey.ToString();
                newPrimaryKey =
                    newPrimaryKey.Substring(newPrimaryKey.Length - 10, 10);

                #endregion 


                #region " Query SIN parametrizar ( PARA Insert ) "

                string strQueryInsert = ""
                    + " insert into Magento_H "
                    + " ( "
                    + " ord_magento, "

                    + " ord_clie, "
                    + " cli_nombre, " //cli_nombre", order.CustomerName
                    + " ord_cupon, " //ord_cupon", order.Cupon
                    + " descuentoimporte, " //descuentoimporte", order.Discount,
                    + " subtotal, " //subtotal", order.Subtotal,
                    + " iva, " //@iva", order.Vat,
                    + " tasa_iva, " //@tasa_iva", order.VatRate,
                    + " metodo_pago_magento, " //@metodo_pago_magento, ", order.PaymentMethod,
                    + " notas " //@notas", order.Notes
                    + " ) "

                    + " values "
                    + " ( "
                    + " '" + newPrimaryKey + "', "

                    + "'" + order.CustomerId.Trim() + "', "
                    + "'" + order.CustomerName.Trim() + "', "
                    + "'" + order.Cupon.Trim() + "', "
                    + "'" + order.Discount.Trim() + "', "
                    + "'" + order.Subtotal.Trim() + "', "
                    + "'" + order.Vat.Trim() + "', "
                    + "'" + order.VatRate.Trim() + "', "
                    + "'" + order.PaymentMethod.Trim() + "', "
                    + "'" + order.Notes.Trim() + "' "
                    + " ) "
                    ;

                #endregion

                #region " Query parametrizado ( PARA Insert ) "

                DynamicParameters dynParameters = new DynamicParameters();
                dynParameters.Add("@ord_magento", newPrimaryKey, (DbType?)SqlDbType.VarChar);
                dynParameters.Add("@ord_clie", order.CustomerId.Trim(), (DbType?)SqlDbType.VarChar);
                dynParameters.Add("@cli_nombre", order.CustomerName.Trim(), (DbType?)SqlDbType.VarChar);
                dynParameters.Add("@ord_cupon", order.Cupon.Trim(), (DbType?)SqlDbType.VarChar);

                //dynParameters.Add("@descuentoimporte", order.Discount.Trim(), (DbType?)SqlDbType.VarChar);
                dynParameters.Add("@descuentoimporte", "0" + order.Discount.Trim(), (DbType?)SqlDbType.VarChar);

                //dynParameters.Add("@subtotal", order.Subtotal.Trim(), (DbType?)SqlDbType.VarChar);
                dynParameters.Add("@subtotal", "0" + order.Subtotal.Trim(), (DbType?)SqlDbType.VarChar);

                //dynParameters.Add("@iva", order.Vat.Trim(), (DbType?)SqlDbType.VarChar);
                dynParameters.Add("@iva", "0" + order.Vat.Trim(), (DbType?)SqlDbType.VarChar);
                dynParameters.Add("@tasa_iva", order.VatRate.Trim(), (DbType?)SqlDbType.VarChar);

                dynParameters.Add("@metodo_pago_magento", order.PaymentMethod.Trim(), (DbType?)SqlDbType.VarChar);
                dynParameters.Add("@notas", order.Notes.Trim(), (DbType?)SqlDbType.VarChar);
                /* 
	            [ord_magento] [varchar](10) NOT NULL,
	            [ord_fecha] [datetime] NULL,
	            [ord_clie] [varchar](5) NULL,
	            [cli_nombre] [varchar](180) NULL,
	            [paqueteria] [varchar](5) NULL,  -- ( SP -> smallint, )

	            [cve_forma_pago] [char](2) NULL,
		            [no_parcialidades] [smallint] NULL,  -- numérico
		            [dias_credito] [smallint] NULL,  -- numérico
		            [subtotal] [decimal](14, 2) NULL,  -- numérico
	            [factura] [varchar](6) NULL,

	            [cve_uso_cfdi] [char](3) NULL,
	            [email_facturacion] [varchar](80) NULL,
		            [iva] [decimal](12, 2) NULL,  -- numérico
	            [tasa_iva] [varchar](4) NULL,  
	            [cve_metodo_pago] [char](3) NULL,

	            [ord_cupon] [varchar](15) NULL,
		            [descuentoimporte] [decimal](14, 2) NULL,  -- numérico
		            [id_magento] [int] NULL,  -- numérico
	            [metodo_pago_magento] [varchar](30) NULL,
	            [orden_compra_credito] [varchar](60) NULL,

	            [notas] [varchar](80) NULL,

                // */

                List<string> _ParameterNames =
                    dynParameters.ParameterNames.Select(x => "@" + x).ToList();
                //_ParameterNames.Add("@ord_magento");
                string strParametrosConcatenados =
                    string.Join(", ", _ParameterNames);

                string strQueryInsertParametrizado = ""
                    + " insert into Magento_H ( " + strParametrosConcatenados.Replace("@", "") + " ) "
                    + " values ( " + strParametrosConcatenados + " ) "
                    ;

                #endregion 


                int executeInsertOrder =
                    //await this._EjecutorSql.ExecuteAsync(strQueryInsert, commandType: CommandType.Text);
                    await this._EjecutorSql.ExecuteAsync(strQueryInsertParametrizado, dynParameters, commandType: CommandType.Text);

                Order insertedOrder =
                    await this.GetOrderByIdAsync(newPrimaryKey);


                #region " Query parametrizado ( PARA Update ) "

                List<string> _ParameterUpdateExpressions =
                    dynParameters.ParameterNames.Select(x => x.Trim() + " = @" + x.Trim()).ToList();

                //dynParameters.Add("@ord_magento", order.IdOrder.Trim(), (DbType?)SqlDbType.VarChar);
                string strQueryUpdateParametrizado = ""
                    + " update Magento_H  set "
                    + string.Join(", ", _ParameterUpdateExpressions)
                    //+ " where ord_magento = @ord_magento "
                    + " where ord_magento = '" + order.IdOrder.Trim() + "'"
                    ;

                #endregion 


                if (dbTransaction != null) { await dbTransaction.CommitAsync(); }
                if (dbConn != null) { await dbConn.CloseAsync(); }  

                return
                    insertedOrder;
            }
            catch (Exception ex)
            {
                if (dbTransaction != null) { await dbTransaction.RollbackAsync(); }
                if (dbConn != null) { await dbConn.CloseAsync(); }  

                throw new Exception(ex.Message);
            }
        }

        public async Task<Order> UpdateOrderHeaderAsync(Order order) 
        {
            if (order == null)
                throw new Exception("El Servicio Web NO recibió ningún dato de la Orden para ser Actualizado.");  // UPDATE

            SqlConnection? dbConn = await this._EjecutorSql.OpenDbConnectionOrNullIfAlreadyAsync();
            SqlTransaction? dbTransaction = this._EjecutorSql.BeginTransactionOrNullIfCurrent();

            try
            {
                #region " Query parametrizado ( PARA Update ) "

                DynamicParameters dynParameters = new DynamicParameters();
                //dynParameters.Add("@ord_magento", order.IdOrder.Trim(), (DbType?)SqlDbType.VarChar);  // La PrimaryKey NO debe modificarse
                dynParameters.Add("@ord_clie", order.CustomerId.Trim(), (DbType?)SqlDbType.VarChar);
                dynParameters.Add("@cli_nombre", order.CustomerName.Trim(), (DbType?)SqlDbType.VarChar);
                dynParameters.Add("@ord_cupon", order.Cupon.Trim(), (DbType?)SqlDbType.VarChar);

                //dynParameters.Add("@descuentoimporte", order.Discount.Trim(), (DbType?)SqlDbType.VarChar);
                dynParameters.Add("@descuentoimporte", "0" + order.Discount.Trim(), (DbType?)SqlDbType.VarChar);

                //dynParameters.Add("@subtotal", order.Subtotal.Trim(), (DbType?)SqlDbType.VarChar);
                dynParameters.Add("@subtotal", "0" + order.Subtotal.Trim(), (DbType?)SqlDbType.VarChar);

                //dynParameters.Add("@iva", order.Vat.Trim(), (DbType?)SqlDbType.VarChar);
                dynParameters.Add("@iva", "0" + order.Vat.Trim(), (DbType?)SqlDbType.VarChar);
                dynParameters.Add("@tasa_iva", order.VatRate.Trim(), (DbType?)SqlDbType.VarChar);

                dynParameters.Add("@metodo_pago_magento", order.PaymentMethod.Trim(), (DbType?)SqlDbType.VarChar);
                dynParameters.Add("@notas", order.Notes.Trim(), (DbType?)SqlDbType.VarChar);


                List<string> _ParameterUpdateExpressions =
                    dynParameters.ParameterNames.Select(x => x.Trim() + " = @" + x.Trim()).ToList();

                //dynParameters.Add("@ord_magento", order.IdOrder.Trim(), (DbType?)SqlDbType.VarChar);
                string strQueryUpdateParametrizado = ""
                    + " update Magento_H  set "
                    + string.Join(", ", _ParameterUpdateExpressions)
                    //+ " where ord_magento = @ord_magento "
                    + " where ord_magento = '" + order.IdOrder.Trim() + "'"
                    ;

                #endregion 


                int executeUpdateOrder =
                    await this._EjecutorSql.ExecuteAsync(strQueryUpdateParametrizado, dynParameters, commandType: CommandType.Text);

                Order updatedOrder =
                    await this.GetOrderByIdAsync(order.IdOrder.Trim());


                if (dbTransaction != null) { await dbTransaction.CommitAsync(); }
                if (dbConn != null) { await dbConn.CloseAsync(); }  

                return
                    updatedOrder;
            }
            catch (Exception ex)
            {
                if (dbTransaction != null) { await dbTransaction.RollbackAsync(); }
                if (dbConn != null) { await dbConn.CloseAsync(); }  

                throw new Exception(ex.Message);
            }
        }

        #endregion 


        // -------------------------------------------------------


        /// <summary>
        /// Agregar un nueva orden desde la pagina de roltec hacia la base de datos 
        /// </summary>
        /// <param name="order"></param>
        /// <returns>
        ///     {
        ///         "cve_orden": "08055",
        ///         "Status": "OK",
        ///         "Message": "Orden creada correctamente."
        ///      }
        /// </returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<OrderResponse> AddOrder(Order order) 
        {
            try
            {
                //To Do: agregar validaciones especificas

                //Encabezado0
                //validar cliente
                var customerExist = await ValidateCustomer(order.CustomerId);
                if (!customerExist)
                {
                    throw new Exception("El cliente no esta registrado");
                }
                // add header
                await AddHeaderOrder(order);
                //AddOrder orederdetail
                await AddOrderDetail(order);
                // get folio
                var newFolio = await GenerateFolio();
                
                order.Folio = newFolio.ToString();
                //Agregar direccion de envio
                var shippindAaddres = await AddShippingAddress(order);
                //Agregar encabezado 1
                var orderTableHeader = await AddOrderToShippingTableHeader(order);
                //agregar detallado 1
                var orderTableDetil = await AddOrderToShippingTableDetail(order);
                //agregar rollo consmo
                var roll = await AddRollUsage(order.IdOrder, order.CustomerId, order.Folio);
                //agregar liberacion del pedido
                var orderRelease = await AddOrderRelease(order.IdOrder, order.Folio);
            
                var newOrder = new OrderResponse
                {
                    Folio = newFolio.ToString(),
                    Message = "Orden Registrada correctamente"
                };
                return newOrder; 
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

        public async Task<bool> ValidateCustomer(string customerId)
        {
            var db = DbConnection();

            string queryString = @"select cli_clave as cve_cliente from dbo.inctclie (nolock) where cli_claveExterna=@customerId";

            var customer = await db.QueryFirstOrDefaultAsync<Customer>(queryString, new { customerId = customerId });

            if(customer != null)
            {
                return true;//si existe
            }

            return false;
           
        }

        /// <summary>
        /// Agrega encabezado de la orden en la base de datos
        /// </summary>
        /// <param name="order"></param>
        /// <returns>true</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> AddHeaderOrder(Order order)
        {
            var db = DbConnection();
            await db.OpenAsync();
            string CreatedAt = DateTime.Now.ToString("yyyyMMdd");
            // Add paremeters to StoreProcedure

            DynamicParameters parameters = new DynamicParameters();
            //20
            parameters.Add("@id_ord", order.IdOrder.ToUpper(), (DbType?)SqlDbType.VarChar);
            parameters.Add("@ord_fecha", CreatedAt, (DbType?)SqlDbType.VarChar);
            parameters.Add("@ord_clie", order.CustomerId, (DbType?)SqlDbType.VarChar);
            parameters.Add("@cli_nombre", order.CustomerName.ToUpper(), (DbType?)SqlDbType.VarChar);
            parameters.Add("@paqueteria", "", (DbType?)SqlDbType.VarChar);
            parameters.Add("@cve_forma_pago", order.BillingAddress.PaymentTypeKey, (DbType?)SqlDbType.VarChar);
            //parameters.Add("@no_parcialidades", order.IdOrder.ToUpper(), (DbType?)SqlDbType.VarChar);

            parameters.Add("@dias_credito", order.BillingAddress.CreditDays, DbType.Int16);

            parameters.Add("@ord_cupon", order.Cupon, (DbType?)SqlDbType.VarChar);
            parameters.Add("@descuentoimporte", order.Discount, (DbType?)SqlDbType.VarChar);
            parameters.Add("@subtotal", order.Subtotal, (DbType?)SqlDbType.VarChar);
            //parameters.Add("@factura", order.ta, (DbType?)SqlDbType.VarChar);
            parameters.Add("@cve_uso_cfdi", order.BillingAddress.CfdiUsageKey, (DbType?)SqlDbType.VarChar);
            //parameters.Add("@email_facturacion", order.Ema, (DbType?)SqlDbType.VarChar);//se puede obtener del cliente
            parameters.Add("@iva", order.Vat, (DbType?)SqlDbType.VarChar);
            parameters.Add("@tasa_iva", order.VatRate, (DbType?)SqlDbType.VarChar);
            //parameters.Add("@cve_metodo_pago", , (DbType?)SqlDbType.VarChar);
            parameters.Add("@metodo_pago", order.PaymentMethod, (DbType?)SqlDbType.VarChar);
            //parameters.Add("@orden_compra_credito", order.IdOrder.ToUpper(), (DbType?)SqlDbType.VarChar);//como debe aplicar?
            parameters.Add("@notas", order.Notes, (DbType?)SqlDbType.VarChar);


            try
            {
                var newOrder = await db.ExecuteAsync("Magento_Orden_Agregar_Magento_H", parameters, commandType: CommandType.StoredProcedure);//modificar el SP
                //To do: return object
            }
            catch (Exception ex)
            {
                await db.CloseAsync();
                throw new Exception(ex.Message);
            }

            return true;
        }


        /// <summary>
        /// Agrega el detallado de una orden (todos los productos que vienen en una orden)
        /// </summary>
        /// <param name="order"></param>
        /// <returns>true</returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> AddOrderDetail(Order order)
        {
            var db = DbConnection();
            // Add paremeters to StoreProcedure
            var i = 0;
            foreach(var orderItem in order.OrderDetail)
            {
                await db.OpenAsync();
                DynamicParameters parameters = new DynamicParameters();

                //TODO: Quitar estos parches cuando se corrijan los tamaños
                // de los parámetros '@bcve_ciudad' y '@scve_ciudad'  en el SP 'Magento_Orden_Agregar_Magento_D' 
                if (order.BillingAddress.CityKey == null) order.BillingAddress.CityKey = "";
                if (order.ShippingAddress.CityKey == null) order.ShippingAddress.CityKey = "";
                order.BillingAddress.CityKey = 
                    order.BillingAddress.CityKey.Substring(0, 4);
                order.ShippingAddress.CityKey = 
                    order.ShippingAddress.CityKey.Substring(0, 4);

                parameters.Add("@id_ord", order.IdOrder, (DbType?)SqlDbType.VarChar);
                parameters.Add("@ord_item", orderItem.orderItemId, (DbType?)SqlDbType.VarChar);
                parameters.Add("@sku", orderItem.Sku, (DbType?)SqlDbType.VarChar);
                parameters.Add("@ord_nombre", order.CustomerName, (DbType?)SqlDbType.VarChar);
                parameters.Add("@ord_cant", orderItem.Quantity, (DbType?)SqlDbType.VarChar);
                parameters.Add("@ord_punit", orderItem.UnitPrice, (DbType?)SqlDbType.VarChar);
                parameters.Add("@ord_descuento", orderItem.Discount, (DbType?)SqlDbType.VarChar);
                //@b billling
                parameters.Add("@bcve_sucursal", order.BillingAddress.IdBillingAddress, (DbType?)SqlDbType.VarChar);
                parameters.Add("@bcalle", order.BillingAddress.Street, (DbType?)SqlDbType.VarChar);
                parameters.Add("@bcolonia", order.BillingAddress.Colony, (DbType?)SqlDbType.VarChar);
                parameters.Add("@bciudad", order.BillingAddress.City, (DbType?)SqlDbType.VarChar);
                parameters.Add("@bcodigo_postal", order.BillingAddress.ZipCode, (DbType?)SqlDbType.VarChar);
                parameters.Add("@bcve_ciudad", order.BillingAddress.CityKey, (DbType?)SqlDbType.VarChar);
                //@s shipping
                parameters.Add("@scve_sucursal", order.ShippingAddress.ShippingAddressId, (DbType?)SqlDbType.VarChar);
                parameters.Add("@scalle", order.ShippingAddress.Street, (DbType?)SqlDbType.VarChar);
                parameters.Add("@scolonia", order.ShippingAddress.Colony, (DbType?)SqlDbType.VarChar);
                parameters.Add("@sciudad", order.ShippingAddress.City, (DbType?)SqlDbType.VarChar);
                parameters.Add("@scodigo_postal", order.ShippingAddress.ZipCode, (DbType?)SqlDbType.VarChar);
                parameters.Add("@scve_ciudad", order.ShippingAddress.CityKey, (DbType?)SqlDbType.VarChar);


                try
                {
                    var newOrder = await db.ExecuteAsync("Magento_Orden_Agregar_Magento_D", parameters, commandType: CommandType.StoredProcedure);//modificar el SP
                }
                catch (Exception ex)
                {
                    
                    throw new Exception(ex.Message);
                }
                finally { await db.CloseAsync(); }



                //exit to bucle
                i++;
                if(i == order.OrderDetail.Count)
                {
                    break;
                }
            }

            return true;
        }

        /// <summary>
        /// generar folio
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<string> GenerateFolio()
        {

            var db = DbConnection();
            db.OpenAsync();

            DynamicParameters parameters = new DynamicParameters();

            parameters.Add("@Folio", 1, direction: ParameterDirection.Output);

            try
            {
                // revisar el return de este SP
                var folio= new FolioResponse();
                folio = await db.QueryFirstOrDefaultAsync<FolioResponse>("SPpedidosConsumo_Obt_Folio", parameters, commandType: CommandType.StoredProcedure);
                //var newOrder = await db.ExecuteAsync("SPpedidosConsumo_Obt_Folio", parameters, commandType: CommandType.StoredProcedure);//modificar el SP
                var f = parameters.Get<int>("@Folio");
                return f.ToString();
            }
            catch (Exception ex)
            {
                await db.CloseAsync();

                throw new Exception(ex.Message);
            }
            
        }

        


        // agregar direccion de envio
        public async Task<bool> AddShippingAddress(Order order)
        {
            var db = DbConnection();
            
            await db.OpenAsync();
            DynamicParameters parameters = new DynamicParameters();

            parameters.Add("@cve_cliente", order.CustomerId, (DbType?)SqlDbType.VarChar);

            //TODO: quitar estos parches cuando se corrija el tamaño del parámetro @cve_sucursal  en el SP
            if (order.ShippingAddress.ShippingAddressId == null)
                order.ShippingAddress.ShippingAddressId = "";
            if (order.ShippingAddress.CityKey == null) order.ShippingAddress.CityKey = "";

            //TODO: corregir tamaño del parámetro en el SP
            parameters.Add("@cve_sucursal", order.ShippingAddress.ShippingAddressId.Substring(0,5), (DbType?)SqlDbType.VarChar);
            //parameters.Add("@cve_sucursal", order.ShippingAddress.ShippingAddressId, (DbType?)SqlDbType.VarChar);

            parameters.Add("@calle", order.ShippingAddress.Street, (DbType?)SqlDbType.VarChar);
            parameters.Add("@colonia", order.ShippingAddress.Colony, (DbType?)SqlDbType.VarChar);
            parameters.Add("@ciudad", order.ShippingAddress.City, (DbType?)SqlDbType.VarChar);
            parameters.Add("@telefono", order.ShippingAddress.Phone, (DbType?)SqlDbType.VarChar);

            parameters.Add("@pais", "Mexico", (DbType?)SqlDbType.VarChar);
            parameters.Add("@codigo_postal", order.ShippingAddress.ZipCode, (DbType?)SqlDbType.VarChar);

            //TODO: corregir tamaño del parámetro en el SP
            parameters.Add("@cve_ciudad", order.ShippingAddress.CityKey.Substring(0,4), (DbType?)SqlDbType.VarChar);
            //parameters.Add("@cve_ciudad", order.ShippingAddress.CityKey, (DbType?)SqlDbType.VarChar);

            parameters.Add("@cli_nombre", order.CustomerName, (DbType?)SqlDbType.VarChar);

            try
            {
                var newShippingAddress  = await db.ExecuteAsync("Magento_Orden_Modificar_DireccionEnvio", parameters, commandType: CommandType.StoredProcedure);//modificar el SP
                return true;
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
            finally { await db.CloseAsync(); }

        }
        
        //agregar encabezado de la orden 1 en esta tabla se le da seguimiento para el envio y actualizaciones del pedido
        public async Task<bool> AddOrderToShippingTableHeader(Order order)
        {
            var db = DbConnection();

            await db.OpenAsync();
            string CreatedAt = DateTime.Now.ToString("yyyyMMdd");

            DynamicParameters parameters = new DynamicParameters();

            parameters.Add("@ord_magento", order.IdOrder, (DbType?)SqlDbType.VarChar);
            parameters.Add("@ord_fecha", CreatedAt, (DbType?)SqlDbType.VarChar);
            parameters.Add("@ord_cli", order.CustomerId, (DbType?)SqlDbType.VarChar);
            parameters.Add("@cli_nombre", order.CustomerName, (DbType?)SqlDbType.VarChar);//denominacion social
            //parameters.Add("@paqueteria", order.CustomerId, (DbType?)SqlDbType.VarChar);
            parameters.Add("@cve_forma_pago", order.BillingAddress.PaymentTypeKey, (DbType?)SqlDbType.VarChar);
            //parameters.Add("@no_parcialidades", order.CustomerId, (DbType?)SqlDbType.VarChar);

            parameters.Add("@dias_credito", order.BillingAddress.CreditDays, DbType.Int16);

            parameters.Add("@ord_cupon", order.Cupon, (DbType?)SqlDbType.VarChar);
            parameters.Add("@descuentoimporte", order.Discount, (DbType?)SqlDbType.VarChar);
            parameters.Add("@subtotal", order.Subtotal, (DbType?)SqlDbType.VarChar);
            //parameters.Add("@factura", order., (DbType?)SqlDbType.VarChar);
            parameters.Add("@cve_uso_cfdi", order.BillingAddress.CfdiUsageKey, (DbType?)SqlDbType.VarChar);
            //parameters.Add("@email_facturacion", order., (DbType?)SqlDbType.VarChar);
            parameters.Add("@iva", order.Vat, (DbType?)SqlDbType.VarChar);
            parameters.Add("@tasa_iva", order.VatRate, (DbType?)SqlDbType.VarChar);
            parameters.Add("@cve_metodo_pago", order.BillingAddress.PaymentMethodKey, (DbType?)SqlDbType.VarChar);
            parameters.Add("@metodo_pago_magento", order.PaymentMethod, (DbType?)SqlDbType.VarChar);
            //parameters.Add("@orden_compra_credito", order.CustomerId, (DbType?)SqlDbType.VarChar);
            parameters.Add("@ordID", order.Folio, (DbType?)SqlDbType.VarChar);
            parameters.Add("@notas", order.Notes, (DbType?)SqlDbType.VarChar);


            //TODO: Cuando el código postal NO existe, el StoredProcedure falla sin especificar la causa 
            // porque esto ocasiona que NO se obtenga una clave de Paquetería
            // y se intente insertar un valor NULL en una columna 'NOT NULL'
            parameters.Add("@scodigo_postal", order.ShippingAddress.ZipCode, (DbType?)SqlDbType.VarChar);


            try
            {
                var newShippingAddressOrderHeader = await db.ExecuteAsync("Magento_Orden_Agregar_Encabezado1", parameters, commandType: CommandType.StoredProcedure);//modificar el SP
                
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally { await db.CloseAsync(); }

        }
        
        //agregar detalle de la orden 1
        public async Task<bool> AddOrderToShippingTableDetail(Order order)
        {
            var db = DbConnection();
            // Add paremeters to StoreProcedure
            var i = 0;
            foreach (var orderItem in order.OrderDetail)
            {
                await db.OpenAsync();
                DynamicParameters parameters = new DynamicParameters();

                parameters.Add("@ordID", order.IdOrder, (DbType?)SqlDbType.VarChar);
                parameters.Add("@ord_cli", order.CustomerId, (DbType?)SqlDbType.VarChar);
                parameters.Add("@ord_item", orderItem.orderItemId, (DbType?)SqlDbType.VarChar);


                //TODO: Los valores NO existentes de  orderItem.Sku ( [Magento_d].Sku ),
                // provocan división sobre Cero en el SP 'Magento_Orden_Agregar_Detalle1'
                parameters.Add("@sku", orderItem.Sku, (DbType?)SqlDbType.VarChar);


                parameters.Add("@ord_nombre", order.CustomerName, (DbType?)SqlDbType.VarChar);
                parameters.Add("@ord_cant", orderItem.Quantity, (DbType?)SqlDbType.VarChar);
                parameters.Add("@ord_punit", orderItem.UnitPrice, (DbType?)SqlDbType.VarChar);
                parameters.Add("@ord_descuento", orderItem.Discount, (DbType?)SqlDbType.VarChar);
                //parameters.Add("@paqueteria", orderItem., (DbType?)SqlDbType.VarChar);
                //@b billling
                parameters.Add("@bcve_sucursal", order.BillingAddress.IdBillingAddress, (DbType?)SqlDbType.VarChar);
                parameters.Add("@bcalle", order.BillingAddress.Street, (DbType?)SqlDbType.VarChar);
                parameters.Add("@bcolonia", order.BillingAddress.Colony, (DbType?)SqlDbType.VarChar);
                parameters.Add("@bcodigo_postal", order.BillingAddress.ZipCode, (DbType?)SqlDbType.VarChar);

                //@s shipping
                parameters.Add("@scve_sucursal", order.ShippingAddress.ShippingAddressId, (DbType?)SqlDbType.VarChar);
                parameters.Add("@scalle", order.ShippingAddress.Street, (DbType?)SqlDbType.VarChar);
                parameters.Add("@scolonia", order.ShippingAddress.Colony, (DbType?)SqlDbType.VarChar);
                parameters.Add("@scodigo_postal", order.ShippingAddress.ZipCode, (DbType?)SqlDbType.VarChar);


                //TODO: Quitar este parche cuando se corrija el tamaño del parámetro '@scve_ciudad' en el StoredProcedure 
                if (order.ShippingAddress.CityKey == null) 
                    order.ShippingAddress.CityKey = "";

                //TODO: Falta corregir el tamaño del parámetro '@scve_ciudad' en el StoredProcedure
                parameters.Add("@scve_ciudad", order.ShippingAddress.CityKey.Substring(0,4), (DbType?)SqlDbType.VarChar);
                //parameters.Add("@scve_ciudad", order.ShippingAddress.CityKey, (DbType?)SqlDbType.VarChar);

                #region " parámetros que el SP 'Magento_Orden_Agregar_Detalle1'  NO utiliza para nada " 

                parameters.Add("@bcve_ciudad", order.BillingAddress.CityKey, (DbType?)SqlDbType.VarChar);
                parameters.Add("@bciudad", order.BillingAddress.City, (DbType?)SqlDbType.VarChar);
                parameters.Add("@sciudad", order.ShippingAddress.City, (DbType?)SqlDbType.VarChar);

                #endregion 


                try
                {
                    var newOrder = await db.ExecuteAsync("Magento_Orden_Agregar_Detalle1", parameters, commandType: CommandType.StoredProcedure);//modificar el SP
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
                finally { await db.CloseAsync();}



                //exit to bucle
                i++;
                if (i == order.OrderDetail.Count)
                {
                    break;
                }
            }

            return true;
        }
        
        //agregar rollo consumo
        public async Task<bool> AddRollUsage(string orderId, string customerId, string folio)
        {
            var db = DbConnection();

            await db.OpenAsync();
            DynamicParameters parameters = new DynamicParameters();

            parameters.Add("@ord_magento", orderId, (DbType?)SqlDbType.VarChar);
            parameters.Add("@ord_cli", customerId, (DbType?)SqlDbType.VarChar);
            parameters.Add("@ordID", folio, (DbType?)SqlDbType.VarChar);//folio


            try
            {
                //TODO: El SP 'Magento_Orden_Agregar_RolloConsumo' intenta Insertar
                // [Magento_d].scve_ciudad, que es  varchar(5)
                // en [opmtdest_RolloConsumo_Destinos].Ciudad, que es  varchar(4) 
                // ...y al truncar TRUENA! 
                await db.ExecuteAsync("Magento_Orden_Agregar_RolloConsumo", parameters, commandType: CommandType.StoredProcedure);//modificar el SP

                return true;            
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally { await db.CloseAsync(); }

        }
        
        //Agregar liberacion del pedido
        public async Task<bool> AddOrderRelease(string orderId,string folio)
        {
            var db = DbConnection();

            await db.OpenAsync();
            DynamicParameters parameters = new DynamicParameters();

            parameters.Add("@ord_magento", orderId, (DbType?)SqlDbType.VarChar);
            parameters.Add("@orden", folio, (DbType?)SqlDbType.VarChar);//folio


            try
            {
                var newRollusage = await db.ExecuteAsync("Magento_Orden_Agregar_LiberacionPedido", parameters, commandType: CommandType.StoredProcedure);//modificar el SP
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally { await db.CloseAsync(); }

        }
    }   

}
