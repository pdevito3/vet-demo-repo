
namespace VetClinic.Api.Tests.RepositoryTests.Vet
{
    using Application.Dtos.Vet;
    using FluentAssertions;
    using VetClinic.Api.Tests.Fakes.Vet;
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
    public class GetVetRepositoryTests
    { 
        
        [Fact]
        public void GetVet_ParametersMatchExpectedValues()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VetClinicDbContext>()
                .UseInMemoryDatabase(databaseName: $"VetDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakeVet = new FakeVet { }.Generate();

            //Act
            using (var context = new VetClinicDbContext(dbOptions))
            {
                context.Vets.AddRange(fakeVet);
                context.SaveChanges();

                var service = new VetRepository(context, new SieveProcessor(sieveOptions));

                //Assert
                var vetById = service.GetVet(fakeVet.VetId);
                                vetById.VetId.Should().Be(fakeVet.VetId);
                vetById.Name.Should().Be(fakeVet.Name);
                vetById.Capacity.Should().Be(fakeVet.Capacity);
                vetById.OpenDate.Should().Be(fakeVet.OpenDate);
                vetById.HasSpayNeuter.Should().Be(fakeVet.HasSpayNeuter);
            }
        }
        
        [Fact]
        public async void GetVetsAsync_CountMatchesAndContainsEquivalentObjects()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VetClinicDbContext>()
                .UseInMemoryDatabase(databaseName: $"VetDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakeVetOne = new FakeVet { }.Generate();
            var fakeVetTwo = new FakeVet { }.Generate();
            var fakeVetThree = new FakeVet { }.Generate();

            //Act
            using (var context = new VetClinicDbContext(dbOptions))
            {
                context.Vets.AddRange(fakeVetOne, fakeVetTwo, fakeVetThree);
                context.SaveChanges();

                var service = new VetRepository(context, new SieveProcessor(sieveOptions));

                var vetRepo = await service.GetVetsAsync(new VetParametersDto());

                //Assert
                vetRepo.Should()
                    .NotBeEmpty()
                    .And.HaveCount(3);

                vetRepo.Should().ContainEquivalentOf(fakeVetOne);
                vetRepo.Should().ContainEquivalentOf(fakeVetTwo);
                vetRepo.Should().ContainEquivalentOf(fakeVetThree);

                context.Database.EnsureDeleted();
            }
        }
        
        [Fact]
        public async void GetVetsAsync_ReturnExpectedPageSize()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VetClinicDbContext>()
                .UseInMemoryDatabase(databaseName: $"VetDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakeVetOne = new FakeVet { }.Generate();
            var fakeVetTwo = new FakeVet { }.Generate();
            var fakeVetThree = new FakeVet { }.Generate();
            
            // need id's due to default sorting
            fakeVetOne.VetId = 1;
            fakeVetTwo.VetId = 2;
            fakeVetThree.VetId = 3;

            //Act
            using (var context = new VetClinicDbContext(dbOptions))
            {
                context.Vets.AddRange(fakeVetOne, fakeVetTwo, fakeVetThree);
                context.SaveChanges();

                var service = new VetRepository(context, new SieveProcessor(sieveOptions));

                var vetRepo = await service.GetVetsAsync(new VetParametersDto { PageSize = 2 });

                //Assert
                vetRepo.Should()
                    .NotBeEmpty()
                    .And.HaveCount(2);

                vetRepo.Should().ContainEquivalentOf(fakeVetOne);
                vetRepo.Should().ContainEquivalentOf(fakeVetTwo);

                context.Database.EnsureDeleted();
            }
        }
        
