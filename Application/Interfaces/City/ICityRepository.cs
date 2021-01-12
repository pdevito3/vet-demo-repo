namespace Application.Interfaces.City
{
    using System;
    using Application.Dtos.City;
    using Application.Wrappers;
    using System.Threading.Tasks;
    using Domain.Entities;

    public interface ICityRepository
    {
        Task<PagedList<City>> GetCitiesAsync(CityParametersDto CityParameters);
        Task<City> GetCityAsync(int CityId);
        City GetCity(int CityId);
        Task AddCity(City city);
        void DeleteCity(City city);
        void UpdateCity(City city);
        bool Save();
        Task<bool> SaveAsync();
    }
}