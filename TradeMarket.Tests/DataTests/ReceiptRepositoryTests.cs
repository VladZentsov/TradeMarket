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
    public class ReceiptRepositoryTests
    {
        [TestCase(1)]
        [TestCase(3)]
        public async Task ReceiptRepository_GetByIdAsync_ReturnsSingleValue(int id)
        {
            using var context = new TradeMarketDbContext(UnitTestHelper.GetUnitTestDbOptions());

            var receiptRepository = new ReceiptRepository(context);
            var receipt = await receiptRepository.GetByIdAsync(id);

            var expected = ExpectedReceipts.FirstOrDefault(x => x.Id == id);

            Assert.That(receipt, Is.EqualTo(expected).Using(new ReceiptEqualityComparer()), message: "GetByIdAsync method works incorrect");
        }

        [Test]
        public async Task ReceiptRepository_GetAllAsync_ReturnsAllValues()
        {
            using var context = new TradeMarketDbContext(UnitTestHelper.GetUnitTestDbOptions());

            var receiptRepository = new ReceiptRepository(context);
            var receipts = await receiptRepository.GetAllAsync();

            Assert.That(receipts, Is.EqualTo(ExpectedReceipts).Using(new ReceiptEqualityComparer()), message: "GetAllAsync method works incorrect");
        }

        [Test]
        public async Task ReceiptRepository_AddAsync_AddsValueToDatabase()
        {
            using var context = new TradeMarketDbContext(UnitTestHelper.GetUnitTestDbOptions());

            var receiptRepository = new ReceiptRepository(context);
            var receipt = new Receipt { Id = 4 };

            await receiptRepository.AddAsync(receipt);
            await context.SaveChangesAsync();

            Assert.That(context.Receipts.Count(), Is.EqualTo(4), message: "AddAsync method works incorrect");
        }

        [Test]
        public async Task ReceiptRepository_DeleteByIdAsync_DeletesEntity()
        {
            using var context = new TradeMarketDbContext(UnitTestHelper.GetUnitTestDbOptions());

            var receiptRepository = new ReceiptRepository(context);

            await receiptRepository.DeleteByIdAsync(1);
            await context.SaveChangesAsync();

            Assert.That(context.Receipts.Count(), Is.EqualTo(2), message: "DeleteByIdAsync works incorrect");
        }

        [Test]
        public async Task ReceiptRepository_Update_UpdatesEntity()
        {
            using var context = new TradeMarketDbContext(UnitTestHelper.GetUnitTestDbOptions());

            var receiptRepository = new ReceiptRepository(context);
            var receipt = new Receipt
            {
                Id = 1,
                CustomerId = 2,
                OperationDate = new DateTime(2021, 10, 5),
                IsCheckedOut = false
            };

            receiptRepository.Update(receipt);
            await context.SaveChangesAsync();

            Assert.That(receipt, Is.EqualTo(new Receipt
            {
                Id = 1,
                CustomerId = 2,
                OperationDate = new DateTime(2021, 10, 5),
                IsCheckedOut = false
            }).Using(new ReceiptEqualityComparer()), message: "Update method works incorrect");
        }

        [Test]
        public async Task ReceiptRepository_GetByIdWithDetailsAsync_ReturnsWithIncludedEntities()
        {
            using var context = new TradeMarketDbContext(UnitTestHelper.GetUnitTestDbOptions());

            var receiptRepository = new ReceiptRepository(context);

            var receipt = await receiptRepository.GetByIdWithDetailsAsync(1);

            var expected = ExpectedReceipts.FirstOrDefault(x => x.Id == 1);

            Assert.That(receipt, 
                Is.EqualTo(expected).Using(new ReceiptEqualityComparer()), message: "GetByIdWithDetailsAsync method works incorrect");
            
            Assert.That(receipt.ReceiptDetails, 
                Is.EqualTo(ExpectedReceiptsDetails.Where(i => i.ReceiptId == expected.Id).OrderBy(i => i.Id)).Using(new ReceiptDetailEqualityComparer()), message: "GetByIdWithDetailsAsync method doesnt't return included entities");
            
            Assert.That(receipt.ReceiptDetails.Select(i => i.Product).OrderBy(i => i.Id), 
                Is.EqualTo(ExpectedProducts.Where(i => i.Id == 1 || i.Id == 2)).Using(new ProductEqualityComparer()), message: "GetByIdWithDetailsAsync method doesnt't return included entities");

            Assert.That(receipt.ReceiptDetails.Select(i => i.Product.Category).OrderBy(i => i.Id),
                Is.EqualTo(ExpectedProductCategories.Where(i => i.Id == 1 || i.Id == 2)).Using(new ProductCategoryEqualityComparer()), message: "GetByIdWithDetailsAsync method doesnt't return included entities");

            Assert.That(receipt.Customer, 
                Is.EqualTo(ExpectedCustomers.FirstOrDefault(i => i.Id == expected.CustomerId)).Using(new CustomerEqualityComparer()), message: "GetByIdWithDetailsAsync method doesnt't return included entities");
        }

        [Test]
        public async Task ReceiptRepository_GetAllWithDetailsAsync_ReturnsWithIncludedEntities()
        {
            using var context = new TradeMarketDbContext(UnitTestHelper.GetUnitTestDbOptions());

            var receiptRepository = new ReceiptRepository(context);

            var receipts = await receiptRepository.GetAllWithDetailsAsync();

            Assert.That(receipts, 
                Is.EqualTo(ExpectedReceipts).Using(new ReceiptEqualityComparer()), message: "GetAllWithDetailsAsync method works incorrect");
            
            Assert.That(receipts.SelectMany(i => i.ReceiptDetails).OrderBy(i => i.Id), 
                Is.EqualTo(ExpectedReceiptsDetails).Using(new ReceiptDetailEqualityComparer()), message: "GetAllWithDetailsAsync method doesnt't return included entities");

            Assert.That(receipts.SelectMany(i => i.ReceiptDetails).Select(i => i.Product).Distinct().OrderBy(i => i.Id),
                Is.EqualTo(ExpectedProducts).Using(new ProductEqualityComparer()), message: "GetByIdWithDetailsAsync method doesnt't return included entities");

            Assert.That(receipts.SelectMany(i => i.ReceiptDetails).Select(i => i.Product.Category).Distinct().OrderBy(i => i.Id),
                Is.EqualTo(ExpectedProductCategories).Using(new ProductCategoryEqualityComparer()), message: "GetByIdWithDetailsAsync method doesnt't return included entities");

            Assert.That(receipts.Select(i => i.Customer).Distinct().OrderBy(i => i.Id),
                Is.EqualTo(ExpectedCustomers).Using(new CustomerEqualityComparer()), message: "GetAllWithDetailsAsync method doesnt't return included entities");
        }

        private static IEnumerable<Receipt> ExpectedReceipts =>
            new[]
            {
                new Receipt { Id = 1, CustomerId = 1, OperationDate = new DateTime(2021, 7, 5), IsCheckedOut = true },
                new Receipt { Id = 2, CustomerId = 1, OperationDate = new DateTime(2021, 8, 10), IsCheckedOut = true },
                new Receipt { Id = 3, CustomerId = 2, OperationDate = new DateTime(2021, 10, 15), IsCheckedOut = false }
            };

        private static IEnumerable<Product> ExpectedProducts =>
            new[]
            {
                new Product { Id = 1, ProductCategoryId = 1, ProductName = "Name1", Price = 20 },
                new Product { Id = 2, ProductCategoryId = 2, ProductName = "Name2", Price = 50 }
            };

        private static IEnumerable<ProductCategory> ExpectedProductCategories =>
            new[]
            {
                new ProductCategory { Id = 1, CategoryName = "Category1" },
                new ProductCategory { Id = 2, CategoryName = "Category2" }
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

        private static IEnumerable<Customer> ExpectedCustomers =>
            new[]
            {
                new Customer { Id = 1, PersonId = 1, DiscountValue = 20 },
                new Customer { Id = 2, PersonId = 2, DiscountValue = 10 }
            };
    }
}
