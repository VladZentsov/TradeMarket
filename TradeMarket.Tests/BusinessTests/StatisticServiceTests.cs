using Data.Entities;
using NUnit.Framework;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Interfaces;
using Moq;
using System.Linq;
using Business.Models;
using Business.Services;
using Library.Tests;

namespace TradeMarket.Tests.BusinessTests
{
    public class StatisticServiceTests
    {
        [TestCase(2)]
        [TestCase(3)]
        public async Task StatisticService_GetMostPopularProducts_ReturnsMostPopularProductsOrderedBySalesCount(int productsCount)
        {
            //arrange 
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(x => x.ReceiptDetailRepository.GetAllWithDetailsAsync()).ReturnsAsync(ReceiptDetailEntities.AsEnumerable());
            var statisticService = new StatisticService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());

            //act
            var actual = await statisticService.GetMostPopularProductsAsync(productsCount);

            //assert
            var expected = ExpectedMostPopularProducts.Take(productsCount);
            actual.Should().BeEquivalentTo(expected, options => options.Excluding(x => x.ReceiptDetailIds));
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public async Task StatisticService_GetMostValuableCustomersAsync_ReturnsCustomersOrderedByAllReceiptsSum(int customerCount)
        {
            //arrange 
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(x => x.ReceiptRepository.GetAllWithDetailsAsync()).ReturnsAsync(ReceiptEntities.AsEnumerable());
            var statisticService = new StatisticService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());

            //act
            var actual = await statisticService.GetMostValuableCustomersAsync(customerCount, new DateTime(2000, 1, 1), new DateTime(2022, 1, 1));

            //assert
            var expected = ExpectedCustomerActivityModels.Take(customerCount);
            actual.Should().BeInDescendingOrder(c => c.ReceiptSum).And.BeEquivalentTo(expected);
        }

        [Test]
        public async Task StatisticService_GetCustomersMostPopularProductsAsync_ReturnsCustomersTopProductsOrderedBySalesCount()
        {
            //arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(x => x.ReceiptRepository.GetAllWithDetailsAsync()).ReturnsAsync(ReceiptEntities.AsEnumerable());
            var statisticService = new StatisticService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());

            //act
            var actual = await statisticService.GetCustomersMostPopularProductsAsync(3, 1);

            //assert
            var expected = ExpectedCustomersMostPopularProducts;

