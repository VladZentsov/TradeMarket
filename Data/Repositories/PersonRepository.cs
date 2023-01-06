using Data.Data;
using Data.Entities;
using Data.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repositories
{
    public class PersonRepository : IPersonRepository
    {
        private readonly DbSet<Person> _persons;
        public PersonRepository(ITradeMarketDbContext dbContext)
        {
            _persons = dbContext.Set<Person>();
        }
        public Task AddAsync(Person entity)
        {
            _persons.Add(entity);
            return Task.CompletedTask;
        }

        public void Delete(Person entity)
        {
            var result = entity != null;
            if (result)
            {
                _persons.Remove(entity);
            }
        }

        public async Task DeleteByIdAsync(int id)
        {
            Delete(await GetByIdAsync(id));
        }

        public async Task<IEnumerable<Person>> GetAllAsync()
        {
            return _persons;
        }

        public async Task<IEnumerable<Person>> GetAllWithDetailsAsync()
        {
            return await GetAllAsync();
        }

        public async Task<Person> GetByIdAsync(int id)
        {
            return await _persons.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Person> GetByIdWithDetailsAsync(int id)
        {
            return await GetByIdAsync(id);
        }

        public void Update(Person entity)
        {
            _persons.Update(entity);
        }
    }
}
