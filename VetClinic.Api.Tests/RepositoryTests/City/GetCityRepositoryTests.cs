
namespace VetClinic.Api.Tests.RepositoryTests.City
{
    using Application.Dtos.City;
    using FluentAssertions;
    using VetClinic.Api.Tests.Fakes.City;
    using Infrastructure.Persistence.Contexts;
    using Infrastructure.Persistence.Repositories;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using Sieve.Models;
    using Sieve.Services;
    using System;
    using System.Linq;
    using Xunit;
    using Application.Interfaces;
    using Moq;

    [Collection("Sequential")]
    public class GetCityRepositoryTests
    { 
        
        [Fact]
        public void GetCity_ParametersMatchExpectedValues()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VetClinicDbContext>()
                .UseInMemoryDatabase(databaseName: $"CityDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakeCity = new FakeCity { }.Generate();

            //Act
            using (var context = new VetClinicDbContext(dbOptions))
            {
                context.Cities.AddRange(fakeCity);
                context.SaveChanges();

                var service = new CityRepository(context, new SieveProcessor(sieveOptions));

                //Assert
                var cityById = service.GetCity(fakeCity.CityId);
                                cityById.CityId.Should().Be(fakeCity.CityId);
                cityById.Name.Should().Be(fakeCity.Name);
            }
        }
        
        [Fact]
        public async void GetCitiesAsync_CountMatchesAndContainsEquivalentObjects()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VetClinicDbContext>()
                .UseInMemoryDatabase(databaseName: $"CityDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakeCityOne = new FakeCity { }.Generate();
            var fakeCityTwo = new FakeCity { }.Generate();
            var fakeCityThree = new FakeCity { }.Generate();

            //Act
            using (var context = new VetClinicDbContext(dbOptions))
            {
                context.Cities.AddRange(fakeCityOne, fakeCityTwo, fakeCityThree);
                context.SaveChanges();

                var service = new CityRepository(context, new SieveProcessor(sieveOptions));

                var cityRepo = await service.GetCitiesAsync(new CityParametersDto());

                //Assert
                cityRepo.Should()
                    .NotBeEmpty()
                    .And.HaveCount(3);

                cityRepo.Should().ContainEquivalentOf(fakeCityOne);
                cityRepo.Should().ContainEquivalentOf(fakeCityTwo);
                cityRepo.Should().ContainEquivalentOf(fakeCityThree);

                context.Database.EnsureDeleted();
            }
        }
        
        [Fact]
        public async void GetCitiesAsync_ReturnExpectedPageSize()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VetClinicDbContext>()
                .UseInMemoryDatabase(databaseName: $"CityDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakeCityOne = new FakeCity { }.Generate();
            var fakeCityTwo = new FakeCity { }.Generate();
            var fakeCityThree = new FakeCity { }.Generate();
            
            // need id's due to default sorting
            fakeCityOne.CityId = 1;
            fakeCityTwo.CityId = 2;
            fakeCityThree.CityId = 3;

            //Act
            using (var context = new VetClinicDbContext(dbOptions))
            {
                context.Cities.AddRange(fakeCityOne, fakeCityTwo, fakeCityThree);
                context.SaveChanges();

                var service = new CityRepository(context, new SieveProcessor(sieveOptions));

                var cityRepo = await service.GetCitiesAsync(new CityParametersDto { PageSize = 2 });

                //Assert
                cityRepo.Should()
                    .NotBeEmpty()
                    .And.HaveCount(2);

                cityRepo.Should().ContainEquivalentOf(fakeCityOne);
                cityRepo.Should().ContainEquivalentOf(fakeCityTwo);

                context.Database.EnsureDeleted();
            }
        }
        
        [Fact]
        public async void GetCitiesAsync_ReturnExpectedPageNumberAndSize()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VetClinicDbContext>()
                .UseInMemoryDatabase(databaseName: $"CityDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakeCityOne = new FakeCity { }.Generate();
            var fakeCityTwo = new FakeCity { }.Generate();
            var fakeCityThree = new FakeCity { }.Generate();
            
            // need id's due to default sorting
            fakeCityOne.CityId = 1;
            fakeCityTwo.CityId = 2;
            fakeCityThree.CityId = 3;

            //Act
            using (var context = new VetClinicDbContext(dbOptions))
            {
                context.Cities.AddRange(fakeCityOne, fakeCityTwo, fakeCityThree);
                context.SaveChanges();

                var service = new CityRepository(context, new SieveProcessor(sieveOptions));

                var cityRepo = await service.GetCitiesAsync(new CityParametersDto { PageSize = 1, PageNumber = 2 });

                //Assert
                cityRepo.Should()
                    .NotBeEmpty()
                    .And.HaveCount(1);

                cityRepo.Should().ContainEquivalentOf(fakeCityTwo);

                context.Database.EnsureDeleted();
            }
        }
        
        
        [Fact]
        public async void GetCitiesAsync_FilterCityIdListWithExact()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VetClinicDbContext>()
                .UseInMemoryDatabase(databaseName: $"CityDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakeCityOne = new FakeCity { }.Generate();
            fakeCityOne.CityId = 1;

            var fakeCityTwo = new FakeCity { }.Generate();
            fakeCityTwo.CityId = 2;

            var fakeCityThree = new FakeCity { }.Generate();
            fakeCityThree.CityId = 3;

            //Act
            using (var context = new VetClinicDbContext(dbOptions))
            {
                context.Cities.AddRange(fakeCityOne, fakeCityTwo, fakeCityThree);
                context.SaveChanges();

                var service = new CityRepository(context, new SieveProcessor(sieveOptions));

                var cityRepo = await service.GetCitiesAsync(new CityParametersDto { Filters = $"CityId == {fakeCityTwo.CityId}" });

                //Assert
                cityRepo.Should()
                    .HaveCount(1);

                context.Database.EnsureDeleted();
            }
        }

    } 
}