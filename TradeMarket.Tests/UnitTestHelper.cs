using Data.Data;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using AutoMapper;
using Business;

namespace Library.Tests
{
    internal static class UnitTestHelper
    {
        public static DbContextOptions<TradeMarketDbContext> GetUnitTestDbOptions()
        {
            var options = new DbContextOptionsBuilder<TradeMarketDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using (var context = new TradeMarketDbContext(options))
            {
                SeedData(context);
            }

            return options;
        }

        public static IMapper CreateMapperProfile()
        {
            var myProfile = new AutomapperProfile();
            var configuration = new MapperConfiguration(cfg => cfg.AddProfile(myProfile));

            return new Mapper(configuration);
        }

        public static void SeedData(TradeMarketDbContext context)
        {
            context.Persons.AddRange(
                new Person { Id = 1, Name = "Name1", Surname = "Surname1", BirthDate = new DateTime(1980, 5, 25) },
                new Person { Id = 2, Name = "Name2", Surname = "Surname2", BirthDate = new DateTime(1984, 10, 19) });
            context.Customers.AddRange(
                new Customer { Id = 1, PersonId = 1, DiscountValue = 20 },
                new Customer { Id = 2, PersonId = 2, DiscountValue = 10 });
            context.ProductCategories.AddRange(
                new ProductCategory { Id = 1, CategoryName = "Category1" },
                new ProductCategory { Id = 2, CategoryName = "Category2" });
            context.Products.AddRange(
                new Product { Id = 1, ProductCategoryId = 1, ProductName = "Name1", Price = 20 },
                new Product { Id = 2, ProductCategoryId = 2, ProductName = "Name2", Price = 50 });
            context.Receipts.AddRange(
                new Receipt { Id = 1, CustomerId = 1, OperationDate = new DateTime(2021, 7, 5), IsCheckedOut = true },
                new Receipt { Id = 2, CustomerId = 1, OperationDate = new DateTime(2021, 8, 10), IsCheckedOut = true },
                new Receipt { Id = 3, CustomerId = 2, OperationDate = new DateTime(2021, 10, 15), IsCheckedOut = false });
            context.ReceiptsDetails.AddRange(
                new ReceiptDetail { Id = 1, ReceiptId = 1, ProductId = 1, UnitPrice = 20, DiscountUnitPrice = 16, Quantity = 3 },
                new ReceiptDetail { Id = 2, ReceiptId = 1, ProductId = 2, UnitPrice = 50, DiscountUnitPrice = 40, Quantity = 1 },
                new ReceiptDetail { Id = 3, ReceiptId = 2, ProductId = 2, UnitPrice = 50, DiscountUnitPrice = 40, Quantity = 2 },
                new ReceiptDetail { Id = 4, ReceiptId = 3, ProductId = 1, UnitPrice = 20, DiscountUnitPrice = 18, Quantity = 2 },
                new ReceiptDetail { Id = 5, ReceiptId = 3, ProductId = 2, UnitPrice = 50, DiscountUnitPrice = 45, Quantity = 5 });
            context.SaveChanges();
        }
    }
}
