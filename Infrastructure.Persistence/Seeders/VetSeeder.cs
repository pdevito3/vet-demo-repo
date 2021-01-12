namespace Infrastructure.Persistence.Seeders
{

    using AutoBogus;
    using Domain.Entities;
    using Infrastructure.Persistence.Contexts;
    using System.Linq;

    public static class VetSeeder
    {
        public static void SeedSampleVetData(VetClinicDbContext context)
        {
            if (!context.Vets.Any())
            {
                context.Vets.Add(new AutoFaker<Vet>());
                context.Vets.Add(new AutoFaker<Vet>());
                context.Vets.Add(new AutoFaker<Vet>());

                context.SaveChanges();
            }
        }
    }
}