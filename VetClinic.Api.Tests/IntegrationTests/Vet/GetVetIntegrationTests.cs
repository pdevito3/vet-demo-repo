
namespace VetClinic.Api.Tests.IntegrationTests.Vet
{
    using Application.Dtos.Vet;
    using FluentAssertions;
    using VetClinic.Api.Tests.Fakes.Vet;
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
    public class GetVetIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    { 
        private readonly CustomWebApplicationFactory _factory;

        public GetVetIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        
        [Fact]
        public async Task GetVets_ReturnsSuccessCodeAndResourceWithAccurateFields()
        {
            var fakeVetOne = new FakeVet { }.Generate();
            var fakeVetTwo = new FakeVet { }.Generate();

            var appFactory = _factory;
            using (var scope = appFactory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<VetClinicDbContext>();
                context.Database.EnsureCreated();

                //context.Vets.RemoveRange(context.Vets);
                context.Vets.AddRange(fakeVetOne, fakeVetTwo);
                context.SaveChanges();
            }

            var client = appFactory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            var result = await client.GetAsync("api/Vets")
                .ConfigureAwait(false);
            var responseContent = await result.Content.ReadAsStringAsync()
                .ConfigureAwait(false);
            var response = JsonConvert.DeserializeObject<Response<IEnumerable<VetDto>>>(responseContent).Data;

            // Assert
            result.StatusCode.Should().Be(200);
            response.Should().ContainEquivalentOf(fakeVetOne, options =>
                options.ExcludingMissingMembers());
            response.Should().ContainEquivalentOf(fakeVetTwo, options =>
                options.ExcludingMissingMembers());
        }
    } 
}