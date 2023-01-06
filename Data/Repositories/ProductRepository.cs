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
    public class ProductRepository : IProductRepository
    {
        private readonly DbSet<Product> _products;
        private readonly DbSet<ReceiptDetail> _receiptsDetails;
        private readonly DbSet<ProductCategory> _productsCategory;
        private readonly DbSet<Receipt> _receipts;
        public ProductRepository(ITradeMarketDbContext dbContext)
        {
            _products = dbContext.Set<Product>();
            _receiptsDetails= dbContext.Set<ReceiptDetail>();
            _productsCategory=dbContext.Set<ProductCategory>();
            _receipts=dbContext.Set<Receipt>();

        }
        public Task AddAsync(Product entity)
        {
            _products.Add(entity);
            return Task.CompletedTask;
        }

        public void Delete(Product entity)
        {
            var result = entity != null;
            if (result)
            {
                _products.Remove(entity);
            }
        }

        public async Task DeleteByIdAsync(int id)
        {
            Delete(await GetByIdAsync(id));
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return _products;
        }

        public async Task<IEnumerable<Product>> GetAllWithDetailsAsync()
        {
            var products = _products
                .Include(p => p.Category)
                .Include(p => p.ReceiptDetails);

            _productsCategory.Load();
            _receiptsDetails.Load();
            _receipts.Load();

            return products;
        }

        public async Task<Product> GetByIdAsync(int id)
        {
            return await _products.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Product> GetByIdWithDetailsAsync(int id)
        {
            return (await GetAllWithDetailsAsync()).FirstOrDefault(c => c.Id == id);
        }

        public void Update(Product entity)
        {
            _products.Update(entity);
        }
    }
}
