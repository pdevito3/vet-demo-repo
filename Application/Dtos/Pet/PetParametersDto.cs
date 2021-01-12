namespace Application.Dtos.Pet
{
    using Application.Dtos.Shared;

    public class PetParametersDto : BasePaginationParameters
    {
        public string Filters { get; set; }
        public string SortOrder { get; set; }
    }
}