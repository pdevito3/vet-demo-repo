namespace VetClinic.Api.Tests.Fakes.Vet
{
    using AutoBogus;
        using Application.Dtos.Vet;

    // or replace 'AutoFaker' with 'Faker' along with your own rules if you don't want all fields to be auto faked
    public class FakeVetDto : AutoFaker<VetDto>
    {
        public FakeVetDto()
        {
            // if you want default values on any of your properties (e.g. an int between a certain range or a date always in the past), you can add `RuleFor` lines describing those defaults
            //RuleFor(v => v.ExampleIntProperty, v => v.Random.Number(50, 100000));
            //RuleFor(v => v.ExampleDateProperty, v => v.Date.Past()); 
        }
    }
}