using System;
using System.ComponentModel.DataAnnotations;

namespace UrbanSisters.Dto
{
    public class SetAdminValue
    {
        [Required]
        [Range(1, Int32.MaxValue)]
        public int Id { get; set; }
        
        [Required]
        public bool IsAdmin { get; set; }
        
        [Required]
        public byte[] RowVersion { get; set; }
    }
}