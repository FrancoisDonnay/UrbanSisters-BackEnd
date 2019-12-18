using System.ComponentModel.DataAnnotations;

namespace UrbanSisters.Dto
{
    public class UserInscription
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
        
        [Required]
        [Compare("Password")]
        public string PasswordConfirmation { get; set; }
    }
}
