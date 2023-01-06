using Business.Models;
using Data.Entities;
using Data.Interfaces;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Business.Services;

using Business.Validation;
using Library.Tests;

namespace TradeMarket.Tests.BusinessTests
{
    public class CustomerServiceTests
    {

        [Test]
        public async Task CustomerService_GetAll_ReturnsAllCustomers()
        {
            //arrange
            var expected = GetTestCustomerModels;
            var mockUnitOfWork = new Mock<IUnitOfWork>();

            mockUnitOfWork
                .Setup(x => x.CustomerRepository.GetAllWithDetailsAsync())
                .ReturnsAsync(GetTestCustomerEntities.AsEnumerable());

            var customerService = new CustomerService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());

            //act
            var actual = await customerService.GetAllAsync();

            //assert
            actual.Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task CustomerService_GetById_ReturnsCustomerModel()
        {
            //arrange
            var expected = GetTestCustomerModels.First();
            var mockUnitOfWork = new Mock<IUnitOfWork>();

            mockUnitOfWork
                .Setup(m => m.CustomerRepository.GetByIdWithDetailsAsync(It.IsAny<int>()))
                .ReturnsAsync(GetTestCustomerEntities.First());

            var customerService = new CustomerService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());

            //act
            var actual = await customerService.GetByIdAsync(1);

            //assert
            actual.Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task CustomerService_AddAsync_AddsModel()
        {
            //arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(m => m.CustomerRepository.AddAsync(It.IsAny<Customer>()));

            var customerService = new CustomerService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());
            var customer = GetTestCustomerModels.First();

            //act
            await customerService.AddAsync(customer);

