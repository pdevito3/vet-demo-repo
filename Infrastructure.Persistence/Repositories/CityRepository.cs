namespace Infrastructure.Persistence.Repositories
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Application.Dtos.City;
    using Application.Interfaces.City;
    using Application.Wrappers;
    using Domain.Entities;
    using Infrastructure.Persistence.Contexts;
    using Microsoft.EntityFrameworkCore;
    using Sieve.Models;
    using Sieve.Services;

    public class CityRepository : ICityRepository
    {
        private VetClinicDbContext _context;
        private readonly SieveProcessor _sieveProcessor;

        public CityRepository(VetClinicDbContext context,
            SieveProcessor sieveProcessor)
        {
            _context = context
                ?? throw new ArgumentNullException(nameof(context));
            _sieveProcessor = sieveProcessor ??
                throw new ArgumentNullException(nameof(sieveProcessor));
        }

        public async Task<PagedList<City>> GetCitiesAsync(CityParametersDto cityParameters)
        {
            if (cityParameters == null)
            {
                throw new ArgumentNullException(nameof(cityParameters));
            }

            var collection = _context.Cities
                as IQueryable<City>; // TODO: AsNoTracking() should increase performance, but will break the sort tests. need to investigate

            var sieveModel = new SieveModel
            {
                Sorts = cityParameters.SortOrder ?? "CityId",
                Filters = cityParameters.Filters
            };

            collection = _sieveProcessor.Apply(sieveModel, collection);

            return await PagedList<City>.CreateAsync(collection,
                cityParameters.PageNumber,
                cityParameters.PageSize);
        }

        public async Task<City> GetCityAsync(int cityId)
        {
            // include marker -- requires return _context.Cities as it's own line with no extra text -- do not delete this comment
            return await _context.Cities
                .FirstOrDefaultAsync(c => c.CityId == cityId);
        }

        public City GetCity(int cityId)
        {
            // include marker -- requires return _context.Cities as it's own line with no extra text -- do not delete this comment
            return _context.Cities
                .FirstOrDefault(c => c.CityId == cityId);
        }

        public async Task AddCity(City city)
        {
            if (city == null)
            {
                throw new ArgumentNullException(nameof(City));
            }

            await _context.Cities.AddAsync(city);
        }

        public void DeleteCity(City city)
        {
            if (city == null)
            {
                throw new ArgumentNullException(nameof(City));
            }

            _context.Cities.Remove(city);
        }

        public void UpdateCity(City city)
        {
            // no implementation for now
        }

        public bool Save()
        {
            return _context.SaveChanges() > 0;
        }

        public async Task<bool> SaveAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}