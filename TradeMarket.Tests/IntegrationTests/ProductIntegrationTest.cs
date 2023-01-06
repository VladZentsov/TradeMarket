using Business.Models;
using Data.Data;
using FluentAssertions;
using Library.Tests.IntegrationTests;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TradeMarket.Tests.IntegrationTests
{
    public class ProductIntegrationTest
    {
        private CustomWebApplicationFactory _factory;
        private HttpClient _client;
        private const string RequestUri = "api/products/";

        [SetUp]
        public void Init()
        {
            _factory = new CustomWebApplicationFactory();
            _client = _factory.CreateClient();
        }

        [Test]
        public async Task ProductsController_GetByFilter_ReturnsAllWithNullFilter()
        {
            //arrange 
            var expected = ProductModels.ToList();

            //act
            var httpResponse = await _client.GetAsync(RequestUri);

            //assert
            httpResponse.EnsureSuccessStatusCode();
            var stringResponse = await httpResponse.Content.ReadAsStringAsync();
            var actual = JsonConvert.DeserializeObject<IEnumerable<ProductModel>>(stringResponse).ToList();

            actual.Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task ProductsController_GetById_ReturnsProductModel()
        {
            //arrange
            var expected = ProductModels.First();

            //act
            var httpResponse = await _client.GetAsync(RequestUri + 1);

            //assert
            httpResponse.EnsureSuccessStatusCode();
            var stringResponse = await httpResponse.Content.ReadAsStringAsync();
            var actual = JsonConvert.DeserializeObject<ProductModel>(stringResponse);

            actual.Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task ProductsController_GetByIdAsync_ReturnsNotFound()
        {
            var productId = 10099;

            // act
            var httpResponse = await _client.GetAsync(RequestUri + productId);

            // assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public async Task ProductsController_GetByFilter_ReturnsProductsThatApplyFilter()
        {
            //arrange
            var expected = new List<ProductModel> { ProductModels.First() };
            var expectedLength = expected.Count;

            //act
            var httpResponse = await _client.GetAsync($"{RequestUri}?categoryId=1&minPrice=15&maxPrice=50");

            //assert
            httpResponse.EnsureSuccessStatusCode();
            var stringResponse = await httpResponse.Content.ReadAsStringAsync();
            var actual = JsonConvert.DeserializeObject<IEnumerable<ProductModel>>(stringResponse).ToList();

            actual.Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task ProductsController_Add_AddsProductToDb()
        {
            //arrange
            var product = new ProductModel { Id = 3, ProductName = "Milk", Price = 25, ProductCategoryId = 1 };

            //act
            var content = new StringContent(JsonConvert.SerializeObject(product), Encoding.UTF8, "application/json");
            var httpResponse = await _client.PostAsync(RequestUri, content);

            //assert
            httpResponse.EnsureSuccessStatusCode();
            var stringResponse = await httpResponse.Content.ReadAsStringAsync();
            var productInResponse = JsonConvert.DeserializeObject<ProductModel>(stringResponse);
            await CheckProductsInfoIntoDb(product, productInResponse.Id, 3);

        }

        [Test]
        public async Task ProductsController_Update_UpdatesProductInDb()
        {
            //arrange
            var product = new ProductModel { Id = 1, ProductCategoryId = 1, ProductName = "NewName1", CategoryName = "NewCategory1", Price = 20 };

            //act
            var content = new StringContent(JsonConvert.SerializeObject(product), Encoding.UTF8, "application/json");
            var httpResponse = await _client.PutAsync(RequestUri + product.Id, content);

            //assert
            httpResponse.EnsureSuccessStatusCode();
            await CheckProductsInfoIntoDb(product, product.Id, 2);
        }

        [Test]
        public async Task ProductsController_Add_ThrowsExceptionIfModelIsInvalid()
        {
            //arrange
            //empty name
            var product = new ProductModel { ProductName = "", Price = 300, ProductCategoryId = 1 };

            //act + assert
            await CheckExceptionWhileAddNewModel(product);
        }

        [Test]
        public async Task ProductsController_Update_ThrowsExceptionIfModelIsInvalid()
        {
            //arrange
            //empty name
            var product = new ProductModel { Id = 1, ProductName = "", Price = 300, ProductCategoryId = 1 };

            //act + assert
            await CheckExceptionWhileUpdateModel(product);
        }

        [Test]
        public async Task ProductController_GetAllCategories_ReturnsAllCategoryModels()
        {
            //arrange 
            var expected = CategoryModels.ToList();
            var expectedLength = expected.Count;

            //act
            var httpResponse = await _client.GetAsync(RequestUri + "categories");

            //assert
            httpResponse.EnsureSuccessStatusCode();
            var stringResponse = await httpResponse.Content.ReadAsStringAsync();
            var actual = JsonConvert.DeserializeObject<IEnumerable<ProductCategoryModel>>(stringResponse).ToList();

            actual.Should().BeEquivalentTo(expected, options => options.Excluding(x => x.ProductIds));
        }

        [Test]
        public async Task ProductsController_AddCategory_AddsCategoryModelToDb()
        {
            //arrange
            var category = new ProductCategoryModel { Id = 3, CategoryName = "Condiments" };
            var expectedLength = CategoryModels.Count() + 1;

            //act
            var content = new StringContent(JsonConvert.SerializeObject(category), Encoding.UTF8, "application/json");
            var httpResponse = await _client.PostAsync(RequestUri + "categories", content);

            //assert
            httpResponse.EnsureSuccessStatusCode();
            var stringResponse = await httpResponse.Content.ReadAsStringAsync();
            var categoryInResponse = JsonConvert.DeserializeObject<ProductCategoryModel>(stringResponse);

            using (var test = _factory.Services.CreateScope())
            {
                var context = test.ServiceProvider.GetService<TradeMarketDbContext>();
                if (context != null)
                {
                    context.ProductCategories.Should().HaveCount(expectedLength);
                    var dbCategory = await context.ProductCategories.FindAsync(categoryInResponse.Id);
                    dbCategory.Should().NotBeNull();
                    dbCategory.CategoryName.Should().Be(category.CategoryName);
                }
            }
        }

        [Test]
        public async Task ProductsController_UpdateCategory_UpdatesCategoryModelInDb()
        {
            //arrange
            var category = CategoryModels.First();
            category.CategoryName = "NewCategory1";
            var expectedLength = CategoryModels.Count();

            //act
            var content = new StringContent(JsonConvert.SerializeObject(category), Encoding.UTF8, "application/json");
            var httpResponse = await _client.PutAsync(RequestUri + $"categories/{category.Id}", content);

            //assert
            httpResponse.EnsureSuccessStatusCode();
            var stringResponse = await httpResponse.Content.ReadAsStringAsync();

            using (var test = _factory.Services.CreateScope())
            {
                var context = test.ServiceProvider.GetService<TradeMarketDbContext>();
                if (context != null)
                {
                    context.ProductCategories.Should().HaveCount(expectedLength);

                    var dbCategory = await context.ProductCategories.FindAsync(category.Id);
                    dbCategory.Should().NotBeNull();
                    dbCategory.CategoryName.Should().Be(category.CategoryName);
                }
            }
        }

        [Test]
        public async Task ProductsController_Delete_DeletesProductFromDb()
        {
            // arrange
            var productId = 1;

            // act
            var httpResponse = await _client.DeleteAsync(RequestUri + productId);

            // assert
            httpResponse.EnsureSuccessStatusCode();
            using (var test = _factory.Services.CreateScope())
            {
                var context = test.ServiceProvider.GetService<TradeMarketDbContext>();
                context.Products.Should().HaveCount(1);
            }
        }

        [Test]
        public async Task ProductsController_DeleteCategory_DeletesCategoryFromDb()
        {
            // arrange
            var categoryId = 1;

            // act
            var httpResponse = await _client.DeleteAsync(RequestUri + $"categories/{categoryId}");

            // assert
            httpResponse.EnsureSuccessStatusCode();
            using (var test = _factory.Services.CreateScope())
            {
                var context = test.ServiceProvider.GetService<TradeMarketDbContext>();
                context.ProductCategories.Should().HaveCount(1);
            }
        }

        public IEnumerable<ProductModel> ProductModels =
            new List<ProductModel>()
            {
                new ProductModel { Id = 1, ProductCategoryId = 1, ProductName = "Name1", CategoryName = "Category1", Price = 20, ReceiptDetailIds = new List<int>(){1, 4}},
                new ProductModel { Id = 2, ProductCategoryId = 2, ProductName = "Name2", CategoryName = "Category2", Price = 50, ReceiptDetailIds = new List<int>(){2, 3, 5}}
            };

        public IEnumerable<ProductCategoryModel> CategoryModels =
            new List<ProductCategoryModel>()
            {
                new ProductCategoryModel {Id = 1, CategoryName = "Category1"},
                new ProductCategoryModel {Id = 2, CategoryName = "Category2"}
            };

        private async Task CheckProductsInfoIntoDb(ProductModel product, int productId, int expectedLength)
        {
            using (var test = _factory.Services.CreateScope())
            {
                var context = test.ServiceProvider.GetService<TradeMarketDbContext>();
                if (context != null)
                {
                    context.Products.Should().HaveCount(expectedLength);

                    var dbProduct = await context.Products.FindAsync(productId);
                    dbProduct.Should().NotBeNull().And.BeEquivalentTo(product, options =>
                        options.Excluding(x => x.Id).ExcludingMissingMembers());
                }
            }
        }

        private async Task CheckExceptionWhileAddNewModel(ProductModel productModel)
        {
            var content = new StringContent(JsonConvert.SerializeObject(productModel), Encoding.UTF8, "application/json");
            var httpResponse = await _client.PostAsync(RequestUri, content);

            httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        private async Task CheckExceptionWhileUpdateModel(ProductModel productModel)
        {
            var content = new StringContent(JsonConvert.SerializeObject(productModel), Encoding.UTF8, "application/json");
            var httpResponse = await _client.PutAsync(RequestUri + productModel.Id, content);

            httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TearDown]
        public void TearDown()
        {
            _factory.Dispose();
            _client.Dispose();
        }
    }
}
