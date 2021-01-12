namespace Application.Dtos.City
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public abstract class CityForManipulationDto 
    {
        public string Name { get; set; }

        // add-on property marker - Do Not Delete This Comment
    }
}