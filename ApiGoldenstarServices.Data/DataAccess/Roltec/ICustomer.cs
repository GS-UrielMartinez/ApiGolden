using ApiGoldenstarServices.Models.Goldenstar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiGoldenstarServices.Data.DataAccess.Roltec
{
    public interface ICustomer
    {
        Task<IEnumerable<Customer>> GetCustomersList();

        Task<CustomerResponse> GetCustomerResponseById(string idCustomer);

        Task<Customer> GetCustomerById(string idCustomer);

        Task<bool> GetCustomerByCustumerKey(string customerKey);

        Task<bool> AddCustomer(Customer customer);

        Task<bool> UpdateCustomer(Customer customer);

        Task ValidateBillingCustomer(Customer customer);
        Task<bool> GetBillingCustomerById(string IdBillingAddress);

        Task<bool> AddShippingAddressToCustomer(ShippingAddress shippingAddress, string customerId);

        Task<BillingAddress> AddBillingAddressToCustomer(BillingAddress billingAddress);

    }


}
