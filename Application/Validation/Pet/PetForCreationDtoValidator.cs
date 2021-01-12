namespace Application.Validation.Pet
{
    using Application.Dtos.Pet;
    using FluentValidation;

    public class PetForCreationDtoValidator: PetForManipulationDtoValidator<PetForCreationDto>
    {
        public PetForCreationDtoValidator()
        {
            // add fluent validation rules that should only be run on creation operations here
            //https://fluentvalidation.net/
        }
    }
}