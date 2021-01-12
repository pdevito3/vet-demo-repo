namespace Application.Interfaces.Vet
{
    using System;
    using Application.Dtos.Vet;
    using Application.Wrappers;
    using System.Threading.Tasks;
    using Domain.Entities;

    public interface IVetRepository
    {
        Task<PagedList<Vet>> GetVetsAsync(VetParametersDto VetParameters);
        Task<Vet> GetVetAsync(int VetId);
        Vet GetVet(int VetId);
        Task AddVet(Vet vet);
        void DeleteVet(Vet vet);
        void UpdateVet(Vet vet);
        bool Save();
        Task<bool> SaveAsync();
    }
}