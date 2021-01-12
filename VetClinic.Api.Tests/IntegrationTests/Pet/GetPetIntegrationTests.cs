
namespace VetClinic.Api.Tests.IntegrationTests.Pet
{
    using Application.Dtos.Pet;
    using FluentAssertions;
    using VetClinic.Api.Tests.Fakes.Pet;
    using Microsoft.AspNetCore.Mvc.Testing;
    using System.Threading.Tasks;
    using Xunit;
    using Newtonsoft.Json;
    using System.Net.Http;
    using WebApi;
    using System.Collections.Generic;
    using Infrastructure.Persistence.Contexts;
    using Microsoft.Extensions.DependencyInjection;
    using Application.Wrappers;

    [Collection("Sequential")]
    public class GetPetIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    { 
        private readonly CustomWebApplicationFactory _factory;

        public GetPetIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        
        [Fact]
        public async Task GetPets_ReturnsSuccessCodeAndResourceWithAccurateFields()
        {
            var fakePetOne = new FakePet { }.Generate();
            var fakePetTwo = new FakePet { }.Generate();

            var appFactory = _factory;
            using (var scope = appFactory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<VetClinicDbContext>();
                context.Database.EnsureCreated();

                //context.Pets.RemoveRange(context.Pets);
                context.Pets.AddRange(fakePetOne, fakePetTwo);
                context.SaveChanges();
            }

            var client = appFactory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            var result = await client.GetAsync("api/Pets")
                .ConfigureAwait(false);
            var responseContent = await result.Content.ReadAsStringAsync()
                .ConfigureAwait(false);
            var response = JsonConvert.DeserializeObject<Response<IEnumerable<PetDto>>>(responseContent).Data;

            // Assert
            result.StatusCode.Should().Be(200);
            response.Should().ContainEquivalentOf(fakePetOne, options =>
                options.ExcludingMissingMembers());
            response.Should().ContainEquivalentOf(fakePetTwo, options =>
                options.ExcludingMissingMembers());
        }
    } 
}