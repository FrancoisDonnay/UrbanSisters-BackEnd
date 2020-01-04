using System;

namespace UrbanSisters.Model
{
    public partial class ChatMessage
    {
        public int Id { get; set; }
        public int AppointmentId { get; set; }
        public int SenderId { get; set; }
        public DateTime Date { get; set; }
        public string Message { get; set; }

        public virtual Appointment Appointment { get; set; }
        public virtual User Sender { get; set; }
    }
}
