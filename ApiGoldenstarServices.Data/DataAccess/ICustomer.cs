using ApiGoldenstarServices.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiGoldenstarServices.Data.DataAccess
{
    public interface ICustomer
    {
        Task<IEnumerable<Customer>> GetCustomersList();

        Task<CustomerResponse> GetCustomerById(string idCustomer);
        
        Task<bool> GetCustomerByCustumerKey(string customerKey);

        Task<bool> AddCustomer(Customer customer);

        Task<bool> UpdateCustomer(Customer customer);

        Task ValidateBillingCustomer(Customer customer);
        Task<bool> GetBillingCustomerById(string IdBillingAddress);

        Task<ShippingAddress> AddShippingAddressToCustomer(ShippingAddress shippingAddress);
        
        Task<BillingAddress> AddBillingAddressToCustomer(BillingAddress billingAddress);

    }


}
