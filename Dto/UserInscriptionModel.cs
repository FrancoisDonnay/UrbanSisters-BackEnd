using System.ComponentModel.DataAnnotations;

namespace UrbanSisters.Dto
{
    public class UserInscriptionModel
    {
        [Required]
        private string FirstName { get; set; }

        [Required]
        private string LastName { get; set; }

        [Required]
        private string Email { get; set; }

        [Required]
        private string Password { get; set; }
    }
}
