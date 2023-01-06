using Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Data.Interfaces
{
    public interface IReceiptRepository : IRepository<Receipt>
    {
        Task<IEnumerable<Receipt>> GetAllWithDetailsAsync();

        Task<Receipt> GetByIdWithDetailsAsync(int id);
    }
}
