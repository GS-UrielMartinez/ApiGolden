using ApiGoldenstarServices.Data.Utils;
using ApiGoldenstarServices.HttpServices.ExternalServices.Roltec;
using ApiGoldenstarServices.Models.Goldenstar;
using ApiGoldenstarServices.Models.ServicesModels.RoltecApi;
using Azure;
using Dapper;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiGoldenstarServices.Data.DataAccess.Admin.Roltec
{
    public class DAInvoice : IInvoice
    {
        private SqlConfiguration _conectionString;

        private IRoltecApi _roltecApi;

        public DAInvoice(SqlConfiguration connectionString, IRoltecApi roltecApi)
        {
            _conectionString = connectionString;
            _roltecApi = roltecApi;
        }


        // Get connection to Database
        protected SqlConnection DbConnection()
        {
            return new SqlConnection(_conectionString.ConnetionString);
        }

       
        //iactionResult
        async Task<OrderRoltec> IInvoice.UpdateInvoiceToWebAsync(string folio)
        {
            //obtener la factura
            
            // generar url
            var urlPDFFile = UrlFile.EncodeUrl(folio+".pdf");
            var urlXMLFile = UrlFile.EncodeUrl(folio+".xml");
            // obtener la orden asociada a la factura
            InvoiceResponse orderInvoiced = await GetOrderByFolio(folio.Substring(3));
            // Obtener la orden
            Order order = await GetOrderAsync(orderInvoiced.OrderId);
            //orden actualizada
            var orderRoltec = new OrderRoltec();
            //orderRoltec.IdOrder = orderInvoiced.;
            orderRoltec.IdOrder = order.IdOrder;
            orderRoltec.Folio = order.Folio;
            orderRoltec.CreatedAt = order.CreatedAt;
            orderRoltec.CustomerId = order.CustomerId;
            orderRoltec.CustomerName = order.CustomerName;
            orderRoltec.PaymentMethod = order.PaymentMethod;
            orderRoltec.Cupon = order.Cupon;
            orderRoltec.Discount = order.Discount;
            orderRoltec.Subtotal = order.Subtotal;
            orderRoltec.Vat = order.Vat;
            orderRoltec.VatRate = order.VatRate;
            orderRoltec.Notes = order.Notes;
            orderRoltec.Status = order.Status;
            orderRoltec.OrderDetail = order.OrderDetail;
            orderRoltec.BillingAddress = order.BillingAddress;

            //invoiceDetail
            orderRoltec.InvoiceDetail.IdInvoice = 1;
            orderRoltec.InvoiceDetail.TypeInvoice = folio.Substring(0, 3);
            orderRoltec.InvoiceDetail.XMLInvoiceFile = urlXMLFile;
            orderRoltec.InvoiceDetail.PDFInvoiceFile = urlPDFFile;

            //ejecutar la peticion hacia roltec,mx
            var response = await _roltecApi.UpdateOrderStatus<OrderRoltec>(orderRoltec);

            return response;
        }

        public Task<Invoice> GetInvoiceAsync(string folio)
        {
            var typeInvoice = folio.Substring(0,3);
            var idFactura = folio.Substring(3);
            //Consulta el detalle general de la factura
            string consulta = $@"select Ft.IdSalesforce as IdSalesforce, fac_sit as Estatus,
                                sum(Ft.fac_cant*Ft.fac_venta) as PrecioVentaTotal, cli.IdSalesforce as IdCliente,
                                Ft.fac_fact as IdGoldenstar, Ft.fac_fecha as Fecha, min(Cf.CveUsoCFDI) as CFDI,
                                min(Cf.CveMetodoPago) as MetodoPago, min(Cf.CveFormaPago) as FormaPago,
                                case when min(fac_dia) >= 997 then 'CONTADO' else 'CREDITO' end as CondicionPago, '' as LigaFactura
                                from inmtfact  as Ft (Nolock)
                                left join inctclie as cli (Nolock) on Ft.fac_clie=cli_clave
                                left join cfdi_archivos as Cf (Nolock) on Cf.folio=Ft.fac_fact and Cf.serie=Ft.fac_serie
                                where Ft.fac_serie = '{typeInvoice}' and Ft.fac_fact = '{idFactura}'
                                group by Ft.fac_serie,Ft.fac_fact,Ft.fac_fecha,Ft.fac_clie,cli_nombre,cli.IdSalesforce, Ft.IdSalesforce, fac_sit
                                order by Ft.fac_serie,Ft.fac_fact";

            throw new NotImplementedException();
        }

        public Task<string> GetInvoiceFileAsync(string folio)
        {
            throw new NotImplementedException();
        }

        public async Task<Order> GetOrderAsync(string orderId)
        {

            Order order = new Order();

            return order;
        }

        public async Task<InvoiceResponse> GetOrderByFolio(string folio)
        {
            SqlConnection sqlConnection = DbConnection();
            sqlConnection.Open();
            string queryString = $@"select 
	                                fac_fact as IdInvoice, 
	                                fac_serie as TypeInvoice,
                                    fac_op as IdOrder

                                from inmtfact 
                                where 
	                                fac_fact={folio} 
	                                and fac_fact = 'p';";

            InvoiceResponse? invoiceResponse = default;
            try
            {
                invoiceResponse = await sqlConnection.QueryFirstOrDefaultAsync<InvoiceResponse>(queryString);

                return invoiceResponse;
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

        public async Task<OrderRoltec> GetOrderRoltecAsync(string orderId)
        {
            SqlConnection sqlConnection = DbConnection();
            sqlConnection.Open();
            string queryString = $@"select 
	                                fac_fact as IdInvoice, 
	                                fac_serie as TypeInvoice,
                                    fac_op as IdOrder

                                from inmtfact 
                                where 
	                                fac_fact={orderId} 
	                                and fac_fact = 'p';";

            OrderRoltec? orderRoltec = default;
            try
            {
                orderRoltec = await sqlConnection.QueryFirstOrDefaultAsync<OrderRoltec>(queryString);

                return orderRoltec;
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
