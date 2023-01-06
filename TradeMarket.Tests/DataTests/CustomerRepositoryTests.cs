using Data.Data;
using Data.Entities;
using Data.Repositories;
using Library.Tests;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TradeMarket.Tests.DataTests
{
    [TestFixture]
    public class CustomerRepositoryTests
    {
        [TestCase(1)]
        [TestCase(2)]
        public async Task CustomerRepository_GetByIdAsync_ReturnsSingleValue(int id)
        {
            using var context = new TradeMarketDbContext(UnitTestHelper.GetUnitTestDbOptions());

            var customerRepository = new CustomerRepository(context);

            var customer = await customerRepository.GetByIdAsync(id);

            var expected = ExpectedCustomers.FirstOrDefault(x => x.Id == id);

            Assert.That(customer, Is.EqualTo(expected).Using(new CustomerEqualityComparer()), message: "GetByIdAsync method works incorrect");
        }

        [Test]
        public async Task CustomerRepository_GetAllAsync_ReturnsAllValues()
        {
            using var context = new TradeMarketDbContext(UnitTestHelper.GetUnitTestDbOptions());

            var customerRepository = new CustomerRepository(context);

            var customers = await customerRepository.GetAllAsync();

            Assert.That(customers, Is.EqualTo(ExpectedCustomers).Using(new CustomerEqualityComparer()), message: "GetAllAsync method works incorrect");
        }

        [Test]
        public async Task CustomerRepository_AddAsync_AddsValueToDatabase()
        {
            using var context = new TradeMarketDbContext(UnitTestHelper.GetUnitTestDbOptions());

            var customerRepository = new CustomerRepository(context);
            var customer = new Customer { Id = 3 };

            await customerRepository.AddAsync(customer);
            await context.SaveChangesAsync();

            Assert.That(context.Customers.Count(), Is.EqualTo(3), message: "AddAsync method works incorrect");
        }

        [Test]
        public async Task CustomerRepository_DeleteByIdAsync_DeletesEntity()
        {
            using var context = new TradeMarketDbContext(UnitTestHelper.GetUnitTestDbOptions());

            var customerRepository = new CustomerRepository(context);

            await customerRepository.DeleteByIdAsync(1);
            await context.SaveChangesAsync();

            Assert.That(context.Customers.Count(), Is.EqualTo(1), message: "DeleteByIdAsync works incorrect");
        }

        [Test]
        public async Task CustomerRepository_Update_UpdatesEntity()
        {
            using var context = new TradeMarketDbContext(UnitTestHelper.GetUnitTestDbOptions());

            var customerRepository = new CustomerRepository(context);
            var customer = new Customer
            {
                Id = 1,
                PersonId = 1,
                DiscountValue = 50
            };

            customerRepository.Update(customer);
            await context.SaveChangesAsync();

            Assert.That(customer, Is.EqualTo(new Customer
            {
                Id = 1,
                PersonId = 1,
                DiscountValue = 50
            }).Using(new CustomerEqualityComparer()), message: "Update method works incorrect");
        }

        [Test]
        public async Task CustomerRepository_GetByIdWithDetailsAsync_ReturnsWithIncludedEntities()
        {
            using var context = new TradeMarketDbContext(UnitTestHelper.GetUnitTestDbOptions());

            var customerRepository = new CustomerRepository(context);

            var customer = await customerRepository.GetByIdWithDetailsAsync(1);

            var expected = ExpectedCustomers.FirstOrDefault(x => x.Id == 1);

            Assert.That(customer, 
                Is.EqualTo(expected).Using(new CustomerEqualityComparer()), message: "GetByIdWithDetailsAsync method works incorrect");
            
            Assert.That(customer.Receipts, 
                Is.EqualTo(ExpectedReceipts.Where(i => i.CustomerId == expected.Id)).Using(new ReceiptEqualityComparer()), message: "GetByIdWithDetailsAsync method doesnt't return included entities");
            
            Assert.That(customer.Receipts.SelectMany(i => i.ReceiptDetails).OrderBy(i => i.Id), 
                Is.EqualTo(ExpectedReceiptsDetails.Where(i => i.ReceiptId == 1 || i.ReceiptId == 2)).Using(new ReceiptDetailEqualityComparer()), message: "GetByIdWithDetailsAsync method doesnt't return included entities");
            
            Assert.That(customer.Person, 
                Is.EqualTo(ExpectedPersons.FirstOrDefault(x => x.Id == expected.PersonId)).Using(new PersonEqualityComparer()), message: "GetByIdWithDetailsAsync method doesnt't return included entities");
        }

        [Test]
        public async Task CustomerRepository_GetAllWithDetailsAsync_ReturnsWithIncludedEntities()
        {
            using var context = new TradeMarketDbContext(UnitTestHelper.GetUnitTestDbOptions());

            var customerRepository = new CustomerRepository(context);

            var customers = await customerRepository.GetAllWithDetailsAsync();

            Assert.That(customers, 
                Is.EqualTo(ExpectedCustomers).Using(new CustomerEqualityComparer()), message: "GetAllWithDetailsAsync method works incorrect");

            Assert.That(customers.SelectMany(i => i .Receipts).OrderBy(i => i.Id), 
                Is.EqualTo(ExpectedReceipts).Using(new ReceiptEqualityComparer()), message: "GetAllWithDetailsAsync method doesnt't return included entities");
            
            Assert.That(customers.SelectMany(i => i.Receipts).SelectMany(i => i.ReceiptDetails).OrderBy(i => i.Id),
                Is.EqualTo(ExpectedReceiptsDetails).Using(new ReceiptDetailEqualityComparer()), message: "GetByIdWithDetailsAsync method doesnt't return included entities");
            
            Assert.That(customers.Select(i => i.Person).OrderBy(i => i.Id), 
                Is.EqualTo(ExpectedPersons).Using(new PersonEqualityComparer()), message: "GetByIdWithDetailsAsync method doesnt't return included entities");
        }

        private static IEnumerable<Person> ExpectedPersons =>
            new[]
            {
                new Person { Id = 1, Name = "Name1", Surname = "Surname1", BirthDate = new DateTime(1980, 5, 25) },
                new Person { Id = 2, Name = "Name2", Surname = "Surname2", BirthDate = new DateTime(1984, 10, 19) }
            };

        private static IEnumerable<Customer> ExpectedCustomers =>
            new[]
            {
                new Customer { Id = 1, PersonId = 1, DiscountValue = 20 },
                new Customer { Id = 2, PersonId = 2, DiscountValue = 10 }
            };

        private static IEnumerable<Receipt> ExpectedReceipts =>
            new[]
            {
                new Receipt { Id = 1, CustomerId = 1, OperationDate = new DateTime(2021, 7, 5), IsCheckedOut = true },
                new Receipt { Id = 2, CustomerId = 1, OperationDate = new DateTime(2021, 8, 10), IsCheckedOut = true },
                new Receipt { Id = 3, CustomerId = 2, OperationDate = new DateTime(2021, 10, 15), IsCheckedOut = false }
            };

        private static IEnumerable<ReceiptDetail> ExpectedReceiptsDetails =>
            new[]
            {
                new ReceiptDetail { Id = 1, ReceiptId = 1, ProductId = 1, UnitPrice = 20, DiscountUnitPrice = 16, Quantity = 3 },
                new ReceiptDetail { Id = 2, ReceiptId = 1, ProductId = 2, UnitPrice = 50, DiscountUnitPrice = 40, Quantity = 1 },
                new ReceiptDetail { Id = 3, ReceiptId = 2, ProductId = 2, UnitPrice = 50, DiscountUnitPrice = 40, Quantity = 2 },
                new ReceiptDetail { Id = 4, ReceiptId = 3, ProductId = 1, UnitPrice = 20, DiscountUnitPrice = 18, Quantity = 2 },
                new ReceiptDetail { Id = 5, ReceiptId = 3, ProductId = 2, UnitPrice = 50, DiscountUnitPrice = 45, Quantity = 5 }
            };
    }
}
