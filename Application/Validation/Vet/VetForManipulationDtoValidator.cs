namespace Application.Validation.Vet
{
    using Application.Dtos.Vet;
    using FluentValidation;
    using System;

    public class VetForManipulationDtoValidator<T> : AbstractValidator<T> where T : VetForManipulationDto
    {
        public VetForManipulationDtoValidator()
        {
            // add fluent validation rules that should be shared between creation and update operations here
            //https://fluentvalidation.net/
        }
    }
}