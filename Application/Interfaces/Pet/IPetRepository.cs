namespace Application.Interfaces.Pet
{
    using System;
    using Application.Dtos.Pet;
    using Application.Wrappers;
    using System.Threading.Tasks;
    using Domain.Entities;

    public interface IPetRepository
    {
        Task<PagedList<Pet>> GetPetsAsync(PetParametersDto PetParameters);
        Task<Pet> GetPetAsync(Guid PetId);
        Pet GetPet(Guid PetId);
        Task AddPet(Pet pet);
        void DeletePet(Pet pet);
        void UpdatePet(Pet pet);
        bool Save();
        Task<bool> SaveAsync();
    }
}