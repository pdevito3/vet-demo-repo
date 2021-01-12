namespace Infrastructure.Persistence.Repositories
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Application.Dtos.Vet;
    using Application.Interfaces.Vet;
    using Application.Wrappers;
    using Domain.Entities;
    using Infrastructure.Persistence.Contexts;
    using Microsoft.EntityFrameworkCore;
    using Sieve.Models;
    using Sieve.Services;

    public class VetRepository : IVetRepository
    {
        private VetClinicDbContext _context;
        private readonly SieveProcessor _sieveProcessor;

        public VetRepository(VetClinicDbContext context,
            SieveProcessor sieveProcessor)
        {
            _context = context
                ?? throw new ArgumentNullException(nameof(context));
            _sieveProcessor = sieveProcessor ??
                throw new ArgumentNullException(nameof(sieveProcessor));
        }

        public async Task<PagedList<Vet>> GetVetsAsync(VetParametersDto vetParameters)
        {
            if (vetParameters == null)
            {
                throw new ArgumentNullException(nameof(vetParameters));
            }

            var collection = _context.Vets
                as IQueryable<Vet>; // TODO: AsNoTracking() should increase performance, but will break the sort tests. need to investigate

            var sieveModel = new SieveModel
            {
                Sorts = vetParameters.SortOrder ?? "VetId",
                Filters = vetParameters.Filters
            };

            collection = _sieveProcessor.Apply(sieveModel, collection);

            return await PagedList<Vet>.CreateAsync(collection,
                vetParameters.PageNumber,
                vetParameters.PageSize);
        }

        public async Task<Vet> GetVetAsync(int vetId)
        {
            // include marker -- requires return _context.Vets as it's own line with no extra text -- do not delete this comment
            return await _context.Vets
                .FirstOrDefaultAsync(v => v.VetId == vetId);
        }

        public Vet GetVet(int vetId)
        {
            // include marker -- requires return _context.Vets as it's own line with no extra text -- do not delete this comment
            return _context.Vets
                .FirstOrDefault(v => v.VetId == vetId);
        }

        public async Task AddVet(Vet vet)
        {
            if (vet == null)
            {
                throw new ArgumentNullException(nameof(Vet));
            }

            await _context.Vets.AddAsync(vet);
        }

        public void DeleteVet(Vet vet)
        {
            if (vet == null)
            {
                throw new ArgumentNullException(nameof(Vet));
            }

            _context.Vets.Remove(vet);
        }

        public void UpdateVet(Vet vet)
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