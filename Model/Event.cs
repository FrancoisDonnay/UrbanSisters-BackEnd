using System;
using System.Collections.Generic;

namespace UrbanSisters.Model
{
    public partial class Event
    {
        public Event()
        {
            Participation = new HashSet<Participation>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Description { get; set; }
        public string Picture { get; set; }

        public virtual ICollection<Participation> Participation { get; set; }
    }
}
