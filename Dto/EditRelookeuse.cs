using System.ComponentModel.DataAnnotations;

namespace UrbanSisters.Dto
{
    public class EditRelookeuse
    {
        [Required]
        public string Description { get; set; }

        [Required]
        public bool IsPro { get; set; }
        
        [Required]
        public byte[] RowVersion { get; set; }
    }
}