using Business.Models;
using FluentAssertions;
using Library.Tests.IntegrationTests;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace TradeMarket.Tests.IntegrationTests
{
    public class StatisticIntegrationTest
    {
        private CustomWebApplicationFactory _factory;
        private HttpClient _client;
        private const string RequestUri = "api/statistics/";

        [SetUp]
        public void Init()
        {
            _factory = new CustomWebApplicationFactory();
            _client = _factory.CreateClient();
        }

        [Test]
        public async Task StatisticsController_GetMostPopularProducts()
        {
            //arrange
            var expected = new List<ProductModel> { GetProductsModels().First(i => i.Id == 2) };

            //act
            var httpResponse = await _client.GetAsync(RequestUri + "popularProducts?productCount=1");

            //assert
            httpResponse.EnsureSuccessStatusCode();
            var stringResponse = await httpResponse.Content.ReadAsStringAsync();
            var actual = JsonConvert.DeserializeObject<IEnumerable<ProductModel>>(stringResponse).ToList();

            actual.Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task StatisticsController_GetCustomersMostPopularProducts()
        {
            //arrange
            var expected = new List<ProductModel> { GetProductsModels().First(i => i.Id == 1) };

            //act
            var httpResponse = await _client.GetAsync(RequestUri + "customer/1/1");

            //assert
            httpResponse.EnsureSuccessStatusCode();
            var stringResponse = await httpResponse.Content.ReadAsStringAsync();
            var actual = JsonConvert.DeserializeObject<IEnumerable<ProductModel>>(stringResponse).ToList();

            actual.Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task StatisticsController_GetMostValuableCustomers()
        {
            //arrange
            var expected = new List<CustomerActivityModel> { GetCustomersActivityModels().First(i => i.CustomerId == 2) };

            //act
            var httpResponse = await _client.GetAsync(RequestUri + "activity/1?startDate=2020-7-21&endDate=2022-7-22");

            //assert
            httpResponse.EnsureSuccessStatusCode();
            var stringResponse = await httpResponse.Content.ReadAsStringAsync();
            var actual = JsonConvert.DeserializeObject<IEnumerable<CustomerActivityModel>>(stringResponse).ToList();

            actual.Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task StatisticsController_GetIncomeOfCategoryInPeriod()
        {
            //arrange
            var expected = 36M;

            //act
            var httpResponse = await _client.GetAsync(RequestUri + "income/1?startDate=2021-7-25&endDate=2021-10-20");

            //assert
            httpResponse.EnsureSuccessStatusCode();
            var stringResponse = await httpResponse.Content.ReadAsStringAsync();
            var actual = JsonConvert.DeserializeObject<decimal>(stringResponse);

            actual.Should().Be(expected);
        }

        [TearDown]
        public void TearDown()
        {
            _factory.Dispose();
            _client.Dispose();
        }

        private List<ProductModel> GetProductsModels() => new List<ProductModel>
            {
                new ProductModel { Id = 1, ProductCategoryId = 1, ProductName = "Name1", CategoryName = "Category1", Price = 20, ReceiptDetailIds = new List<int>(){ 1, 4 } },
                new ProductModel { Id = 2, ProductCategoryId = 2, ProductName = "Name2", CategoryName = "Category2", Price = 50, ReceiptDetailIds = new List<int>(){ 2, 3, 5 } }
            };

        private List<CustomerActivityModel> GetCustomersActivityModels() => new List<CustomerActivityModel>
            {
                new CustomerActivityModel { CustomerId = 1, CustomerName = "Name1 Surname1", ReceiptSum = 261 },
                new CustomerActivityModel { CustomerId = 2, CustomerName = "Name2 Surname2", ReceiptSum = 261 }
            };
    }
}
