
namespace VetClinic.Api.Tests.IntegrationTests.City
{
    using Application.Dtos.City;
    using FluentAssertions;
    using VetClinic.Api.Tests.Fakes.City;
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
    public class GetCityIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    { 
        private readonly CustomWebApplicationFactory _factory;

        public GetCityIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        
        [Fact]
        public async Task GetCities_ReturnsSuccessCodeAndResourceWithAccurateFields()
        {
            var fakeCityOne = new FakeCity { }.Generate();
            var fakeCityTwo = new FakeCity { }.Generate();

            var appFactory = _factory;
            using (var scope = appFactory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<VetClinicDbContext>();
                context.Database.EnsureCreated();

                //context.Cities.RemoveRange(context.Cities);
                context.Cities.AddRange(fakeCityOne, fakeCityTwo);
                context.SaveChanges();
            }

            var client = appFactory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            var result = await client.GetAsync("api/Cities")
                .ConfigureAwait(false);
            var responseContent = await result.Content.ReadAsStringAsync()
                .ConfigureAwait(false);
            var response = JsonConvert.DeserializeObject<Response<IEnumerable<CityDto>>>(responseContent).Data;

            // Assert
            result.StatusCode.Should().Be(200);
            response.Should().ContainEquivalentOf(fakeCityOne, options =>
                options.ExcludingMissingMembers());
            response.Should().ContainEquivalentOf(fakeCityTwo, options =>
                options.ExcludingMissingMembers());
        }
    } 
}