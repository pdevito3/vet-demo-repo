namespace Application.Dtos.City
{
    using Application.Dtos.Shared;

    public class CityParametersDto : BasePaginationParameters
    {
        public string Filters { get; set; }
        public string SortOrder { get; set; }
    }
}