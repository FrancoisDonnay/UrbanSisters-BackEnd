using System.ComponentModel.DataAnnotations;

namespace UrbanSisters.Dto
{
    public class UserRowVersion
    {
        [Required]
        public byte[] RowVersion { get; set; }
    }
}