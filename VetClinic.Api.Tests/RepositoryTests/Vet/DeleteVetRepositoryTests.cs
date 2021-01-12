
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
    public class DeleteVetRepositoryTests
    { 
        
        [Fact]
        public void DeleteVet_ReturnsProperCount()
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

                var service = new VetRepository(context, new SieveProcessor(sieveOptions));
                service.DeleteVet(fakeVetTwo);

                context.SaveChanges();

                //Assert
                var vetList = context.Vets.ToList();

                vetList.Should()
                    .NotBeEmpty()
                    .And.HaveCount(2);

                vetList.Should().ContainEquivalentOf(fakeVetOne);
                vetList.Should().ContainEquivalentOf(fakeVetThree);
                Assert.DoesNotContain(vetList, v => v == fakeVetTwo);

                context.Database.EnsureDeleted();
            }
        }
    } 
}