using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UrbanSisters.Model
{
    public partial class User
    {
        public User()
        {
            Appointment = new HashSet<Appointment>();
            ChatMessage = new HashSet<ChatMessage>();
            Participation = new HashSet<Participation>();
        }

        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool IsAdmin { get; set; }
        
        [Timestamp]
        public byte[] RowVersion { get; set; }

        public virtual Relookeuse Relookeuse { get; set; }
        public virtual ICollection<Appointment> Appointment { get; set; }
        public virtual ICollection<ChatMessage> ChatMessage { get; set; }
        public virtual ICollection<Participation> Participation { get; set; }
    }
}
