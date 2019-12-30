using System.ComponentModel.DataAnnotations;

namespace UrbanSisters.Dto
{
    public class Rating
    {
        [Required]
        [Range(0, 5)]
        public int Value { get; set; }
        [Required]
        public byte[] RowVersion { get; set; }
    }
}