using Business.Models;
using Business.Services;
using Data.Entities;
using Data.Interfaces;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Business.Validation;
using Library.Tests;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;

namespace TradeMarket.Tests.BusinessTests
{
    public class ReceiptServiceTests
    {
        [Test]
        public async Task ReceiptService_GetAll_ReturnsAllReceipts()
        {
            //arrange
            var expected = GetTestReceiptsModels;
            var mockUnitOfWork = new Mock<IUnitOfWork>();

            mockUnitOfWork
                .Setup(x => x.ReceiptRepository.GetAllWithDetailsAsync())
                .ReturnsAsync(GetTestReceiptsEntities.AsEnumerable());

            var receiptService = new ReceiptService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());

            //act
            var actual = await receiptService.GetAllAsync();

            //assert
            actual.Should().BeEquivalentTo(expected);
        }

        [TestCase(1)]
        [TestCase(2)]
        public async Task ReceiptService_GetById_ReturnsReceiptModel(int id)
        {
            //arrange
            var expected = GetTestReceiptsModels.FirstOrDefault(x => x.Id == id);

            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork
                .Setup(x => x.ReceiptRepository.GetByIdWithDetailsAsync(It.IsAny<int>()))
                .ReturnsAsync(GetTestReceiptsEntities.FirstOrDefault(x => x.Id == id));

            var receiptService = new ReceiptService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());

            //act
            var actual = await receiptService.GetByIdAsync(1);

