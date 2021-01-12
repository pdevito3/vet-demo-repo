
namespace VetClinic.Api.Tests.RepositoryTests.Pet
{
    using Application.Dtos.Pet;
    using FluentAssertions;
    using VetClinic.Api.Tests.Fakes.Pet;
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
    public class DeletePetRepositoryTests
    { 
        
        [Fact]
        public void DeletePet_ReturnsProperCount()
        {
            //Arrange
            var dbOptions = new DbContextOptionsBuilder<VetClinicDbContext>()
                .UseInMemoryDatabase(databaseName: $"PetDb{Guid.NewGuid()}")
                .Options;
            var sieveOptions = Options.Create(new SieveOptions());

            var fakePetOne = new FakePet { }.Generate();
            var fakePetTwo = new FakePet { }.Generate();
            var fakePetThree = new FakePet { }.Generate();

            //Act
            using (var context = new VetClinicDbContext(dbOptions))
            {
                context.Pets.AddRange(fakePetOne, fakePetTwo, fakePetThree);

                var service = new PetRepository(context, new SieveProcessor(sieveOptions));
                service.DeletePet(fakePetTwo);

                context.SaveChanges();

                //Assert
                var petList = context.Pets.ToList();

                petList.Should()
                    .NotBeEmpty()
                    .And.HaveCount(2);

                petList.Should().ContainEquivalentOf(fakePetOne);
                petList.Should().ContainEquivalentOf(fakePetThree);
                Assert.DoesNotContain(petList, p => p == fakePetTwo);

                context.Database.EnsureDeleted();
            }
        }
    } 
}