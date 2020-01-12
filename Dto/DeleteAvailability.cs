using System;
using System.ComponentModel.DataAnnotations;

namespace UrbanSisters.Dto
{
    public class DeleteAvailability
    {
        [Required]
        [Range(1, Int32.MaxValue)]
        public int Id { get; set; }
        [Required]
        public byte[] RowVersion { get; set; }
    }
}