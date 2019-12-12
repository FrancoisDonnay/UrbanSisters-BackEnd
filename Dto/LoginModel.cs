using System.ComponentModel.DataAnnotations;

namespace UrbanSisters.Dto
{
    public class LoginModel
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}