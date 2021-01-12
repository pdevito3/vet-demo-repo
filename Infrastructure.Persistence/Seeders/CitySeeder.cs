namespace Infrastructure.Persistence.Seeders
{

    using AutoBogus;
    using Domain.Entities;
    using Infrastructure.Persistence.Contexts;
    using System.Linq;

    public static class CitySeeder
    {
        public static void SeedSampleCityData(VetClinicDbContext context)
        {
            if (!context.Cities.Any())
            {
                context.Cities.Add(new AutoFaker<City>());
                context.Cities.Add(new AutoFaker<City>());
                context.Cities.Add(new AutoFaker<City>());

                context.SaveChanges();
            }
        }
    }
}