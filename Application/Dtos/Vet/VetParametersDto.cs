namespace Application.Dtos.Vet
{
    using Application.Dtos.Shared;

    public class VetParametersDto : BasePaginationParameters
    {
        public string Filters { get; set; }
        public string SortOrder { get; set; }
    }
}