        [Fact]
        public async void GetVetsAsync_ReturnExpectedPageNumberAndSize()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VetClinicDbContext>()
                .UseInMemoryDatabase(databaseName: $"VetDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakeVetOne = new FakeVet { }.Generate();
            var fakeVetTwo = new FakeVet { }.Generate();
            var fakeVetThree = new FakeVet { }.Generate();
            
            // need id's due to default sorting
            fakeVetOne.VetId = 1;
            fakeVetTwo.VetId = 2;
            fakeVetThree.VetId = 3;

            //Act
            using (var context = new VetClinicDbContext(dbOptions))
            {
                context.Vets.AddRange(fakeVetOne, fakeVetTwo, fakeVetThree);
                context.SaveChanges();

                var service = new VetRepository(context, new SieveProcessor(sieveOptions));

                var vetRepo = await service.GetVetsAsync(new VetParametersDto { PageSize = 1, PageNumber = 2 });

                //Assert
                vetRepo.Should()
                    .NotBeEmpty()
                    .And.HaveCount(1);

                vetRepo.Should().ContainEquivalentOf(fakeVetTwo);

                context.Database.EnsureDeleted();
            }
        }
        
        [Fact]
        public async void GetVetsAsync_ListNameSortedInAscOrder()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VetClinicDbContext>()
                .UseInMemoryDatabase(databaseName: $"VetDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakeVetOne = new FakeVet { }.Generate();
            fakeVetOne.Name = "bravo";

            var fakeVetTwo = new FakeVet { }.Generate();
            fakeVetTwo.Name = "alpha";

            var fakeVetThree = new FakeVet { }.Generate();
            fakeVetThree.Name = "charlie";

            //Act
            using (var context = new VetClinicDbContext(dbOptions))
            {
                context.Vets.AddRange(fakeVetOne, fakeVetTwo, fakeVetThree);
                context.SaveChanges();

                var service = new VetRepository(context, new SieveProcessor(sieveOptions));

                var vetRepo = await service.GetVetsAsync(new VetParametersDto { SortOrder = "Name" });

                //Assert
                vetRepo.Should()
                    .ContainInOrder(fakeVetTwo, fakeVetOne, fakeVetThree);

                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async void GetVetsAsync_ListNameSortedInDescOrder()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VetClinicDbContext>()
                .UseInMemoryDatabase(databaseName: $"VetDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakeVetOne = new FakeVet { }.Generate();
            fakeVetOne.Name = "bravo";

            var fakeVetTwo = new FakeVet { }.Generate();
            fakeVetTwo.Name = "alpha";

            var fakeVetThree = new FakeVet { }.Generate();
            fakeVetThree.Name = "charlie";

            //Act
            using (var context = new VetClinicDbContext(dbOptions))
            {
                context.Vets.AddRange(fakeVetOne, fakeVetTwo, fakeVetThree);
                context.SaveChanges();

                var service = new VetRepository(context, new SieveProcessor(sieveOptions));

                var vetRepo = await service.GetVetsAsync(new VetParametersDto { SortOrder = "-Name" });

                //Assert
                vetRepo.Should()
                    .ContainInOrder(fakeVetThree, fakeVetOne, fakeVetTwo);

                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async void GetVetsAsync_ListCapacitySortedInAscOrder()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VetClinicDbContext>()
                .UseInMemoryDatabase(databaseName: $"VetDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakeVetOne = new FakeVet { }.Generate();
            fakeVetOne.Capacity = 2;

            var fakeVetTwo = new FakeVet { }.Generate();
            fakeVetTwo.Capacity = 1;

            var fakeVetThree = new FakeVet { }.Generate();
            fakeVetThree.Capacity = 3;

            //Act
            using (var context = new VetClinicDbContext(dbOptions))
            {
                context.Vets.AddRange(fakeVetOne, fakeVetTwo, fakeVetThree);
                context.SaveChanges();

                var service = new VetRepository(context, new SieveProcessor(sieveOptions));

                var vetRepo = await service.GetVetsAsync(new VetParametersDto { SortOrder = "Capacity" });

                //Assert
                vetRepo.Should()
                    .ContainInOrder(fakeVetTwo, fakeVetOne, fakeVetThree);

                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async void GetVetsAsync_ListCapacitySortedInDescOrder()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VetClinicDbContext>()
                .UseInMemoryDatabase(databaseName: $"VetDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakeVetOne = new FakeVet { }.Generate();
            fakeVetOne.Capacity = 2;

            var fakeVetTwo = new FakeVet { }.Generate();
            fakeVetTwo.Capacity = 1;

            var fakeVetThree = new FakeVet { }.Generate();
            fakeVetThree.Capacity = 3;

            //Act
            using (var context = new VetClinicDbContext(dbOptions))
            {
                context.Vets.AddRange(fakeVetOne, fakeVetTwo, fakeVetThree);
                context.SaveChanges();

                var service = new VetRepository(context, new SieveProcessor(sieveOptions));

                var vetRepo = await service.GetVetsAsync(new VetParametersDto { SortOrder = "-Capacity" });

                //Assert
                vetRepo.Should()
                    .ContainInOrder(fakeVetThree, fakeVetOne, fakeVetTwo);

                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async void GetVetsAsync_ListOpenDateSortedInAscOrder()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VetClinicDbContext>()
                .UseInMemoryDatabase(databaseName: $"VetDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakeVetOne = new FakeVet { }.Generate();
            fakeVetOne.OpenDate = DateTime.Now.AddDays(2);

            var fakeVetTwo = new FakeVet { }.Generate();
            fakeVetTwo.OpenDate = DateTime.Now.AddDays(1);

            var fakeVetThree = new FakeVet { }.Generate();
            fakeVetThree.OpenDate = DateTime.Now.AddDays(3);

            //Act
            using (var context = new VetClinicDbContext(dbOptions))
            {
                context.Vets.AddRange(fakeVetOne, fakeVetTwo, fakeVetThree);
                context.SaveChanges();

                var service = new VetRepository(context, new SieveProcessor(sieveOptions));

                var vetRepo = await service.GetVetsAsync(new VetParametersDto { SortOrder = "OpenDate" });

                //Assert
                vetRepo.Should()
                    .ContainInOrder(fakeVetTwo, fakeVetOne, fakeVetThree);

                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async void GetVetsAsync_ListOpenDateSortedInDescOrder()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VetClinicDbContext>()
                .UseInMemoryDatabase(databaseName: $"VetDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakeVetOne = new FakeVet { }.Generate();
            fakeVetOne.OpenDate = DateTime.Now.AddDays(2);

            var fakeVetTwo = new FakeVet { }.Generate();
            fakeVetTwo.OpenDate = DateTime.Now.AddDays(1);

            var fakeVetThree = new FakeVet { }.Generate();
            fakeVetThree.OpenDate = DateTime.Now.AddDays(3);

            //Act
            using (var context = new VetClinicDbContext(dbOptions))
            {
                context.Vets.AddRange(fakeVetOne, fakeVetTwo, fakeVetThree);
                context.SaveChanges();

                var service = new VetRepository(context, new SieveProcessor(sieveOptions));

                var vetRepo = await service.GetVetsAsync(new VetParametersDto { SortOrder = "-OpenDate" });

                //Assert
                vetRepo.Should()
                    .ContainInOrder(fakeVetThree, fakeVetOne, fakeVetTwo);

                context.Database.EnsureDeleted();
            }
        }

        
        [Fact]
        public async void GetVetsAsync_FilterVetIdListWithExact()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VetClinicDbContext>()
                .UseInMemoryDatabase(databaseName: $"VetDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakeVetOne = new FakeVet { }.Generate();
            fakeVetOne.VetId = 1;

            var fakeVetTwo = new FakeVet { }.Generate();
            fakeVetTwo.VetId = 2;

            var fakeVetThree = new FakeVet { }.Generate();
            fakeVetThree.VetId = 3;

            //Act
            using (var context = new VetClinicDbContext(dbOptions))
            {
                context.Vets.AddRange(fakeVetOne, fakeVetTwo, fakeVetThree);
                context.SaveChanges();

                var service = new VetRepository(context, new SieveProcessor(sieveOptions));

                var vetRepo = await service.GetVetsAsync(new VetParametersDto { Filters = $"VetId == {fakeVetTwo.VetId}" });

                //Assert
                vetRepo.Should()
                    .HaveCount(1);

                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async void GetVetsAsync_FilterNameListWithExact()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VetClinicDbContext>()
                .UseInMemoryDatabase(databaseName: $"VetDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakeVetOne = new FakeVet { }.Generate();
            fakeVetOne.Name = "alpha";

            var fakeVetTwo = new FakeVet { }.Generate();
            fakeVetTwo.Name = "bravo";

            var fakeVetThree = new FakeVet { }.Generate();
            fakeVetThree.Name = "charlie";

            //Act
            using (var context = new VetClinicDbContext(dbOptions))
            {
                context.Vets.AddRange(fakeVetOne, fakeVetTwo, fakeVetThree);
                context.SaveChanges();

                var service = new VetRepository(context, new SieveProcessor(sieveOptions));

                var vetRepo = await service.GetVetsAsync(new VetParametersDto { Filters = $"Name == {fakeVetTwo.Name}" });

                //Assert
                vetRepo.Should()
                    .HaveCount(1);

                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async void GetVetsAsync_FilterCapacityListWithExact()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VetClinicDbContext>()
                .UseInMemoryDatabase(databaseName: $"VetDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakeVetOne = new FakeVet { }.Generate();
            fakeVetOne.Capacity = 1;

            var fakeVetTwo = new FakeVet { }.Generate();
            fakeVetTwo.Capacity = 2;

            var fakeVetThree = new FakeVet { }.Generate();
            fakeVetThree.Capacity = 3;

            //Act
            using (var context = new VetClinicDbContext(dbOptions))
            {
                context.Vets.AddRange(fakeVetOne, fakeVetTwo, fakeVetThree);
                context.SaveChanges();

                var service = new VetRepository(context, new SieveProcessor(sieveOptions));

                var vetRepo = await service.GetVetsAsync(new VetParametersDto { Filters = $"Capacity == {fakeVetTwo.Capacity}" });

                //Assert
                vetRepo.Should()
                    .HaveCount(1);

                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async void GetVetsAsync_FilterOpenDateListWithExact()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VetClinicDbContext>()
                .UseInMemoryDatabase(databaseName: $"VetDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakeVetOne = new FakeVet { }.Generate();
            fakeVetOne.OpenDate = DateTime.Now.AddDays(1);

            var fakeVetTwo = new FakeVet { }.Generate();
            fakeVetTwo.OpenDate = DateTime.Parse(DateTime.Now.AddDays(2).ToString("MM/dd/yyyy"));

            var fakeVetThree = new FakeVet { }.Generate();
            fakeVetThree.OpenDate = DateTime.Now.AddDays(3);

            //Act
            using (var context = new VetClinicDbContext(dbOptions))
            {
                context.Vets.AddRange(fakeVetOne, fakeVetTwo, fakeVetThree);
                context.SaveChanges();

                var service = new VetRepository(context, new SieveProcessor(sieveOptions));

                var vetRepo = await service.GetVetsAsync(new VetParametersDto { Filters = $"OpenDate == {fakeVetTwo.OpenDate}" });

                //Assert
                vetRepo.Should()
                    .HaveCount(1);

                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async void GetVetsAsync_FilterHasSpayNeuterListWithExact()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VetClinicDbContext>()
                .UseInMemoryDatabase(databaseName: $"VetDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakeVetOne = new FakeVet { }.Generate();
            fakeVetOne.HasSpayNeuter = false;

            var fakeVetTwo = new FakeVet { }.Generate();
            fakeVetTwo.HasSpayNeuter = true;

            var fakeVetThree = new FakeVet { }.Generate();
            fakeVetThree.HasSpayNeuter = false;

            //Act
            using (var context = new VetClinicDbContext(dbOptions))
            {
                context.Vets.AddRange(fakeVetOne, fakeVetTwo, fakeVetThree);
                context.SaveChanges();

                var service = new VetRepository(context, new SieveProcessor(sieveOptions));

                var vetRepo = await service.GetVetsAsync(new VetParametersDto { Filters = $"HasSpayNeuter == {fakeVetTwo.HasSpayNeuter}" });

                //Assert
                vetRepo.Should()
                    .HaveCount(1);

                context.Database.EnsureDeleted();
            }
        }

    } 
}