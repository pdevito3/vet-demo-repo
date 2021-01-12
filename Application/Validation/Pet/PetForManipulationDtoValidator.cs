namespace Application.Validation.Pet
{
    using Application.Dtos.Pet;
    using FluentValidation;
    using System;

    public class PetForManipulationDtoValidator<T> : AbstractValidator<T> where T : PetForManipulationDto
    {
        public PetForManipulationDtoValidator()
        {
            // add fluent validation rules that should be shared between creation and update operations here
            //https://fluentvalidation.net/
        }
    }
}