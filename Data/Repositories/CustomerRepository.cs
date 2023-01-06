using Data.Data;
using Data.Entities;
using Data.EqualityComparers;
using Data.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly DbSet<Customer> _customers;
        private readonly DbSet<Person> _persons;
        private readonly DbSet<Receipt> _receipt;
        private readonly DbSet<ReceiptDetail> _receiptsDetails;
        private readonly ITradeMarketDbContext _dbContext;
        public CustomerRepository(ITradeMarketDbContext dbContext)
        {
            _customers=dbContext.Set<Customer>();
            _persons=dbContext.Set<Person>();
            _receipt=dbContext.Set<Receipt>();
            _receiptsDetails=dbContext.Set<ReceiptDetail>();
            _dbContext=dbContext;

        }
        public Task AddAsync(Customer entity)
        {
            _customers.Add(entity);
            return Task.CompletedTask;
        }

        public void Delete(Customer entity)
        {
            var result = entity != null;
            if(result)
            {
                _customers.Remove(entity);
            }
        }

        public async Task DeleteByIdAsync(int id)
        {
            Delete(await GetByIdAsync(id));
        }

        public async Task<IEnumerable<Customer>> GetAllAsync()
        {
            return _customers;
        }

        public async Task<IEnumerable<Customer>> GetAllWithDetailsAsync()
        {
            var customers = _customers.Include(c => c.Person)
                .Include(c => c.Receipts)
                .ThenInclude(x => x.ReceiptDetails);
            _persons.Load();
            _receipt.Load();
            _receiptsDetails.Load();
            return customers;
        }

        public async Task<Customer> GetByIdAsync(int id)
        {
            return await _customers.FirstOrDefaultAsync(c=>c.Id==id);
        }

        public async Task<Customer> GetByIdWithDetailsAsync(int id)
        {
            return (await GetAllWithDetailsAsync()).FirstOrDefault(c => c.Id == id);
        }

        public async void Update(Customer entity)
        {
            return;
            var oldEntity = await GetByIdWithDetailsAsync(entity.Id);

            if (oldEntity == null)
            {
                return;
            }

            if (entity.PersonId == 0)
            {
                entity.PersonId = oldEntity.PersonId;
                entity.Person.Id = oldEntity.PersonId;
            }

            var person = entity.Person;

            var personComparer = new PersonEqualityComparer();
            if (!personComparer.Equals(oldEntity.Person, person))
            {
                await DeleteByIdAsync(entity.Id);

                _persons.Remove(await _persons.FirstOrDefaultAsync(p => p.Id == person.Id));

                _persons.Add(person);
            }

            _customers.Update(entity);            
        }
    }
}
