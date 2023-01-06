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
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TradeMarket.Tests.IntegrationTests
{
    public class CustomerIntegrationTest
    {
        private CustomWebApplicationFactory _factory;
        private HttpClient _client;
        private const string RequestUri = "api/customers/";

        [SetUp]
        public void Init()
        {
            _factory = new CustomWebApplicationFactory();
            _client = _factory.CreateClient();
        }


        [Test]
        public async Task CustomerController_GetAll_ReturnsAllFromDb()
        {
            //arrange
            var expected = ExpectedCustomerModels.ToList();

            // act
            var httpResponse = await _client.GetAsync(RequestUri);

            // assert
            httpResponse.EnsureSuccessStatusCode();
            var stringResponse = await httpResponse.Content.ReadAsStringAsync();
            var actual = JsonConvert.DeserializeObject<IEnumerable<CustomerModel>>(stringResponse).ToList();

            actual.Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task CustomersController_GetByIdAsync_ReturnsCustomerById()
        {
            //arrange
            var expected = ExpectedCustomerModels.First();
            var customerId = 1;

            // act
            var httpResponse = await _client.GetAsync(RequestUri + customerId);

            // assert
            httpResponse.EnsureSuccessStatusCode();
            var stringResponse = await httpResponse.Content.ReadAsStringAsync();
            var actual = JsonConvert.DeserializeObject<CustomerModel>(stringResponse);

            actual.Should().BeEquivalentTo(expected);
        }


        [Test]
        public async Task CustomersController_GetByIdAsync_ReturnsNotFound()
        {
            var customerId = 1099;

            // act
            var httpResponse = await _client.GetAsync(RequestUri + customerId);

            // assert
            httpResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Test]
        public async Task CustomersController_GetByProductId_ReturnsCustomersWhoBoughtProduct()
        {
            //arrange
            var expected = ExpectedCustomerModels.ToList();
            var productId = 1;

            //act
            var httpResponse = await _client.GetAsync(RequestUri + $"products/{productId}");

            //assert
            httpResponse.EnsureSuccessStatusCode();
            var stringResponse = await httpResponse.Content.ReadAsStringAsync();
            var actual = JsonConvert.DeserializeObject<IEnumerable<CustomerModel>>(stringResponse).ToList();

            actual.Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task CustomersController_Add_AddsCustomerToDb()
        {
            //arrange
            var customer = new CustomerModel
            {
                Id = 3,
                Name = "Desmond",
                Surname = "Morris",
                BirthDate = new DateTime(1955, 4, 12),
                DiscountValue = 5
            };

            var content = new StringContent(JsonConvert.SerializeObject(customer), Encoding.UTF8, "application/json");

            //act
            var httpResponse = await _client.PostAsync(RequestUri, content);

            //assert
            httpResponse.EnsureSuccessStatusCode();
            var stringResponse = await httpResponse.Content.ReadAsStringAsync();
            var customerInResponse = JsonConvert.DeserializeObject<CustomerModel>(stringResponse);
            await CheckCustomersInfoIntoDb(customer, customerInResponse.Id, 3);
        }

        [Test]
        public async Task CustomersController_Add_ThrowsExceptionIfModelIsInvalid()
        {
            // name is empty
            var customer = new CustomerModel { Name = string.Empty, Surname = "Haskell", BirthDate = new DateTime(1962, 4, 12), DiscountValue = 50 };
            await CheckExceptionWhileAddNewModel(customer);

            // surname is empty
            customer.Name = "Louis";
            customer.Surname = string.Empty;
            await CheckExceptionWhileAddNewModel(customer);

            // birthdate is not valid 
            customer.Surname = "Hamilton";
            customer.BirthDate = new DateTime(2050, 12, 1);
            await CheckExceptionWhileAddNewModel(customer);

            //discount value is not valid
            customer.BirthDate = new DateTime(2000, 12, 31);
            customer.DiscountValue = -10;
            await CheckExceptionWhileAddNewModel(customer);
        }

        [Test]
        public async Task CustomersController_Update_UpdateCustomerInDb()
        {
            var customer = new CustomerModel
            {
                Id = 1,
                Name = "NewName1",
                Surname = "NewSurname1",
                BirthDate = new DateTime(1980, 5, 25),
                DiscountValue = 50,
            };
            var content = new StringContent(JsonConvert.SerializeObject(customer), Encoding.UTF8, "application/json");

            //act
            var httpResponse = await _client.PutAsync(RequestUri + customer.Id, content);

            //assert
            httpResponse.EnsureSuccessStatusCode();
            await CheckCustomersInfoIntoDb(customer, customer.Id, 2);
        }


        [Test]
        public async Task CustomersController_Update_ThrowsExceptionIfModelIsInvalid()
        {
            //name is empty
            var customer = new CustomerModel { Id = 1, Name = "", Surname = "NewSurname1", BirthDate = new DateTime(1980, 5, 25), DiscountValue = 20 };
            await CheckExceptionWhileUpdateModel(customer);

            //surname is empty
            customer.Name = "Dame";
            customer.Surname = "";
            await CheckExceptionWhileUpdateModel(customer);

            //birthdate is invalid
            customer.Surname = "Durname";
            customer.BirthDate = new DateTime(1469, 2, 2);
            await CheckExceptionWhileAddNewModel(customer);

            //discount value is not valid
            customer.BirthDate = new DateTime(2000, 12, 31);
            customer.DiscountValue = -1;
            await CheckExceptionWhileUpdateModel(customer);
        }

        [Test]
        public async Task CustomersController_Delete_DeletesCustomerFromDb()
        {
            // arrange
            var customerId = 1;
            var expectedLength = ExpectedCustomerModels.Count() - 1;

            // act
            var httpResponse = await _client.DeleteAsync(RequestUri + customerId);

            // assert
            httpResponse.EnsureSuccessStatusCode();
            using (var test = _factory.Services.CreateScope())
            {
                var context = test.ServiceProvider.GetService<TradeMarketDbContext>();
                context.Customers.Should().HaveCount(expectedLength);
            }
        }

        private static readonly IEnumerable<CustomerModel> ExpectedCustomerModels =
            new List<CustomerModel>()
            {
                new CustomerModel
                {
                    Id = 1, Name = "Name1", Surname = "Surname1", BirthDate = new DateTime(1980, 5, 25),
                    DiscountValue = 20, ReceiptsIds = new List<int>() {1, 2}
                },
                new CustomerModel
                {
                    Id = 2, Name = "Name2", Surname = "Surname2", BirthDate = new DateTime(1984, 10, 19),
                    DiscountValue = 10, ReceiptsIds = new List<int>() { 3 }
                },
            };

        #region  helpers

        private async Task CheckExceptionWhileAddNewModel(CustomerModel customer)
        {
            var context = new StringContent(JsonConvert.SerializeObject(customer), Encoding.UTF8, "application/json");
            var httpResponse = await _client.PostAsync(RequestUri, context);

            httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        private async Task CheckExceptionWhileUpdateModel(CustomerModel customer)
        {
            var context = new StringContent(JsonConvert.SerializeObject(customer), Encoding.UTF8, "application/json");
            var httpResponse = await _client.PutAsync(RequestUri + customer.Id, context);

            httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        private async Task CheckCustomersInfoIntoDb(CustomerModel customer, int customerId, int expectedLength)
        {
            using (var test = _factory.Services.CreateScope())
            {
                var context = test.ServiceProvider.GetService<TradeMarketDbContext>();
                context.Customers.Should().HaveCount(expectedLength);

                var dbCustomer = await context.Customers.FindAsync(customerId);
                dbCustomer.Should().NotBeNull();
                dbCustomer.DiscountValue.Should().Be(dbCustomer.DiscountValue);

                var dbPerson = await context.Persons.FindAsync(customerId);
                dbPerson.Should().NotBeNull().And.BeEquivalentTo(customer, options => options
                    .Including(x => x.Name)
                    .Including(x => x.Surname)
                    .Including(x => x.BirthDate)
                );
            }
        }
        #endregion

        [TearDown]
        public void TearDown()
        {
            _factory.Dispose();
            _client.Dispose();
        }

    }
}
