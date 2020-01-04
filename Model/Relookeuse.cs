using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

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
        
        [Timestamp]
        public byte[] RowVersion { get; set; }

        public double? AvgMark
        {
            get
            {
                if (Appointment.All(ap => ap.Mark == null))
                {
                    return null;
                }
            
                return Appointment.Aggregate(0.0, (i, appointment) => i + appointment.Mark.GetValueOrDefault(0)) / Appointment.Count(ap => ap.Mark != null);
            }
        }
    }
}
