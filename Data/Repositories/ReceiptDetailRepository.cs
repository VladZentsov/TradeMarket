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
    public class ReceiptDetailRepository : IReceiptDetailRepository
    {
        private readonly DbSet<ReceiptDetail> _receiptsDetails;
        private readonly DbSet<Product> _products;
        private readonly DbSet<Receipt> _receipts;
        private readonly DbSet<ProductCategory> _productCategories;
        public ReceiptDetailRepository(ITradeMarketDbContext dbContext)
        {
            _receiptsDetails = dbContext.Set<ReceiptDetail>();
            _products = dbContext.Set<Product>();
            _receipts= dbContext.Set<Receipt>();
            _productCategories = dbContext.Set<ProductCategory>();
        }
        public Task AddAsync(ReceiptDetail entity)
        {
            _receiptsDetails.Add(entity);
            return Task.CompletedTask;
        }

        public void Delete(ReceiptDetail entity)
        {
            var result = entity != null;
            if (result)
            {
                _receiptsDetails.Remove(entity);
            }
        }

        public async Task DeleteByIdAsync(int id)
        {
            Delete(await GetByIdAsync(id));
        }

        public async Task<IEnumerable<ReceiptDetail>> GetAllAsync()
        {
            return _receiptsDetails;
        }

        public async Task<IEnumerable<ReceiptDetail>> GetAllWithDetailsAsync()
        {
            var receiptsDetails = _receiptsDetails
                .Include(rd => rd.Receipt)
                .Include(rd => rd.Product)
                .ThenInclude(p => p.Category);

            _products.Load();
            _receipts.Load();
            _productCategories.Load();

            return receiptsDetails;
        }

        public async Task<ReceiptDetail> GetByIdAsync(int id)
        {
            return await _receiptsDetails.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<ReceiptDetail> GetByIdWithDetailsAsync(int id)
        {
            return (await GetAllWithDetailsAsync()).FirstOrDefault(c => c.Id == id);
        }

        public void Update(ReceiptDetail entity)
        {
            _receiptsDetails.Update(entity);
        }
    }
}