            //assert
            mockUnitOfWork.Verify(x => x.CustomerRepository.AddAsync(It.Is<Customer>(x =>
                            x.Id == customer.Id && x.DiscountValue == customer.DiscountValue &&
                            x.Person.Surname == customer.Surname && x.Person.Name == customer.Name &&
                            x.Person.BirthDate == customer.BirthDate)), Times.Once);
            mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Once);
        }

        [Test]
        public async Task CustomerService_AddAsync_ThrowsMarketExceptionWithEmptyName()
        {
            //arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(m => m.CustomerRepository.AddAsync(It.IsAny<Customer>()));

            var customerService = new CustomerService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());
            var customer = GetTestCustomerModels.First();
            customer.Name = string.Empty;

            //act
            Func<Task> act = async () => await customerService.AddAsync(customer);

            //assert
            await act.Should().ThrowAsync<MarketException>();
        }


        [Test]
        public async Task CustomerService_AddAsync_ThrowsMarketExceptionWithNullObject()
        {
            //arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(m => m.CustomerRepository.AddAsync(It.IsAny<Customer>()));

            var customerService = new CustomerService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());

            //act
            Func<Task> act = async () => await customerService.AddAsync(null);

            //assert
            await act.Should().ThrowAsync<MarketException>();
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(100)]
        public async Task CustomerService_DeleteAsync_DeletesCustomer(int id)
        {
            //arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(m => m.CustomerRepository.DeleteByIdAsync(It.IsAny<int>()));
            var customerService = new CustomerService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());

            //act
            await customerService.DeleteAsync(id);

            //assert
            mockUnitOfWork.Verify(x => x.CustomerRepository.DeleteByIdAsync(id), Times.Once());
            mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Once());
        }

        [TestCase("2039-10-10")]
        [TestCase("1000-1-1")]
        public async Task CustomerService_AddAsync_ThrowsMarketExceptionWithInvalidDate(DateTime birthDate)
        {
            //arrange 
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(m => m.CustomerRepository.AddAsync(It.IsAny<Customer>()));

            var customerService = new CustomerService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());
            var customer = GetTestCustomerModels.First();
            customer.BirthDate = birthDate;

            //act
            Func<Task> act = async () => await customerService.AddAsync(customer);

            //assert
            await act.Should().ThrowAsync<MarketException>();
        }

        [Test]
        public async Task CustomerService_UpdateAsync_UpdatesCustomer()
        {
            //arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(m => m.CustomerRepository.Update(It.IsAny<Customer>()));
            mockUnitOfWork.Setup(m => m.PersonRepository.Update(It.IsAny<Person>()));

            var customerService = new CustomerService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());
            var customer = GetTestCustomerModels.First();

            //act
            await customerService.UpdateAsync(customer);

            //assert
            mockUnitOfWork.Verify(x => x.CustomerRepository.Update(It.Is<Customer>(x =>
                x.Id == customer.Id && x.DiscountValue == customer.DiscountValue &&
                x.Person.Surname == customer.Surname && x.Person.Name == customer.Name &&
                x.Person.BirthDate == customer.BirthDate)), Times.Once);
            mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Once);
        }

        [Test]
        public async Task CustomerService_UpdateAsync_ThrowsMarketExceptionWithEmptySurname()
        {
            //arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(m => m.CustomerRepository.Update(It.IsAny<Customer>()));

            var customerService = new CustomerService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());
            var customer = GetTestCustomerModels.Last();
            customer.Surname = null;

            //act
            Func<Task> act = async () => await customerService.UpdateAsync(customer);

            //assert
            await act.Should().ThrowAsync<MarketException>();
        }

        [TestCase("2030-1-1")]
        [TestCase("1430-1-1")]
        public async Task CustomerService_UpdateAsync_ThrowsMarketExceptionWithInvalidDate(DateTime birthDate)
        {
            //arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(m => m.CustomerRepository.Update(It.IsAny<Customer>()));

            var customerService = new CustomerService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());
            var customer = GetTestCustomerModels.First();
            customer.BirthDate = birthDate;

            //act
            Func<Task> act = async () => await customerService.UpdateAsync(customer);

            //assert
            await act.Should().ThrowAsync<MarketException>();
        }

        [TestCase(2, new[] { 1, 3 })]
        [TestCase(3, new[] { 1, 2 })]
        [TestCase(1, new[] { 1, 2, 3 })]
        public async Task CustomerService_GetCustomersByProductIdAsync_ReturnsCustomersWhoBoughtProduct(int productId, int[] expectedCustomerIds)
        {
            //arrange
            var expected = GetTestCustomerModels.Where(x => expectedCustomerIds.Contains(x.Id)).ToList();

            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork
                .Setup(m => m.CustomerRepository.GetAllWithDetailsAsync())
                .ReturnsAsync(GetTestCustomerEntities.AsQueryable());

            var customerService = new CustomerService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());

            //act
            var actual = await customerService.GetCustomersByProductIdAsync(productId);

            //assert
            actual.Should().BeEquivalentTo(expected);
        }

        #region TestData
        public List<CustomerModel> GetTestCustomerModels =>
            new List<CustomerModel>()
            {
                new CustomerModel { Id = 1, Name = "Viktor", Surname = "Zhuk",  BirthDate = new DateTime(1995, 1, 2), DiscountValue = 10, ReceiptsIds = new List<int>(){ 1 } },
                new CustomerModel { Id = 2, Name = "Nassim", Surname = "Taleb", BirthDate = new DateTime(1965, 5, 12), DiscountValue = 15, ReceiptsIds = new List<int>(){ 2 } },
                new CustomerModel { Id = 3, Name = "Desmond", Surname = "Morris", BirthDate = new DateTime(1955, 4, 12), DiscountValue = 5, ReceiptsIds = new List<int>(){ 3 } },
                new CustomerModel { Id = 4, Name = "Lebron", Surname = "James", BirthDate = new DateTime(1983, 12, 31), DiscountValue = 12, ReceiptsIds = new List<int>(){ 4 } }
            };

        public List<Person> GetTestPersonEntities =>
           new List<Person>()
           {
                new Person { Id = 1, Name = "Viktor", Surname = "Zhuk", BirthDate = new DateTime(1995, 1, 2) },
                new Person { Id = 2, Name = "Nassim", Surname = "Taleb", BirthDate = new DateTime(1965, 5, 12) },
                new Person { Id = 3, Name = "Desmond", Surname = "Morris",  BirthDate = new DateTime(1955, 4, 12) },
                new Person { Id = 4, Name = "Lebron", Surname = "James", BirthDate = new DateTime(1983, 12, 31) }
           };

        public List<Customer> GetTestCustomerEntities =>
            new List<Customer>()
            {
                new Customer { Id = 1, PersonId = 1, Person = GetTestPersonEntities[0], DiscountValue = 10, Receipts = new List<Receipt> {GetTestReceiptsEntitiesWithReceiptDetails[0]} },
                new Customer { Id = 2, PersonId = 2, Person = GetTestPersonEntities[1], DiscountValue = 15, Receipts = new List<Receipt> {GetTestReceiptsEntitiesWithReceiptDetails[1]} },
                new Customer { Id = 3, PersonId = 3, Person = GetTestPersonEntities[2], DiscountValue = 5, Receipts = new List<Receipt> {GetTestReceiptsEntitiesWithReceiptDetails[2]}},
                new Customer { Id = 4, PersonId = 4, Person = GetTestPersonEntities[3], DiscountValue = 12, Receipts = new List<Receipt> {GetTestReceiptsEntitiesWithReceiptDetails[3]} },
            };

        public List<Receipt> GetTestReceiptsEntitiesWithReceiptDetails =>
             new List<Receipt>()
             {
                new Receipt
                {
                    Id = 1, CustomerId = 1, IsCheckedOut = false,
                    ReceiptDetails = new List<ReceiptDetail>()
                    {
                        new ReceiptDetail { Id = 1, ProductId = 1, UnitPrice = 10, Quantity = 2, ReceiptId = 1 },
                        new ReceiptDetail { Id = 2, ProductId = 2, UnitPrice = 20, Quantity = 3, ReceiptId = 1},
                        new ReceiptDetail { Id = 3, ProductId = 3, UnitPrice = 25, Quantity = 1, ReceiptId = 1,}
                    }
                },
                new Receipt
                {
                    Id = 2, CustomerId = 2, IsCheckedOut = false,
                    ReceiptDetails = new List<ReceiptDetail>()
                    {
                        new ReceiptDetail { Id = 4, ProductId = 1, UnitPrice = 10, Quantity = 10, ReceiptId = 2 },
                        new ReceiptDetail { Id = 5, ProductId = 3, UnitPrice = 25, Quantity = 1, ReceiptId = 2}
                    }
                },
                new Receipt
                {
                    Id = 3, CustomerId = 3, IsCheckedOut = false,
                    ReceiptDetails = new List<ReceiptDetail>()
                    {
                        new ReceiptDetail { Id = 6, ProductId = 1, UnitPrice = 10, Quantity = 10, ReceiptId = 3 },
                        new ReceiptDetail { Id = 7, ProductId = 2, UnitPrice = 25, Quantity = 1, ReceiptId = 3}
                    }
                },
                  new Receipt
                {
                    Id = 4, CustomerId = 4, IsCheckedOut = false,
                    ReceiptDetails = new List<ReceiptDetail>()
                    {
                        new ReceiptDetail { Id = 8, ProductId = 5, UnitPrice = 10, Quantity = 10, ReceiptId = 4 },
                        new ReceiptDetail { Id = 8, ProductId = 6, UnitPrice = 25, Quantity = 1, ReceiptId = 4 }
                    }
                }
             };
    }
    #endregion
}
