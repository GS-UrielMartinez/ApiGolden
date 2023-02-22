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

        Task<Customer> GetCustomerById(int id);

        Task<Customer> AddCustomer(Customer customer);

        Task<Customer> UpdateCustomer(Customer customer);

        Task ValidateDuplicatedCustomer(Customer customer);

        Task<ShippingAddress> AddShippingAddressToCustomer(ShippingAddress shippingAddress);
        
        Task<BillingAddress> AddBillingAddressToCustomer(BillingAddress billingAddress);

    }
}
