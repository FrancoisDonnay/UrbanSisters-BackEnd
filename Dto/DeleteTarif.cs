using System.ComponentModel.DataAnnotations;

namespace UrbanSisters.Dto
{
    public class DeleteTarif
    {
        [Required]
        public string Service { get; set; }
        [Required]
        public byte[] RowVersion { get; set; }
    }
}