using System.ComponentModel.DataAnnotations;

namespace UrbanSisters.Dto
{
    public class RelookeuseRowVersion
    {
        [Required]
        public byte[] RowVersion { get; set; }
    }
}