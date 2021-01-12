namespace Application.Mappings
{
    using Application.Dtos.Vet;
    using AutoMapper;
    using Domain.Entities;

    public class VetProfile : Profile
    {
        public VetProfile()
        {
            //createmap<to this, from this>
            CreateMap<Vet, VetDto>()
                .ReverseMap();
            CreateMap<VetForCreationDto, Vet>();
            CreateMap<VetForUpdateDto, Vet>()
                .ReverseMap();
        }
    }
}