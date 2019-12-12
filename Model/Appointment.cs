using System;
using System.Collections.Generic;

namespace UrbanSisters.Model
{
    public partial class Appointment
    {
        public Appointment()
        {
            ChatMessage = new HashSet<ChatMessage>();
        }

        public int Id { get; set; }
        public int UserId { get; set; }
        public int RelookeuseId { get; set; }
        public DateTime Date { get; set; }
        public bool Accepted { get; set; }
        public bool Makeup { get; set; }
        public string CancelRaison { get; set; }
        public int? Mark { get; set; }
        public bool Finished { get; set; }

        public virtual Relookeuse Relookeuse { get; set; }
        public virtual User User { get; set; }
        public virtual ICollection<ChatMessage> ChatMessage { get; set; }
    }
}
