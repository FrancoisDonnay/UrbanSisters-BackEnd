using System.ComponentModel.DataAnnotations;

namespace UrbanSisters.Dto
{
    public class RelookeuseInscription
    {
        [Required]
        public string Description { get; set; }

        [Required]
        public bool IsPro { get; set; }
    }
}