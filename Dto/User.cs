using System.Collections.Generic;
using UrbanSisters.Model;

namespace UrbanSisters.Dto
{
    public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public bool IsAdmin { get; set; }
        public virtual Relookeuse Relookeuse { get; set; }
        public virtual ICollection<Appointment> Appointment { get; set; }
        public virtual ICollection<Participation> Participation { get; set; }
    }
}
