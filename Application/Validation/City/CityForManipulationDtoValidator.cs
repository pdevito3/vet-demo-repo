namespace Application.Validation.City
{
    using Application.Dtos.City;
    using FluentValidation;
    using System;

    public class CityForManipulationDtoValidator<T> : AbstractValidator<T> where T : CityForManipulationDto
    {
        public CityForManipulationDtoValidator()
        {
            // add fluent validation rules that should be shared between creation and update operations here
            //https://fluentvalidation.net/
        }
    }
}