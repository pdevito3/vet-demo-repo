namespace Infrastructure.Persistence
{
    using Infrastructure.Persistence.Contexts;
    using Application.Interfaces.City;
    using Application.Interfaces.Vet;
    using Application.Interfaces.Pet;
    using Infrastructure.Persistence.Repositories;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Sieve.Services;

    public static class ServiceRegistration
    {
        public static void AddPersistenceInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            #region DbContext -- Do Not Delete            
            if (configuration.GetValue<bool>("UseInMemoryDatabase"))
            {
                services.AddDbContext<VetClinicDbContext>(options =>
                    options.UseInMemoryDatabase($"VetClinic"));
            }
            else
            {
                services.AddDbContext<VetClinicDbContext>(options =>
                    options.UseSqlServer(
                        configuration.GetConnectionString("VetClinic"),
                        builder => builder.MigrationsAssembly(typeof(VetClinicDbContext).Assembly.FullName)));
            }
            #endregion

            services.AddScoped<SieveProcessor>();

            #region Repositories -- Do Not Delete
            services.AddScoped<ICityRepository, CityRepository>();
            services.AddScoped<IVetRepository, VetRepository>();
            services.AddScoped<IPetRepository, PetRepository>();
            #endregion
        }
    }
}
