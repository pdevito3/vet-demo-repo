namespace Application.Validation.City
{
    using Application.Dtos.City;
    using FluentValidation;

    public class CityForUpdateDtoValidator: CityForManipulationDtoValidator<CityForUpdateDto>
    {
        public CityForUpdateDtoValidator()
        {
            // add fluent validation rules that should only be run on update operations here
            //https://fluentvalidation.net/
        }
    }
}