            //assert
            actual.Should().BeEquivalentTo(expected);
        }


        [Test]
        public async Task ReceiptService_AddAsync_AddsReceipt()
        {
            //arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(m => m.ReceiptRepository.AddAsync(It.IsAny<Receipt>()));

            var receiptService = new ReceiptService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());
            var receipt = GetTestReceiptsModels.First();

            //act
            await receiptService.AddAsync(receipt);

            //assert
            mockUnitOfWork.Verify(x => x.ReceiptRepository.AddAsync(It.Is<Receipt>(c => c.Id == receipt.Id)), Times.Once);
            mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Once);
        }

        [Test]
        public async Task ReceiptService_UpdateAsync_UpdatesReceipt()
        {
            //arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(m => m.ReceiptRepository.Update(It.IsAny<Receipt>()));

            var receiptService = new ReceiptService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());
            var receipt = GetTestReceiptsModels.First();

            //act
            await receiptService.UpdateAsync(receipt);

            //assert
            mockUnitOfWork.Verify(x => x.ReceiptRepository.Update(It.Is<Receipt>(receipt1 => receipt1.Id == receipt.Id)), Times.Once);
            mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Once);
        }

        [TestCase(1)]
        [TestCase(2)]
        public async Task ReceiptService_DeleteAsync_DeletesReceiptWithDetails(int id)
        {
            //arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            var receipt = GetTestReceiptsEntities.FirstOrDefault(x => x.Id == id);
            int expectedDetailsLength = receipt.ReceiptDetails.Count();

            mockUnitOfWork.Setup(m => m.ReceiptRepository.GetByIdWithDetailsAsync(It.IsAny<int>())).ReturnsAsync(receipt);
            mockUnitOfWork.Setup(m => m.ReceiptRepository.DeleteByIdAsync(It.IsAny<int>()));
            mockUnitOfWork.Setup(m => m.ReceiptDetailRepository.Delete(It.IsAny<ReceiptDetail>()));

            var receiptService = new ReceiptService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());

            //act
            await receiptService.DeleteAsync(id);

            //assert
            mockUnitOfWork.Verify(x => x.ReceiptRepository.DeleteByIdAsync(It.Is<int>(x => x == id)), Times.Once());
            mockUnitOfWork.Verify(x => x.ReceiptDetailRepository.Delete(It.Is<ReceiptDetail>(detail => detail.ReceiptId == id)),
                failMessage: "All existing receipt details must be deleted", times: Times.Exactly(expectedDetailsLength));
            mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Once());
        }


        [TestCase(1)]
        [TestCase(2)]
        public async Task ReceiptService_GetReceiptDetailsAsync_ReturnsDetailsByReceiptId(int receiptId)
        {
            //arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(x => x.ReceiptRepository.GetByIdWithDetailsAsync(It.IsAny<int>()))
                .ReturnsAsync(GetTestReceiptsEntities.FirstOrDefault(x => x.Id == receiptId));
            var receiptService = new ReceiptService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());

            //act
            var actual = await receiptService.GetReceiptDetailsAsync(receiptId);

            //assert
            var expected = GetTestReceiptsEntities.FirstOrDefault(x => x.Id == receiptId)?.ReceiptDetails;

            actual.Should().BeEquivalentTo(expected, options => options.Excluding(x => x.Product).Excluding(x => x.Receipt));
        }


        [TestCase("2021-1-1", "2021-2-1", new[] { 1, 2 })]
        [TestCase("2021-2-1", "2021-3-1", new[] { 3, 4 })]
        public async Task ReceiptService_GetReceiptsByPeriodAsync_ReturnsReceiptsInPeriod(DateTime startDate, DateTime endDate, IEnumerable<int> expectedReceiptIds)
        {
            //arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(x => x.ReceiptRepository.GetAllWithDetailsAsync()).ReturnsAsync(GetTestReceiptsEntities.AsEnumerable());
            var receiptService = new ReceiptService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());

            //act
            var actual = await receiptService.GetReceiptsByPeriodAsync(startDate, endDate);

            //assert
            var expected = GetTestReceiptsModels.Where(x => expectedReceiptIds.Contains(x.Id));

            actual.Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task ReceiptService_ToPayAsync_ReturnsSumByReceiptIdWithDiscount()
        {
            //arrange
            var receipt = new Receipt
            {
                Id = 1,
                ReceiptDetails = new List<ReceiptDetail> {
                new ReceiptDetail { Id = 1, ProductId = 1, UnitPrice = 10, DiscountUnitPrice = 9, Quantity = 2, ReceiptId = 1 },
                new ReceiptDetail { Id = 2, ProductId = 2, UnitPrice = 20, DiscountUnitPrice = 19, Quantity = 3, ReceiptId = 1 },
                new ReceiptDetail { Id = 3, ProductId = 3, UnitPrice = 25, DiscountUnitPrice = 24, Quantity = 1, ReceiptId = 1 }
            }
            };

            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(x => x.ReceiptRepository.GetByIdWithDetailsAsync(It.IsAny<int>())).ReturnsAsync(receipt);
            var receiptService = new ReceiptService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());

            //act
            var actual = await receiptService.ToPayAsync(receipt.Id);

            //assert
            Assert.AreEqual(99, actual);
        }

        [TestCase]
        public async Task ReceiptService_AddProductAsync_CreatesReceiptDetailIfProductWasNotAddedBefore()
        {
            //arrange 
            var productId = 4;
            var discountValue = 15;

            var receipt = new Receipt
            {
                Id = 1,
                Customer = new Customer { Id = 1, DiscountValue = discountValue },
                ReceiptDetails = new List<ReceiptDetail> {
                new ReceiptDetail { Id = 1, ProductId = 1, UnitPrice = 10, DiscountUnitPrice = 9, Quantity = 2, ReceiptId = 1 },
                new ReceiptDetail { Id = 2, ProductId = 2, UnitPrice = 20, DiscountUnitPrice = 19, Quantity = 3, ReceiptId = 1 },
                new ReceiptDetail { Id = 3, ProductId = 3, UnitPrice = 25, DiscountUnitPrice = 24, Quantity = 1, ReceiptId = 1 }
            }
            };

            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(x => x.ReceiptRepository.GetByIdWithDetailsAsync(It.IsAny<int>())).ReturnsAsync(receipt);
            mockUnitOfWork.Setup(x => x.ProductRepository.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(ProductEntities.FirstOrDefault(x => x.Id == productId));
            mockUnitOfWork.Setup(x => x.ReceiptDetailRepository.AddAsync(It.IsAny<ReceiptDetail>()));
            var receiptService = new ReceiptService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());

            //act
            await receiptService.AddProductAsync(productId, 1, 5);

            //assert
            mockUnitOfWork.Verify(x => x.ReceiptDetailRepository.AddAsync(It.Is<ReceiptDetail>(receiptDetail =>
                receiptDetail.ReceiptId == receipt.Id && receiptDetail.ProductId == productId && receiptDetail.Quantity == 5)), Times.Once);
            mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Once);
        }

        [Test]
        public async Task ReceiptService_AddProductAsync_UpdatesQuantityIfProductWasAddedToReceipt()
        {
            //arrange 
            var receipt = new Receipt
            {
                Id = 1,
                ReceiptDetails = new List<ReceiptDetail> {
                new ReceiptDetail { Id = 1, ProductId = 1, UnitPrice = 10, DiscountUnitPrice = 9, Quantity = 2, ReceiptId = 1 },
                new ReceiptDetail { Id = 2, ProductId = 2, UnitPrice = 20, DiscountUnitPrice = 19, Quantity = 3, ReceiptId = 1 },
                new ReceiptDetail { Id = 3, ProductId = 3, UnitPrice = 25, DiscountUnitPrice = 24, Quantity = 1, ReceiptId = 1 }
                }
            };

            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(x => x.ReceiptRepository.GetByIdWithDetailsAsync(It.IsAny<int>())).ReturnsAsync(receipt);
            mockUnitOfWork.Setup(x => x.ReceiptDetailRepository.AddAsync(It.IsAny<ReceiptDetail>()));
            var receiptService = new ReceiptService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());

            //act
            await receiptService.AddProductAsync(1, 1, 5);

            //assert
            var actualQuantity = receipt.ReceiptDetails.First().Quantity;
            actualQuantity.Should().Be(7);
            mockUnitOfWork.Verify(x => x.ReceiptDetailRepository.AddAsync(It.IsAny<ReceiptDetail>()), Times.Never);
            mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Once);
        }


        [TestCase(1, 15, 15.3)]
        public async Task ReceiptService_AddProduct_SetsDiscountUnitPriceValueAccordingToCustomersDiscount(int productId, int discount, decimal expectedDiscountPrice)
        {
            //arrange
            var product = ProductEntities.FirstOrDefault(x => x.Id == productId);
            var receipt = new Receipt { Id = 1, CustomerId = 1, Customer = new Customer { Id = 1, DiscountValue = discount } };

            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(x => x.ReceiptRepository.GetByIdWithDetailsAsync(It.IsAny<int>())).ReturnsAsync(receipt);
            mockUnitOfWork.Setup(x => x.ProductRepository.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(product);
            mockUnitOfWork.Setup(x => x.ReceiptDetailRepository.AddAsync(It.IsAny<ReceiptDetail>()));

            var receiptService = new ReceiptService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());

            //act
            await receiptService.AddProductAsync(1, 1, 2);

            //assert
            mockUnitOfWork.Verify(x => x.ReceiptDetailRepository.AddAsync(It.Is<ReceiptDetail>(detail =>
                detail.ReceiptId == receipt.Id && detail.UnitPrice == product.Price && detail.ProductId == product.Id &&
                detail.DiscountUnitPrice == expectedDiscountPrice)), Times.Once);

            mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Once);
        }


        [TestCase(1)]
        [TestCase(2)]
        public async Task ReceiptService_AddProduct_ThrowsMarketExceptionIfProductDoesNotExist(int productId)
        {
            //arrange
            var receipt = new Receipt { Id = 1, CustomerId = 1 };

            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(x => x.ReceiptRepository.GetByIdWithDetailsAsync(It.IsAny<int>())).ReturnsAsync(receipt);
            mockUnitOfWork.Setup(x => x.ProductRepository.GetByIdAsync(It.IsAny<int>()));

            var receiptService = new ReceiptService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());

            //act
            Func<Task> act = async () => await receiptService.AddProductAsync(productId, 1, 1);

            await act.Should().ThrowAsync<MarketException>();
        }

        [TestCase(1)]
        [TestCase(2)]
        public async Task ReceiptService_AddProduct_ThrowsMarketExceptionIfReceiptDoesNotExist(int receiptId)
        {
            //arrange

            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(x => x.ReceiptRepository.GetByIdWithDetailsAsync(It.IsAny<int>()));
            mockUnitOfWork.Setup(x => x.ProductRepository.GetByIdAsync(It.IsAny<int>()));

            var receiptService = new ReceiptService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());

            //act
            Func<Task> act = async () => await receiptService.AddProductAsync(1, receiptId, 1);

            await act.Should().ThrowAsync<MarketException>();
        }


        [Test]
        public async Task ReceiptService_CheckOutAsync_UpdatesCheckOutPropertyValueAndSavesChanges()
        {
            //arrange
            var receipt = new Receipt { Id = 1, IsCheckedOut = false };
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(x => x.ReceiptRepository
                .GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(receipt);

            var receiptService = new ReceiptService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());

            //act
            await receiptService.CheckOutAsync(1);

            //assert
            receipt.IsCheckedOut.Should().BeTrue();
            mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Once);
        }


        [Test]
        public async Task ReceiptService_RemoveProductAsync_UpdatesDetailQuantityValue()
        {
            //arrange
            var receipt = new Receipt
            {
                Id = 1,
                ReceiptDetails = new List<ReceiptDetail>
                {
                    new ReceiptDetail { Id = 1, ProductId = 1, UnitPrice = 10, DiscountUnitPrice = 9, Quantity = 2, ReceiptId = 1 },
                    new ReceiptDetail { Id = 2, ProductId = 2, UnitPrice = 20, DiscountUnitPrice = 19, Quantity = 3, ReceiptId = 1 },
                    new ReceiptDetail { Id = 3, ProductId = 3, UnitPrice = 25, DiscountUnitPrice = 24, Quantity = 1, ReceiptId = 1 }
                }
            };

            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(x => x.ReceiptRepository.GetByIdWithDetailsAsync(It.IsAny<int>())).ReturnsAsync(receipt);
            var receiptService = new ReceiptService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());

            //act
            await receiptService.RemoveProductAsync(1, 1, 1);

            //assert
            var actualQuantity = receipt.ReceiptDetails.First().Quantity;
            actualQuantity.Should().Be(1);
            mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Once);
        }

        [Test]
        public async Task ReceiptService_RemoveProductAsync_DeletesDetailIfQuantityEqualsZero()
        {

            //arrange
            var receipt = new Receipt
            {
                Id = 1,
                ReceiptDetails = new List<ReceiptDetail>
                {
                    new ReceiptDetail { Id = 1, ProductId = 1, UnitPrice = 10, DiscountUnitPrice = 9, Quantity = 2, ReceiptId = 1 },
                    new ReceiptDetail { Id = 2, ProductId = 2, UnitPrice = 20, DiscountUnitPrice = 19, Quantity = 3, ReceiptId = 1 },
                    new ReceiptDetail { Id = 3, ProductId = 3, UnitPrice = 25, DiscountUnitPrice = 24, Quantity = 1, ReceiptId = 1 }
                }
            };

            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(x => x.ReceiptRepository.GetByIdWithDetailsAsync(It.IsAny<int>())).ReturnsAsync(receipt);
            mockUnitOfWork.Setup(x => x.ReceiptDetailRepository.Delete(It.IsAny<ReceiptDetail>()));
            var receiptService = new ReceiptService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());

            //act
            await receiptService.RemoveProductAsync(1, 1, 2);

            //assert
            mockUnitOfWork.Verify(x => x.ReceiptDetailRepository.Delete(It.Is<ReceiptDetail>(rd => rd.ReceiptId == receipt.Id && rd.ProductId == 1)), Times.Once());
            mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Once);
        }

        #region  TestData

        private static IEnumerable<Receipt> GetTestReceiptsEntities =>
          new List<Receipt>()
          {
                new Receipt
                {
                    Id = 1, CustomerId = 1, IsCheckedOut = false, OperationDate = new DateTime(2021, 1, 2),
                    ReceiptDetails = new List<ReceiptDetail>()
                    {
                        new ReceiptDetail { Id = 1, ProductId = 1, UnitPrice = 10, DiscountUnitPrice = 9, Quantity = 2, ReceiptId = 1 },
                        new ReceiptDetail { Id = 2, ProductId = 2, UnitPrice = 20, DiscountUnitPrice = 19, Quantity = 3, ReceiptId = 1},
                        new ReceiptDetail { Id = 3, ProductId = 3, UnitPrice = 25, DiscountUnitPrice = 24, Quantity = 1, ReceiptId = 1}
                    }
                },
                new Receipt
                {
                    Id = 2, CustomerId = 2, IsCheckedOut = false, OperationDate = new DateTime(2021, 1, 15),
                    ReceiptDetails = new List<ReceiptDetail>()
                    {
                        new ReceiptDetail { Id = 4, ProductId = 1, UnitPrice = 10, DiscountUnitPrice = 9, Quantity = 10, ReceiptId = 2 },
                        new ReceiptDetail { Id = 5, ProductId = 3, UnitPrice = 25, DiscountUnitPrice = 24, Quantity = 1, ReceiptId = 2}
                    }
                },
                new Receipt
                {
                    Id = 3, CustomerId = 3, IsCheckedOut = false, OperationDate = new DateTime(2021, 2, 15),
                    ReceiptDetails = new List<ReceiptDetail>()
                    {
                        new ReceiptDetail { Id = 6, ProductId = 1, UnitPrice = 10, DiscountUnitPrice = 9, Quantity = 10, ReceiptId = 3 },
                        new ReceiptDetail { Id = 7, ProductId = 2, UnitPrice = 25, DiscountUnitPrice = 24, Quantity = 1, ReceiptId = 3}
                    }
                },
                  new Receipt
                {
                    Id = 4, CustomerId = 4, IsCheckedOut = false, OperationDate = new DateTime(2021, 2, 28),
                    ReceiptDetails = new List<ReceiptDetail>()
                    {
                        new ReceiptDetail { Id = 8, ProductId = 5, UnitPrice = 10,  DiscountUnitPrice = 9, Quantity = 10, ReceiptId = 4 },
                        new ReceiptDetail { Id = 9, ProductId = 6, UnitPrice = 25,  DiscountUnitPrice = 24, Quantity = 1, ReceiptId = 4 }
                    }
                }
          };

        private static IEnumerable<ReceiptModel> GetTestReceiptsModels =>
         new List<ReceiptModel>()
         {
                new ReceiptModel
                {
                    Id = 1, CustomerId = 1, IsCheckedOut = false,  OperationDate = new DateTime(2021, 1, 2),
                    ReceiptDetailsIds = new List<int>()
                    {
                       1,
                       2,
                       3,
                    }
                },
                new ReceiptModel
                {
                    Id = 2, CustomerId = 2, IsCheckedOut = false,  OperationDate = new DateTime(2021, 1, 15),
                    ReceiptDetailsIds = new List<int>()
                    {
                        4,
                        5
                    }
                },
                new ReceiptModel
                {
                    Id = 3, CustomerId = 3, IsCheckedOut = false, OperationDate = new DateTime(2021, 2, 15),
                    ReceiptDetailsIds = new List<int>()
                    {
                        6,
                        7
                    }
                },
                  new ReceiptModel
                {
                    Id = 4, CustomerId = 4, IsCheckedOut = false, OperationDate = new DateTime(2021, 2, 28),
                    ReceiptDetailsIds = new List<int>()
                    {
                        8,
                        9
                    }
                }
         };


        private static IEnumerable<Product> ProductEntities =>
            new List<Product>
            {
                new Product {Id = 1, ProductName = "Chai", ProductCategoryId = 1, Category = new ProductCategory { Id = 1, CategoryName = "Beverages" }, Price = 18.00m },
                new Product {Id = 2, ProductName = "Chang", ProductCategoryId = 1, Category = new ProductCategory { Id = 1, CategoryName = "Beverages" }, Price = 19.00m },
                new Product {Id = 3, ProductName = "Aniseed Syrup", ProductCategoryId = 2, Category = new ProductCategory { Id = 2, CategoryName = "Condiments" }, Price = 10.00m },
                new Product {Id = 4, ProductName = "Chef Anton's Cajun Seasoning", ProductCategoryId = 2, Category =  new ProductCategory { Id = 2, CategoryName = "Condiments" }, Price = 22.00m },
                new Product {Id = 5, ProductName = "Grandma's Boysenberry Spread", ProductCategoryId = 3, Category = new ProductCategory { Id = 3, CategoryName = "Confections" } , Price = 25.00m },
            };

        #endregion
    }
}
