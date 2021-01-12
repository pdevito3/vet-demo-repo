namespace Infrastructure.Persistence.Seeders
{

    using AutoBogus;
    using Domain.Entities;
    using Infrastructure.Persistence.Contexts;
    using System.Linq;

    public static class PetSeeder
    {
        public static void SeedSamplePetData(VetClinicDbContext context)
        {
            if (!context.Pets.Any())
            {
                context.Pets.Add(new AutoFaker<Pet>());
                context.Pets.Add(new AutoFaker<Pet>());
                context.Pets.Add(new AutoFaker<Pet>());

                context.SaveChanges();
            }
        }
    }
}