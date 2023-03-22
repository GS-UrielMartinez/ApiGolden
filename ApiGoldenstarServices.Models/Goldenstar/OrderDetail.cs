using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiGoldenstarServices.Models.Goldenstar
{
    public class OrderDetail
    {
        public int orderItemId { get; set; } //ord_item_id

        public string Sku { get; set; } //sku

        public string orderName { get; set; } //ord_name

        public double UnitPrice { get; set; } //ord_punit

        public double Discount { get; set; } //ord_descuento

        public int Quantity { get; set; } //ord_cant

    }
}
