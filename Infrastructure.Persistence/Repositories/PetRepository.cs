namespace Infrastructure.Persistence.Repositories
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Application.Dtos.Pet;
    using Application.Interfaces.Pet;
    using Application.Wrappers;
    using Domain.Entities;
    using Infrastructure.Persistence.Contexts;
    using Microsoft.EntityFrameworkCore;
    using Sieve.Models;
    using Sieve.Services;

    public class PetRepository : IPetRepository
    {
        private VetClinicDbContext _context;
        private readonly SieveProcessor _sieveProcessor;

        public PetRepository(VetClinicDbContext context,
            SieveProcessor sieveProcessor)
        {
            _context = context
                ?? throw new ArgumentNullException(nameof(context));
            _sieveProcessor = sieveProcessor ??
                throw new ArgumentNullException(nameof(sieveProcessor));
        }

        public async Task<PagedList<Pet>> GetPetsAsync(PetParametersDto petParameters)
        {
            if (petParameters == null)
            {
                throw new ArgumentNullException(nameof(petParameters));
            }

            var collection = _context.Pets
                as IQueryable<Pet>; // TODO: AsNoTracking() should increase performance, but will break the sort tests. need to investigate

            var sieveModel = new SieveModel
            {
                Sorts = petParameters.SortOrder ?? "PetId",
                Filters = petParameters.Filters
            };

            collection = _sieveProcessor.Apply(sieveModel, collection);

            return await PagedList<Pet>.CreateAsync(collection,
                petParameters.PageNumber,
                petParameters.PageSize);
        }

        public async Task<Pet> GetPetAsync(Guid petId)
        {
            // include marker -- requires return _context.Pets as it's own line with no extra text -- do not delete this comment
            return await _context.Pets
                .FirstOrDefaultAsync(p => p.PetId == petId);
        }

        public Pet GetPet(Guid petId)
        {
            // include marker -- requires return _context.Pets as it's own line with no extra text -- do not delete this comment
            return _context.Pets
                .FirstOrDefault(p => p.PetId == petId);
        }

        public async Task AddPet(Pet pet)
        {
            if (pet == null)
            {
                throw new ArgumentNullException(nameof(Pet));
            }

            await _context.Pets.AddAsync(pet);
        }

        public void DeletePet(Pet pet)
        {
            if (pet == null)
            {
                throw new ArgumentNullException(nameof(Pet));
            }

            _context.Pets.Remove(pet);
        }

        public void UpdatePet(Pet pet)
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