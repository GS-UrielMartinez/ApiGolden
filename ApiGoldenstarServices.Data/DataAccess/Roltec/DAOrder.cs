using ApiGoldenstarServices.Models;
using ApiGoldenstarServices.Models.Goldenstar;
using Dapper;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiGoldenstarServices.Data.DataAccess.Goldenstar
{
    /// <summary>
    /// Acceso a datos para agregar, actualizar las ordenes provenientes de la pagina de roltec.mx
    /// </summary>
    public class DAOrder : IOrder
    {
        private SqlConfiguration _sqlConfiguration;

        public DAOrder(SqlConfiguration sqlConfiguration)
        {
            _sqlConfiguration = sqlConfiguration;
        }

        // Get connection to Database
        public SqlConnection DbConnection()
        {
            return new SqlConnection(_sqlConfiguration.ConnetionString);
        }


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

        //private async Task<Order> getOrderByIdOrder(string orderId)
        //{
        //    var db = DbConnection();

        //    string queryString = @"select cli_clave as IdOrder from dbo.inctclie (nolock) where cli_claveExterna=@orderId";

        //    var customer = await db.QueryFirstOrDefaultAsync<Order>(queryString, new { orderId = orderId });
        //}

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
            parameters.Add("@dias_credito", order.BillingAddress.CreditDays, (DbType?)SqlDbType.VarChar);
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

            parameters.Add("@cve_sucursal", order.ShippingAddress.ShippingAddressId, (DbType?)SqlDbType.VarChar);
            parameters.Add("@calle", order.ShippingAddress.Street, (DbType?)SqlDbType.VarChar);
            parameters.Add("@colonia", order.ShippingAddress.Colony, (DbType?)SqlDbType.VarChar);
            parameters.Add("@ciudad", order.ShippingAddress.City, (DbType?)SqlDbType.VarChar);
            parameters.Add("@telefono", order.ShippingAddress.Phone, (DbType?)SqlDbType.VarChar);

            parameters.Add("@pais", "Mexico", (DbType?)SqlDbType.VarChar);
            parameters.Add("@codigo_postal", order.ShippingAddress.ZipCode, (DbType?)SqlDbType.VarChar);
            parameters.Add("@cve_ciudad", order.ShippingAddress.CityKey, (DbType?)SqlDbType.VarChar);
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
            parameters.Add("@dias_credito", order.BillingAddress.CreditDays, (DbType?)SqlDbType.VarChar);
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
            parameters.Add("@scodigo_postal", order.ShippingAddress.ZipCode, (DbType?)SqlDbType.VarChar);
            parameters.Add("@ordID", order.Folio, (DbType?)SqlDbType.VarChar);
            parameters.Add("@notas", order.Notes, (DbType?)SqlDbType.VarChar);


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
