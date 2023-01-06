using Business.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Business.Interfaces
{
    public interface ICustomerService : ICrud<CustomerModel>
    {
        Task<IEnumerable<CustomerModel>> GetCustomersByProductIdAsync(int productId);
    }
}
