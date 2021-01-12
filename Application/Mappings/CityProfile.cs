namespace Application.Mappings
{
    using Application.Dtos.City;
    using AutoMapper;
    using Domain.Entities;

    public class CityProfile : Profile
    {
        public CityProfile()
        {
            //createmap<to this, from this>
            CreateMap<City, CityDto>()
                .ReverseMap();
            CreateMap<CityForCreationDto, City>();
            CreateMap<CityForUpdateDto, City>()
                .ReverseMap();
        }
    }
}