            actual.Should().BeEquivalentTo(expected, options =>
                    options.Excluding(x => x.ReceiptDetailIds)
                );
        }

        [TestCase(1, 260)]
        [TestCase(2, 48)]
        public async Task StatisticService_GetIncomeOfCategoryInPeriod_ReturnsSumOfCategoryProductsSalesInPeriod(int categoryId, decimal sum)
        {
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(x => x.ReceiptRepository.GetAllWithDetailsAsync()).ReturnsAsync(ReceiptEntities.AsEnumerable());
            var statisticService = new StatisticService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());

            //act
            var actual = await statisticService.GetIncomeOfCategoryInPeriod(categoryId, new DateTime(2021, 1, 1), new DateTime(2021, 2, 2));

            //assert
            Assert.That(actual.Equals(sum));
        }

        #region TestData

        public static IEnumerable<Product> ProductEntities =
            new List<Product>
            {
                new Product { Id = 1, ProductName = "Chai", ProductCategoryId = 1, Category = new ProductCategory { Id = 1, CategoryName = "Beverages" }, Price = 18.00m },
                new Product { Id = 2, ProductName = "Chang", ProductCategoryId = 1, Category = new ProductCategory { Id = 1, CategoryName = "Beverages" }, Price = 19.00m },
                new Product { Id = 3, ProductName = "Aniseed Syrup", ProductCategoryId = 2, Category = new ProductCategory { Id = 2, CategoryName = "Condiments" }, Price = 10.00m },
                new Product { Id = 4, ProductName = "Chef Anton's Cajun Seasoning", ProductCategoryId = 2, Category = new ProductCategory { Id = 2, CategoryName = "Condiments" }, Price = 22.00m },
                new Product { Id = 5, ProductName = "Grandma's Boysenberry Spread", ProductCategoryId = 3, Category = new ProductCategory { Id = 3, CategoryName = "Confections" }, Price = 25.00m },
                new Product { Id = 6, ProductName = "Uncle Bob's Organic Dried Pears", ProductCategoryId = 4, Category = new ProductCategory { Id = 4, CategoryName = "Dairy Products" }, Price = 14.60m },
            };

        public static IEnumerable<ProductModel> ExpectedMostPopularProducts =
            new List<ProductModel>()
            {
                new ProductModel { Id = 1, ProductName = "Chai", ProductCategoryId = 1, CategoryName =  "Beverages", Price = 18.00m },
                new ProductModel { Id = 2, ProductName = "Chang", ProductCategoryId = 1, CategoryName =  "Beverages", Price = 19.00m },
                new ProductModel { Id = 5, ProductName = "Grandma's Boysenberry Spread", ProductCategoryId = 3, CategoryName = "Confections", Price = 25.00m },
            };

        public static IEnumerable<ReceiptDetail> ReceiptDetailEntities =
            new List<ReceiptDetail>()
            {
                new ReceiptDetail { Id = 1, ProductId = 1, UnitPrice = 10, Product = ProductEntities.ElementAt(0), DiscountUnitPrice = 9, Quantity = 2, ReceiptId = 1 },
                new ReceiptDetail { Id = 2, ProductId = 2, UnitPrice = 20, Product = ProductEntities.ElementAt(1), DiscountUnitPrice = 19, Quantity = 8, ReceiptId = 1},
                new ReceiptDetail { Id = 3, ProductId = 3, UnitPrice = 25, Product = ProductEntities.ElementAt(2), DiscountUnitPrice = 24, Quantity = 1, ReceiptId = 1 },
                new ReceiptDetail { Id = 4, ProductId = 1, UnitPrice = 10, Product = ProductEntities.ElementAt(0), DiscountUnitPrice = 9, Quantity = 10, ReceiptId = 2 },
                new ReceiptDetail { Id = 5, ProductId = 3, UnitPrice = 25, Product = ProductEntities.ElementAt(2), DiscountUnitPrice = 24, Quantity = 1, ReceiptId = 2},
                new ReceiptDetail { Id = 6, ProductId = 5, UnitPrice = 10, Product = ProductEntities.ElementAt(4), DiscountUnitPrice= 9, Quantity = 5, ReceiptId = 1},
            };

        public static IEnumerable<Customer> CustomerEntities =
            new List<Customer>()
            {
                new Customer { Id = 1, PersonId = 1, Person = new Person { Id = 1, Name = "Viktor", Surname = "Zhuk", BirthDate = new DateTime(1995, 1, 2) }, DiscountValue = 10},
                new Customer { Id = 2, PersonId = 2, Person = new Person { Id = 2, Name = "Nassim", Surname = "Taleb", BirthDate = new DateTime(1965, 5, 12) }, DiscountValue = 15},
                new Customer { Id = 3, PersonId = 3, Person =  new Person { Id = 3, Name = "Desmond", Surname = "Morris",  BirthDate = new DateTime(1955, 4, 12) }, DiscountValue = 5},
                new Customer { Id = 4, PersonId = 4, Person =  new Person { Id = 4, Name = "Lebron", Surname = "James", BirthDate = new DateTime(1983, 12, 31) }, DiscountValue = 12}
            };

        public static IEnumerable<Receipt> ReceiptEntities =
            new List<Receipt>()
            {
                new Receipt
                {
                    Id = 1, CustomerId = 1, Customer = CustomerEntities.ElementAt(0), IsCheckedOut = false, OperationDate = new DateTime(2021, 1, 2),
                    ReceiptDetails = new List<ReceiptDetail>()
                    {
                        new ReceiptDetail { Id = 1, ProductId = 1, UnitPrice = 10, Product = ProductEntities.ElementAt(0), DiscountUnitPrice = 9, Quantity = 2, ReceiptId = 1 },
                        new ReceiptDetail { Id = 2, ProductId = 2, UnitPrice = 20, Product = ProductEntities.ElementAt(1), DiscountUnitPrice = 19, Quantity = 8, ReceiptId = 1},
                        new ReceiptDetail { Id = 3, ProductId = 3, UnitPrice = 25, Product = ProductEntities.ElementAt(2), DiscountUnitPrice = 24, Quantity = 1, ReceiptId = 1 },
                    }
                },
                new Receipt
                {
                    Id = 2, CustomerId = 2, Customer = CustomerEntities.ElementAt(1), IsCheckedOut = false, OperationDate = new DateTime(2021, 1, 15),
                    ReceiptDetails = new List<ReceiptDetail>()
                    {
                        new ReceiptDetail { Id = 4, ProductId = 1, UnitPrice = 10, Product = ProductEntities.ElementAt(0), DiscountUnitPrice = 9, Quantity = 10, ReceiptId = 2 },
                        new ReceiptDetail { Id = 5, ProductId = 3, UnitPrice = 25, Product = ProductEntities.ElementAt(2), DiscountUnitPrice = 24, Quantity = 1, ReceiptId = 2 }
                    }
                },
                new Receipt
                {
                    Id = 3, CustomerId = 1, Customer = CustomerEntities.ElementAt(0), IsCheckedOut = false, OperationDate = new DateTime(2021, 2, 15),
                    ReceiptDetails = new List<ReceiptDetail>()
                    {
                        new ReceiptDetail { Id = 6, ProductId = 1, UnitPrice = 10, Product = ProductEntities.ElementAt(0), DiscountUnitPrice = 9, Quantity = 10, ReceiptId = 3 },
                        new ReceiptDetail { Id = 7, ProductId = 2, UnitPrice = 25, Product = ProductEntities.ElementAt(1), DiscountUnitPrice = 24, Quantity = 1, ReceiptId = 3 }
                    }
                },
                new Receipt
                {
                    Id = 4, CustomerId = 3, Customer = CustomerEntities.ElementAt(2), IsCheckedOut = false, OperationDate = new DateTime(2021, 2, 28),
                    ReceiptDetails = new List<ReceiptDetail>()
                    {
                        new ReceiptDetail { Id = 8, ProductId = 5, UnitPrice = 10, Product = ProductEntities.ElementAt(4), DiscountUnitPrice = 9, Quantity = 15, ReceiptId = 4 },
                        new ReceiptDetail { Id = 9, ProductId = 6, UnitPrice = 25, Product = ProductEntities.ElementAt(5), DiscountUnitPrice = 24, Quantity = 1, ReceiptId = 4 }
                    }
                }
            };

        public static IEnumerable<CustomerActivityModel> ExpectedCustomerActivityModels =
            new List<CustomerActivityModel>()
            {
                new CustomerActivityModel { CustomerId = 1, CustomerName  = "Viktor Zhuk", ReceiptSum = 308 },
                new CustomerActivityModel { CustomerId = 3, CustomerName = "Desmond Morris", ReceiptSum = 159 },
                new CustomerActivityModel { CustomerId = 2, CustomerName = "Nassim Taleb", ReceiptSum = 114 }
            };

        public static IEnumerable<ProductModel> ExpectedCustomersMostPopularProducts =
            new List<ProductModel>()
            {
                new ProductModel { Id = 1, ProductName = "Chai", ProductCategoryId = 1, CategoryName =  "Beverages", Price = 18.00m },
                new ProductModel { Id = 2, ProductName = "Chang", ProductCategoryId = 1, CategoryName =  "Beverages", Price = 19.00m },
                new ProductModel { Id = 3, ProductName = "Aniseed Syrup", ProductCategoryId = 2, CategoryName = "Condiments", Price = 10.00m  }
            };

        #endregion
    }
}
