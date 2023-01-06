using NUnit.Framework;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Business.Models;
using Data.Entities;
using Data.Interfaces;
using Moq;
using System.Linq;
using Business.Services;
using Business.Validation;
using Library.Tests;

namespace TradeMarket.Tests.BusinessTests
{
    public class ProductServiceTests
    {
        [Test]
        public async Task ProductService_GetAll_ReturnsAllProducts()
        {
            //arrange
            var expected = ProductModels.ToList();
            var mockUnitOfWork = new Mock<IUnitOfWork>();

            mockUnitOfWork
                .Setup(x => x.ProductRepository.GetAllWithDetailsAsync())
                .ReturnsAsync(ProductEntities.AsEnumerable());

            var productService = new ProductService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());

            //act
            var actual = await productService.GetAllAsync();

            //assert
            actual.Should().BeEquivalentTo(expected, options =>
                options.Excluding(x => x.ReceiptDetailIds));
        }

        [Test]
        public async Task ProductService_GetAllProductCategoriesAsync_ReturnsAllCategories()
        {
            //arrange
            var expected = ProductCategoryModels;
            var mockUnitOfWork = new Mock<IUnitOfWork>();

            mockUnitOfWork
                .Setup(x => x.ProductCategoryRepository.GetAllAsync())
                .ReturnsAsync(ProductCategoryEntities.AsEnumerable());

            var productService = new ProductService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());

            //act
            var actual = await productService.GetAllProductCategoriesAsync();

