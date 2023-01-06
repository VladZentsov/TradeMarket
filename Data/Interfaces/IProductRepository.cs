using Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Data.Interfaces
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<IEnumerable<Product>> GetAllWithDetailsAsync();

        Task<Product> GetByIdWithDetailsAsync(int id);
    }
}
