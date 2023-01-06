using Data.Data;
using Data.Entities;
using Data.Repositories;
using Library.Tests;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TradeMarket.Tests.DataTests
{
    [TestFixture]
    public class PersonRepositoryTests
    {
        [TestCase(1)]
        [TestCase(2)]
        public async Task PersonRepository_GetByIdAsync_ReturnsSingleValue(int id)
        {
            using var context = new TradeMarketDbContext(UnitTestHelper.GetUnitTestDbOptions());

            var personRepository = new PersonRepository(context);
            var person = await personRepository.GetByIdAsync(id);

            var expected = ExpectedPersons.FirstOrDefault(x => x.Id == id);

            Assert.That(person, Is.EqualTo(expected).Using(new PersonEqualityComparer()), message: "GetByIdAsync method works incorrect");
        }

        [Test]
        public async Task PersonRepository_GetAllAsync_ReturnsAllValues()
        {
            using var context = new TradeMarketDbContext(UnitTestHelper.GetUnitTestDbOptions());

            var personRepository = new PersonRepository(context);
            var persons = await personRepository.GetAllAsync();

            Assert.That(persons, Is.EqualTo(ExpectedPersons).Using(new PersonEqualityComparer()), message: "GetAllAsync method works incorrect");
        }

        [Test]
        public async Task PersonRepository_AddAsync_AddsValueToDatabase()
        {
            using var context = new TradeMarketDbContext(UnitTestHelper.GetUnitTestDbOptions());

            var personRepository = new PersonRepository(context);
            var person = new Person { Id = 3 };

            await personRepository.AddAsync(person);
            await context.SaveChangesAsync();

            Assert.That(context.Persons.Count(), Is.EqualTo(3), message: "AddAsync method works incorrect");
        }

        [Test]
        public async Task PersonRepository_DeleteByIdAsync_DeletesEntity()
        {
            using var context = new TradeMarketDbContext(UnitTestHelper.GetUnitTestDbOptions());

            var personRepository = new PersonRepository(context);

            await personRepository.DeleteByIdAsync(1);
            await context.SaveChangesAsync();

            Assert.That(context.Persons.Count(), Is.EqualTo(1), message: "DeleteByIdAsync works incorrect");
        }

        [Test]
        public async Task PersonRepository_Update_UpdatesEntity()
        {
            using var context = new TradeMarketDbContext(UnitTestHelper.GetUnitTestDbOptions());

            var personRepository = new PersonRepository(context);
            var person = new Person
            {
                Id = 1,
                Name = "Name_Updated",
                Surname = "Surname_Updated",
                BirthDate = new DateTime(1980, 7, 25)
            };

            personRepository.Update(person);
            await context.SaveChangesAsync();

            Assert.That(person, Is.EqualTo(new Person
            {
                Id = 1,
                Name = "Name_Updated",
                Surname = "Surname_Updated",
                BirthDate = new DateTime(1980, 7, 25)
            }).Using(new PersonEqualityComparer()), message: "Update method works incorrect");
        }

        private static IEnumerable<Person> ExpectedPersons =>
            new[]
            {
                new Person { Id = 1, Name = "Name1", Surname = "Surname1", BirthDate = new DateTime(1980, 5, 25) },
                new Person { Id = 2, Name = "Name2", Surname = "Surname2", BirthDate = new DateTime(1984, 10, 19) }
            };
    }
}