            //assert
            actual.Should().BeEquivalentTo(expected, options =>
                options.Excluding(x => x.ProductIds));
        }

        [TestCase(1)]
        [TestCase(2)]
        public async Task ProductService_GetById_ReturnsProductModel(int id)
        {
            //arrange
            var expected = ProductModels.FirstOrDefault(x => x.Id == id);

            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork
                .Setup(x => x.ProductRepository.GetByIdWithDetailsAsync(It.IsAny<int>()))
                .ReturnsAsync(ProductEntities.FirstOrDefault(x => x.Id == id));

            var productService = new ProductService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());

            //act
            var actual = await productService.GetByIdAsync(1);

            //assert
            actual.Should().BeEquivalentTo(expected, options =>
              options.Excluding(x => x.ReceiptDetailIds));
        }


        [Test]
        public async Task ProductService_AddAsync_AddsProduct()
        {
            //arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(m => m.ProductRepository.AddAsync(It.IsAny<Product>()));

            var productService = new ProductService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());
            var product = new ProductModel { Id = 1, ProductName = "Chai", ProductCategoryId = 1, Price = 18.00m };

            //act
            await productService.AddAsync(product);

            //assert
            mockUnitOfWork.Verify(x => x.ProductRepository.AddAsync(It.Is<Product>(c => c.Id == product.Id && c.ProductCategoryId == product.ProductCategoryId && c.Price == product.Price && c.ProductName == product.ProductName)), Times.Once);
            mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Once);
        }

        [Test]
        public async Task ProductService_AddCategoryAsync_AddsCategory()
        {
            //arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(m => m.ProductCategoryRepository.AddAsync(It.IsAny<ProductCategory>()));

            var productService = new ProductService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());
            var category = new ProductCategoryModel { Id = 1, CategoryName = "Bevearages" };

            //act
            await productService.AddCategoryAsync(category);

            //add equality comparer
            //assert
            mockUnitOfWork.Verify(x => x.ProductCategoryRepository.AddAsync(It.Is<ProductCategory>(c => c.Id == category.Id && c.CategoryName == category.CategoryName)), Times.Once);
            mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Once);
        }

        [Test]
        public async Task ProductService_AddAsync_ThrowsMarketExceptionWithEmptyProductName()
        {
            //arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(m => m.ProductRepository.AddAsync(It.IsAny<Product>()));

            var productService = new ProductService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());
            var product = new ProductModel { Id = 1, ProductName = string.Empty, ProductCategoryId = 1, CategoryName = "Beverages", Price = 18.00m };

            //act
            Func<Task> act = async () => await productService.AddAsync(product);

            //assert
            await act.Should().ThrowAsync<MarketException>();
        }

        [TestCase(-5000.50)]
        [TestCase(-500000)]
        [TestCase(-0.0001)]
        public async Task ProductService_AddAsync_ThrowsMarketExceptionIfPriceIsNegative(decimal price)
        {
            //arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(m => m.ProductRepository.AddAsync(It.IsAny<Product>()));

            var productService = new ProductService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());
            var product = new ProductModel { Id = 1, ProductName = "Cola", ProductCategoryId = 1, CategoryName = "Beverages", Price = price };

            //act
            Func<Task> act = async () => await productService.AddAsync(product);

            //assert
            await act.Should().ThrowAsync<MarketException>();
        }


        [Test]
        public async Task ProductService_AddCategoryAsync_ThrowsMarketExceptionWithEmptyCategoryName()
        {
            //arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(m => m.ProductCategoryRepository.AddAsync(It.IsAny<ProductCategory>()));

            var productService = new ProductService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());
            var category = new ProductCategoryModel { Id = 1, CategoryName = "" };

            //act
            Func<Task> act = async () => await productService.AddCategoryAsync(category);

            //assert
            await act.Should().ThrowAsync<MarketException>();
        }

        [TestCase(1)]
        [TestCase(2)]
        public async Task ProductService_DeleteAsync_DeletesProduct(int id)
        {
            //arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(m => m.ProductRepository.DeleteByIdAsync(It.IsAny<int>()));
            var productService = new ProductService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());

            //act
            await productService.DeleteAsync(id);

            //assert
            mockUnitOfWork.Verify(x => x.ProductRepository.DeleteByIdAsync(id), Times.Once);
            mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Once);
        }

        [TestCase(1)]
        [TestCase(2)]
        public async Task ProductService_RemoveCategoryAsync_DeletesCategory(int id)
        {
            //arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(m => m.ProductCategoryRepository.DeleteByIdAsync(It.IsAny<int>()));
            var productService = new ProductService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());

            //act
            await productService.RemoveCategoryAsync(id);

            //assert
            mockUnitOfWork.Verify(x => x.ProductCategoryRepository.DeleteByIdAsync(id), Times.Once);
            mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Once);
        }

        [Test]
        public async Task ProductService_UpdateAsync_UpdatesProduct()
        {
            //arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(m => m.ProductRepository.Update(It.IsAny<Product>()));

            var productService = new ProductService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());
            var product = new ProductModel { Id = 7, ProductName = "Queso Cabrales", ProductCategoryId = 4, CategoryName = "Dairy Products", Price = 21.00m };

            //act
            await productService.UpdateAsync(product);

            //assert
            mockUnitOfWork.Verify(x => x.ProductRepository.Update(It.Is<Product>(c => c.Id == product.Id && c.ProductCategoryId == product.ProductCategoryId && c.Price == product.Price && c.ProductName == product.ProductName)), Times.Once);
            mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Once);
        }

        [Test]
        public async Task ProductService_UpdateAsync_ThrowsMarketExceptionsWithEmptyName()
        {
            //arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(m => m.ProductRepository.Update(It.IsAny<Product>()));

            var productService = new ProductService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());
            var product = new ProductModel { Id = 7, ProductName = "", ProductCategoryId = 4, CategoryName = "Dairy Products", Price = 21.00m };

            //act
            Func<Task> act = async () => await productService.UpdateAsync(product);

            //assert
            await act.Should().ThrowAsync<MarketException>();
        }

        [Test]
        public async Task ProductService_UpdateCategoryAsync_UpdatesCategory()
        {
            //arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(m => m.ProductCategoryRepository.Update(It.IsAny<ProductCategory>()));

            var productService = new ProductService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());
            var category = new ProductCategoryModel { Id = 77, CategoryName = "Name" };

            //act
            await productService.UpdateCategoryAsync(category);

            //assert
            mockUnitOfWork.Verify(x => x.ProductCategoryRepository.Update(It.Is<ProductCategory>(c => c.Id == category.Id && category.CategoryName == c.CategoryName)), Times.Once);
            mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Once);
        }

        [Test]
        public async Task ProductService_UpdateCategory_ThrowsMarketExceptionsWithEmptyName()
        {
            //arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(m => m.ProductCategoryRepository.Update(It.IsAny<ProductCategory>()));

            var productService = new ProductService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());
            var category = new ProductCategoryModel { Id = 77, CategoryName = "" };

            //act
            Func<Task> act = async () => await productService.UpdateCategoryAsync(category);

            //assert
            await act.Should().ThrowAsync<MarketException>();
        }

        [TestCase(1, new[] { 1, 2 })]
        [TestCase(2, new[] { 3, 4 })]
        [TestCase(3, new[] { 5 })]
        public async Task ProductService_GetByFilterAsync_ReturnsProductsByCategory(int categoryId, IEnumerable<int> expectedProductIds)
        {
            //arrange
            var expected = ProductModels.Where(x => expectedProductIds.Contains(x.Id));
            var filter = new FilterSearchModel { CategoryId = categoryId };

            var mockUnitOfWork = new Mock<IUnitOfWork>();

            mockUnitOfWork
                .Setup(x => x.ProductRepository.GetAllWithDetailsAsync())
                .ReturnsAsync(ProductEntities.AsEnumerable());

            var productService = new ProductService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());

            //act
            var actual = await productService.GetByFilterAsync(filter);

            //assert
            actual.Should().BeEquivalentTo(expected, options =>
                options.Excluding(x => x.ReceiptDetailIds)
            );
        }

        [TestCase(20, new[] { 4, 5 })]
        [TestCase(24, new[] { 5 })]
        public async Task ProductService_GetByFilter_ReturnsProductByMinPrice(int minPrice, IEnumerable<int> expectedProductIds)
        {
            //arrange
            var expected = ProductModels.Where(x => expectedProductIds.Contains(x.Id));
            var filter = new FilterSearchModel { MinPrice = minPrice };

            var mockUnitOfWork = new Mock<IUnitOfWork>();

            mockUnitOfWork
                .Setup(x => x.ProductRepository.GetAllWithDetailsAsync())
                .ReturnsAsync(ProductEntities.AsEnumerable());

            var productService = new ProductService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());

            //act
            var actual = await productService.GetByFilterAsync(filter);

            //assert
            actual.Should().BeEquivalentTo(expected, options =>
                options.Excluding(x => x.ReceiptDetailIds)
            );
        }

        [TestCase(1, 16, null, new[] { 1, 2 })]
        [TestCase(2, 15, 25, new[] { 4 })]

        public async Task ProductService_GetByFilter_ReturnsProductByFilter(int? categoryId, int? minPrice, int? maxPrice, IEnumerable<int> expectedProductIds)
        {
            //arrange
            var expected = ProductModels.Where(x => expectedProductIds.Contains(x.Id)).ToList();
            var filter = new FilterSearchModel { CategoryId = categoryId, MinPrice = minPrice, MaxPrice = maxPrice };

            var mockUnitOfWork = new Mock<IUnitOfWork>();

            mockUnitOfWork
                .Setup(x => x.ProductRepository.GetAllWithDetailsAsync())
                .ReturnsAsync(ProductEntities.AsEnumerable());

            var productService = new ProductService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());

            //act
            var actual = await productService.GetByFilterAsync(filter);

            //assert
            actual.Should().BeEquivalentTo(expected, options => options.Excluding(x => x.ReceiptDetailIds)
            );
        }

        #region Test Data

        private static IEnumerable<ProductModel> ProductModels =>
            new List<ProductModel>
            {
                new ProductModel { Id = 1, ProductName = "Chai", ProductCategoryId = 1, CategoryName =  "Beverages", Price = 18.00m },
                new ProductModel { Id = 2, ProductName = "Chang", ProductCategoryId = 1, CategoryName =  "Beverages", Price = 19.00m },
                new ProductModel { Id = 3, ProductName = "Aniseed Syrup", ProductCategoryId = 2,  CategoryName = "Condiments", Price = 10.00m },
                new ProductModel { Id = 4, ProductName = "Chef Anton's Cajun Seasoning", ProductCategoryId = 2,  CategoryName = "Condiments", Price = 22.00m },
                new ProductModel { Id = 5, ProductName = "Grandma's Boysenberry Spread", ProductCategoryId = 3, CategoryName = "Confections", Price = 25.00m },
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

        private static IEnumerable<ProductCategory> ProductCategoryEntities =>
            new List<ProductCategory>
            {
                new ProductCategory
                {
                    Id = 1, CategoryName = "Beverages",
                },
                new ProductCategory
                {
                    Id = 2, CategoryName = "Condiments",
                },
                new ProductCategory
                {
                    Id = 3, CategoryName = "Confections",
                },
                new ProductCategory
                {
                    Id = 4, CategoryName = "Dairy Products",
                },
            };

        private static IEnumerable<ProductCategoryModel> ProductCategoryModels =>
            new List<ProductCategoryModel>
            {
                new ProductCategoryModel { Id = 1, CategoryName = "Beverages" },
                new ProductCategoryModel { Id = 2, CategoryName = "Condiments" },
                new ProductCategoryModel { Id = 3, CategoryName = "Confections" },
                new ProductCategoryModel { Id = 4, CategoryName = "Dairy Products"}
            };

        #endregion
    }
}
