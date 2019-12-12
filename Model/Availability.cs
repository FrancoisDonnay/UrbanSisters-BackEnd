using System;
using System.Collections.Generic;

namespace UrbanSisters.Model
{
    public partial class Availability
    {
        public int Id { get; set; }
        public int RelookeuseId { get; set; }
        public int DayOfWeek { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }

        public virtual Relookeuse Relookeuse { get; set; }
    }
}
