using Business.Models;
using Data.Data;
using FluentAssertions;
using Library.Tests.IntegrationTests;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TradeMarket.Tests.IntegrationTests
{
    public class ReceiptIntegrationTest
    {
        private CustomWebApplicationFactory _factory;
        private HttpClient _client;
        private const string RequestUri = "api/receipts/";

        [SetUp]
        public void Init()
        {
            _factory = new CustomWebApplicationFactory();
            _client = _factory.CreateClient();
        }

        [Test]
        public async Task ReceiptsController_GetAll_ReturnsAllReceiptModels()
        {
            //arrange
            var expected = GetReceiptsModels();

            //act
            var httpResponse = await _client.GetAsync(RequestUri);

            //assert
            httpResponse.EnsureSuccessStatusCode();
            var stringResponse = await httpResponse.Content.ReadAsStringAsync();
            var actual = JsonConvert.DeserializeObject<IEnumerable<ReceiptModel>>(stringResponse).ToList();

            actual.Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task ReceiptsController_GetById_ReturnsReceiptModel()
        {
            //arrange
            var expected = GetReceiptsModels().First();

            //act
            var httpResponse = await _client.GetAsync(RequestUri + 1);

            //assert
            httpResponse.EnsureSuccessStatusCode();
            var stringResponse = await httpResponse.Content.ReadAsStringAsync();
            var actual = JsonConvert.DeserializeObject<ReceiptModel>(stringResponse);

            actual.Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task ReceiptsController_GetReceiptDetails_ReturnsReceiptDetailModels()
        {
            //arrange
            var expected = GetReceiptDetailsModels().Where(i => i.ReceiptId == 1);

            //act
            var httpResponse = await _client.GetAsync(RequestUri + "1/details");

            //assert
            httpResponse.EnsureSuccessStatusCode();
            var stringResponse = await httpResponse.Content.ReadAsStringAsync();
            var actual = JsonConvert.DeserializeObject<IEnumerable<ReceiptDetailModel>>(stringResponse).ToList();

            actual.Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task ReceiptsController_GetSum_ReturnsReceiptSum()
        {
            //arrange
            var expected = 88M;

            //act
            var httpResponse = await _client.GetAsync(RequestUri + "1/sum");

            //assert
            httpResponse.EnsureSuccessStatusCode();
            var stringResponse = await httpResponse.Content.ReadAsStringAsync();
            var actual = JsonConvert.DeserializeObject<decimal>(stringResponse);

            actual.Should().Be(expected);
        }

        [Test]
        public async Task ReceiptsController_GetReceiptsByPeriod_ReturnsReceiptModels()
        {
            //arrange
            var expected = GetReceiptsModels().Where(i => i.Id == 2 || i.Id == 3);

            //act
            var httpResponse = await _client.GetAsync(RequestUri + "period?startDate=2021-7-25&endDate=2021-10-20");

            //assert
            httpResponse.EnsureSuccessStatusCode();
            var stringResponse = await httpResponse.Content.ReadAsStringAsync();
            var actual = JsonConvert.DeserializeObject<IEnumerable<ReceiptModel>>(stringResponse);

            actual.Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task ReceiptsController_Add_AddsReceiptModelToDb()
        {
            //arrange
            var receipt = new ReceiptModel { Id = 4, CustomerId = 2, OperationDate = new DateTime(2021, 11, 27), IsCheckedOut = true, ReceiptDetailsIds = new List<int>() };

            //act
            var content = new StringContent(JsonConvert.SerializeObject(receipt), Encoding.UTF8, "application/json");
            var httpResponse = await _client.PostAsync(RequestUri, content);

            //assert
            httpResponse.EnsureSuccessStatusCode();
            var stringResponse = await httpResponse.Content.ReadAsStringAsync();
            var receiptInResponse = JsonConvert.DeserializeObject<ReceiptModel>(stringResponse);

            await CheckReceiptsInfoInDb(receipt, receiptInResponse.Id, 4);
        }

        [Test]
        public async Task ReceiptsController_Update_UpdatesReceiptInDb()
        {
            //arrange
            var receipt = new ReceiptModel { Id = 1, CustomerId = 2, OperationDate = new DateTime(2021, 10, 5), IsCheckedOut = false, ReceiptDetailsIds = new List<int>() { 1, 2 } };

            //act
            var content = new StringContent(JsonConvert.SerializeObject(receipt), Encoding.UTF8, "application/json");
            var httpResponse = await _client.PutAsync(RequestUri + receipt.Id, content);

            //assert
            httpResponse.EnsureSuccessStatusCode();

            await CheckReceiptsInfoInDb(receipt, receipt.Id, 3);
        }

        [Test]
        public async Task ReceiptsController_AddProduct_UpdatesReceiptDetailModelInDb()
        {
            //arrange
            var receiptDetail = new ReceiptDetailModel { Id = 2, ReceiptId = 1, ProductId = 2, UnitPrice = 50, DiscountUnitPrice = 40, Quantity = 4 };

            //act
            var content = new StringContent(JsonConvert.SerializeObject(receiptDetail), Encoding.UTF8, "application/json");
            var httpResponse = await _client.PutAsync(RequestUri + "1/products/add/2/3", content);

            //assert
            httpResponse.EnsureSuccessStatusCode();

            await CheckReceiptDetailsInfoInDb(receiptDetail, receiptDetail.ReceiptId, receiptDetail.ProductId, 5);
        }

        [Test]
        public async Task ReceiptsController_AddProduct_AddsReceiptDetailModelInDb()
        {
            //arrange
            var receiptDetail = new ReceiptDetailModel { Id = 6, ReceiptId = 2, ProductId = 1, UnitPrice = 20, DiscountUnitPrice = 16, Quantity = 4 };

            //act
            var content = new StringContent(JsonConvert.SerializeObject(receiptDetail), Encoding.UTF8, "application/json");
            var httpResponse = await _client.PutAsync(RequestUri + "2/products/add/1/4", content);

            //assert
            httpResponse.EnsureSuccessStatusCode();

            await CheckReceiptDetailsInfoInDb(receiptDetail, receiptDetail.ReceiptId, receiptDetail.ProductId, 6);
        }

        [Test]
        public async Task ReceiptsController_RemoveProduct_UpdateReceiptDetailModelInDb()
        {
            //arrange
            var receiptDetail = new ReceiptDetailModel { Id = 5, ReceiptId = 3, ProductId = 2, UnitPrice = 50, DiscountUnitPrice = 45, Quantity = 3 };

            //act
            var content = new StringContent(JsonConvert.SerializeObject(receiptDetail), Encoding.UTF8, "application/json");
            var httpResponse = await _client.PutAsync(RequestUri + "3/products/remove/2/2", content);

            //assert
            httpResponse.EnsureSuccessStatusCode();

            await CheckReceiptDetailsInfoInDb(receiptDetail, receiptDetail.ReceiptId, receiptDetail.ProductId, 5);
        }

        [Test]
        public async Task ReceiptsController_RemoveProduct_DeleteReceiptDetailModelInDb()
        {
            //arrange
            var receiptDetailId = 4;

            //act
            var content = new StringContent(JsonConvert.SerializeObject(receiptDetailId), Encoding.UTF8, "application/json");
            var httpResponse = await _client.PutAsync(RequestUri + $"3/products/remove/1/2", content);

            //assert
            httpResponse.EnsureSuccessStatusCode();

            using var test = _factory.Services.CreateScope();
            var context = test.ServiceProvider.GetService<TradeMarketDbContext>();
            context.ReceiptsDetails.Should().HaveCount(4);
        }

        [Test]
        public async Task ReceiptsController_CheckOut_UpdateReceiptModelInDb()
        {
            //arrange
            var receiptId = 3;

            //act
            var content = new StringContent(JsonConvert.SerializeObject(receiptId), Encoding.UTF8, "application/json");
            var httpResponse = await _client.PutAsync(RequestUri + $"3/checkout", content);

            //assert
            httpResponse.EnsureSuccessStatusCode();

            using var test = _factory.Services.CreateScope();
            var context = test.ServiceProvider.GetService<TradeMarketDbContext>();
            var dbReceipt = context.Receipts.FirstOrDefault(i => i.Id == receiptId);
            dbReceipt.IsCheckedOut.Should().BeTrue();
        }

        [Test]
        public async Task ReceiptsController_Delete_DeletesReceiptFromDb()
        {
            // arrange
            var receiptId = 1;

            // act
            var httpResponse = await _client.DeleteAsync(RequestUri + receiptId);

            // assert
            httpResponse.EnsureSuccessStatusCode();

            using var test = _factory.Services.CreateScope();
            var context = test.ServiceProvider.GetService<TradeMarketDbContext>();
            context.Receipts.Should().HaveCount(2);
        }

        [TearDown]
        public void TearDown()
        {
            _factory.Dispose();
            _client.Dispose();
        }

        private List<ReceiptModel> GetReceiptsModels() => new List<ReceiptModel>
            {
                new ReceiptModel { Id = 1, CustomerId = 1, OperationDate = new DateTime(2021, 7, 5), IsCheckedOut = true, ReceiptDetailsIds = new List<int>(){ 1, 2 } },
                new ReceiptModel { Id = 2, CustomerId = 1, OperationDate = new DateTime(2021, 8, 10), IsCheckedOut = true, ReceiptDetailsIds = new List<int>(){ 3 } },
                new ReceiptModel { Id = 3, CustomerId = 2, OperationDate = new DateTime(2021, 10, 15), IsCheckedOut = false, ReceiptDetailsIds = new List<int>(){ 4, 5 } }
            };

        private List<ReceiptDetailModel> GetReceiptDetailsModels() => new List<ReceiptDetailModel>
            {
                new ReceiptDetailModel { Id = 1, ReceiptId = 1, ProductId = 1, UnitPrice = 20, DiscountUnitPrice = 16, Quantity = 3 },
                new ReceiptDetailModel { Id = 2, ReceiptId = 1, ProductId = 2, UnitPrice = 50, DiscountUnitPrice = 40, Quantity = 1 },
                new ReceiptDetailModel { Id = 3, ReceiptId = 2, ProductId = 2, UnitPrice = 50, DiscountUnitPrice = 40, Quantity = 2 },
                new ReceiptDetailModel { Id = 4, ReceiptId = 3, ProductId = 1, UnitPrice = 20, DiscountUnitPrice = 18, Quantity = 2 },
                new ReceiptDetailModel { Id = 5, ReceiptId = 3, ProductId = 2, UnitPrice = 50, DiscountUnitPrice = 45, Quantity = 5 }
            };

        private async Task CheckReceiptsInfoInDb(ReceiptModel receipt, int receiptId, int expectedLength)
        {
            using var test = _factory.Services.CreateScope();
            var context = test.ServiceProvider.GetService<TradeMarketDbContext>();
            if (context != null)
            {
                context.Receipts.Should().HaveCount(expectedLength);

                var dbReceipt = await context.Receipts.FindAsync(receiptId);
                dbReceipt.Should().NotBeNull().And.BeEquivalentTo(receipt, options =>
                    options.Excluding(x => x.Id).ExcludingMissingMembers());
            }
        }

        private async Task CheckReceiptDetailsInfoInDb(ReceiptDetailModel receiptDetail, int receiptId, int productId, int expectedLength)
        {
            using var test = _factory.Services.CreateScope();
            var context = test.ServiceProvider.GetService<TradeMarketDbContext>();
            if (context != null)
            {
                context.ReceiptsDetails.Should().HaveCount(expectedLength);

                var dbReceipt = await context.ReceiptsDetails.FindAsync(receiptId, productId);
                dbReceipt.Should().NotBeNull().And.BeEquivalentTo(receiptDetail, options =>
                    options.Excluding(x => x.Id).ExcludingMissingMembers());
            }
        }
    }
}
