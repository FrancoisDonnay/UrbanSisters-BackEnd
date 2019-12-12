using System;
using System.Collections.Generic;

namespace UrbanSisters.Model
{
    public partial class Relookeuse
    {
        public Relookeuse()
        {
            Appointment = new HashSet<Appointment>();
            Availability = new HashSet<Availability>();
            PortfolioPicture = new HashSet<PortfolioPicture>();
            Tarif = new HashSet<Tarif>();
        }

        public int UserId { get; set; }
        public string Picture { get; set; }
        public string Description { get; set; }
        public bool IsPro { get; set; }

        public virtual User User { get; set; }
        public virtual ICollection<Appointment> Appointment { get; set; }
        public virtual ICollection<Availability> Availability { get; set; }
        public virtual ICollection<PortfolioPicture> PortfolioPicture { get; set; }
        public virtual ICollection<Tarif> Tarif { get; set; }
    }
}
