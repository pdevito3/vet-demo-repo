namespace Domain.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Sieve.Attributes;

    [Table("Vet")]
    public class Vet
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        [Sieve(CanFilter = true, CanSort = false)]
        public int VetId { get; set; }

        [Sieve(CanFilter = true, CanSort = true)]
        public string Name { get; set; }

        [Sieve(CanFilter = true, CanSort = true)]
        public int Capacity { get; set; }

        [Sieve(CanFilter = true, CanSort = true)]
        public DateTime? OpenDate { get; set; }

        [Sieve(CanFilter = true, CanSort = true)]
        public bool HasSpayNeuter { get; set; }

        // add-on property marker - Do Not Delete This Comment
    }
}