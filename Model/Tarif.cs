using System;
using System.Collections.Generic;

namespace UrbanSisters.Model
{
    public partial class Tarif
    {
        public int RelookeuseId { get; set; }
        public string Service { get; set; }
        public decimal Price { get; set; }

        public virtual Relookeuse Relookeuse { get; set; }
    }
}
