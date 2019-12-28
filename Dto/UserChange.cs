using System.ComponentModel.DataAnnotations;

namespace UrbanSisters.Dto
{
    public class UserChange
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public byte[] RowVersion { get; set; }
    }
}