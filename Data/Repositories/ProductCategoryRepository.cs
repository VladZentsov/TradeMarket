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
    public class ProductCategoryRepository : IProductCategoryRepository
    {
        private readonly DbSet<ProductCategory> _productCategories;
        public ProductCategoryRepository(ITradeMarketDbContext dbContext)
        {
            _productCategories = dbContext.Set<ProductCategory>();
        }
        public Task AddAsync(ProductCategory entity)
        {
            _productCategories.Add(entity);
            return Task.CompletedTask;
        }

        public void Delete(ProductCategory entity)
        {
            var result = entity != null;
            if (result)
            {
                _productCategories.Remove(entity);
            }
        }

        public async Task DeleteByIdAsync(int id)
        {
            Delete(await GetByIdAsync(id));
        }

        public async Task<IEnumerable<ProductCategory>> GetAllAsync()
        {
            return _productCategories;
        }

        public async Task<IEnumerable<ProductCategory>> GetAllWithDetailsAsync()
        {
            return await GetAllAsync();
        }

        public async Task<ProductCategory> GetByIdAsync(int id)
        {
            return await _productCategories.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<ProductCategory> GetByIdWithDetailsAsync(int id)
        {
            return await GetByIdAsync(id);
        }

        public void Update(ProductCategory entity)
        {
            _productCategories.Update(entity);
        }
    }
}
