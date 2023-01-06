using System.Threading.Tasks;

namespace Data.Interfaces
{
    public interface IUnitOfWork
    {
        ICustomerRepository CustomerRepository { get; }

        IPersonRepository PersonRepository { get; }

        IProductRepository ProductRepository { get; }

        IProductCategoryRepository ProductCategoryRepository { get; }

        IReceiptRepository ReceiptRepository { get; }

        IReceiptDetailRepository ReceiptDetailRepository { get; }

        Task SaveAsync();
    }
}
