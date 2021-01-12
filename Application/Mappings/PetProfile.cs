namespace Application.Mappings
{
    using Application.Dtos.Pet;
    using AutoMapper;
    using Domain.Entities;

    public class PetProfile : Profile
    {
        public PetProfile()
        {
            //createmap<to this, from this>
            CreateMap<Pet, PetDto>()
                .ReverseMap();
            CreateMap<PetForCreationDto, Pet>();
            CreateMap<PetForUpdateDto, Pet>()
                .ReverseMap();
        }
    }
}