using Data.Data;
using Data.Entities;
using Data.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repositories
{
    public class ReceiptRepository : IReceiptRepository
    {
        private readonly DbSet<ReceiptDetail> _receiptsDetails;
        private readonly DbSet<Product> _products;
        private readonly DbSet<Receipt> _receipts;
        private readonly DbSet<ProductCategory> _productCategories;
        private readonly DbSet<Customer> _customers;
        private readonly DbSet<Person> _persons;
        public ReceiptRepository(ITradeMarketDbContext dbContext)
        {
            _receipts = dbContext.Set<Receipt>();
            _receiptsDetails = dbContext.Set<ReceiptDetail>();
            _products = dbContext.Set<Product>();
            _productCategories = dbContext.Set<ProductCategory>();
            _customers= dbContext.Set<Customer>();
            _persons = dbContext.Set<Person>();
        }
        public Task AddAsync(Receipt entity)
        {
            _receipts.Add(entity);
            return Task.CompletedTask;
        }

        public void Delete(Receipt entity)
        {
            var result = entity != null;
            if (result)
            {
                _receipts.Remove(entity);
            }
        }

        public async Task DeleteByIdAsync(int id)
        {
            Delete(await GetByIdAsync(id));
        }

        public async Task<IEnumerable<Receipt>> GetAllAsync()
        {
            return _receipts;
        }

        public async Task<IEnumerable<Receipt>> GetAllWithDetailsAsync()
        {
            var receipts = _receipts
                .Include(r => r.Customer)
                .Include(r => r.ReceiptDetails)
                .ThenInclude(rd => rd.Product)
                .ThenInclude(p => p.Category);

            _customers.Load();
            _persons.Load();
            _receipts.Load();
            _products.Load();
            _productCategories.Load();

            return receipts;
        }

        public async Task<Receipt> GetByIdAsync(int id)
        {
            return await _receipts.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Receipt> GetByIdWithDetailsAsync(int id)
        {
            return (await GetAllWithDetailsAsync()).FirstOrDefault(c => c.Id == id);
        }

        public void Update(Receipt entity)
        {
            _receipts.Update(entity);
        }
    }
}
