using Data.Data;
using Data.Entities;
using Data.Repositories;
using Library.Tests;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TradeMarket.Tests.DataTests
{
    [TestFixture]
    public class ProductRepositoryTests
    {
        [TestCase(1)]
        [TestCase(2)]
        public async Task ProductRepository_GetByIdAsync_ReturnsSingleValue(int id)
        {
            using var context = new TradeMarketDbContext(UnitTestHelper.GetUnitTestDbOptions());

            var productRepository = new ProductRepository(context);

            var product = await productRepository.GetByIdAsync(id);

            var expected = ExpectedProducts.FirstOrDefault(x => x.Id == id);

            Assert.That(product, Is.EqualTo(expected).Using(new ProductEqualityComparer()), message: "GetByIdAsync method works incorrect");
        }

        [Test]
        public async Task ProductRepository_GetAllAsync_ReturnsAllValues()
        {
            using var context = new TradeMarketDbContext(UnitTestHelper.GetUnitTestDbOptions());

            var productRepository = new ProductRepository(context);

            var products = await productRepository.GetAllAsync();

            Assert.That(products, Is.EqualTo(ExpectedProducts).Using(new ProductEqualityComparer()), message: "GetAllAsync method works incorrect");
        }

        [Test]
        public async Task ProductRepository_AddAsync_AddsValueToDatabase()
        {
            using var context = new TradeMarketDbContext(UnitTestHelper.GetUnitTestDbOptions());

            var productRepository = new ProductRepository(context);
            var product = new Product { Id = 3 };

            await productRepository.AddAsync(product);
            await context.SaveChangesAsync();

            Assert.That(context.Products.Count(), Is.EqualTo(3), message: "AddAsync method works incorrect");
        }

        [Test]
        public async Task ProductRepository_DeleteByIdAsync_DeletesEntity()
        {
            using var context = new TradeMarketDbContext(UnitTestHelper.GetUnitTestDbOptions());

            var productRepository = new ProductRepository(context);

            await productRepository.DeleteByIdAsync(1);
            await context.SaveChangesAsync();

            Assert.That(context.Products.Count(), Is.EqualTo(1), message: "DeleteByIdAsync works incorrect");
        }

        [Test]
        public async Task ProductRepository_Update_UpdatesEntity()
        {
            using var context = new TradeMarketDbContext(UnitTestHelper.GetUnitTestDbOptions());

            var productRepository = new ProductRepository(context);
            var product = new Product
            {
                Id = 1,
                ProductCategoryId = 1,
                ProductName = "Yogurt",
                Price = 30
            };

            productRepository.Update(product);
            await context.SaveChangesAsync();

            Assert.That(product, Is.EqualTo(new Product
            {
                Id = 1,
                ProductCategoryId = 1,
                ProductName = "Yogurt",
                Price = 30
            }).Using(new ProductEqualityComparer()), message: "Update method works incorrect");
        }

        [Test]
        public async Task ProductRepository_GetByIdWithDetailsAsync_ReturnsWithIncludedEntities()
        {
            using var context = new TradeMarketDbContext(UnitTestHelper.GetUnitTestDbOptions());

            var productRepository = new ProductRepository(context);

            var product = await productRepository.GetByIdWithDetailsAsync(1);

            var expected = ExpectedProducts.FirstOrDefault(x => x.Id == 1);
            var expectedReceiptDetailsCount = 2;

            Assert.That(product, Is.EqualTo(expected).Using(new ProductEqualityComparer()), message: "GetByIdWithDetailsAsync method works incorrect");
            Assert.That(product.ReceiptDetails.Count, Is.EqualTo(expectedReceiptDetailsCount), message: "GetByIdWithDetailsAsync method doesnt't return included entities");
            Assert.That(product.Category, Is.Not.Null, message: "GetByIdWithDetailsAsync method doesnt't return included entities");
        }

        [Test]
        public async Task ProductRepository_GetAllWithDetailsAsync_ReturnsWithIncludedEntities()
        {
            using var context = new TradeMarketDbContext(UnitTestHelper.GetUnitTestDbOptions());

            var productRepository = new ProductRepository(context);

            var products = await productRepository.GetAllWithDetailsAsync();
            var product = products.FirstOrDefault(x => x.Id == 1);

            var expectedReceiptDetailsCount = 2;

            Assert.That(products, Is.EqualTo(ExpectedProducts).Using(new ProductEqualityComparer()), message: "GetAllWithDetailsAsync method works incorrect");
            Assert.That(product.ReceiptDetails.Count, Is.EqualTo(expectedReceiptDetailsCount), message: "GetAllWithDetailsAsync method doesnt't return included entities");
            Assert.That(product.Category, Is.Not.Null, message: "GetByIdWithDetailsAsync method doesnt't return included entities");
        }

        private static IEnumerable<Product> ExpectedProducts =>
            new[]
            {
                new Product { Id = 1, ProductCategoryId = 1, ProductName = "Name1", Price = 20 },
                new Product { Id = 2, ProductCategoryId = 2, ProductName = "Name2", Price = 50 }
            };
    }
}
