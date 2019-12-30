using System.Collections;
using System.Collections.Generic;

namespace UrbanSisters.Dto
{
    public class DetailedRelookeuse:Relookeuse
    {
        public virtual ICollection<Availability> Availability { get; set; }
        public virtual ICollection<PortfolioPicture> PortfolioPicture { get; set; }
        public virtual ICollection<Tarif> Tarif { get; set; }
    }
}