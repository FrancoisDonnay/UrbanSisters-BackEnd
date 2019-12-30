using System;

namespace UrbanSisters.Dto
{
    public class Appointment
    {
        public int Id { get; set; }
        public int RelookeuseId { get; set; }
        public string RelookeuseFirstName { get; set; }
        public string RelookeuseLastName { get; set; }
        public DateTime Date { get; set; }
        public bool Accepted { get; set; }
        public bool Makeup { get; set; }
        public string CancelRaison { get; set; }
        public int? Mark { get; set; }
        public bool Finished { get; set; }
        public byte[] RowVersion { get; set; }
    }
}