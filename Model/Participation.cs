using System;

namespace UrbanSisters.Model
{
    public partial class Participation
    {
        public int EventId { get; set; }
        public int UserId { get; set; }
        public DateTime Date { get; set; }

        public virtual Event Event { get; set; }
        public virtual User User { get; set; }
    }
}
