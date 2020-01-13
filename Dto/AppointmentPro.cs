using System;

namespace UrbanSisters.Dto
{
    public class AppointmentPro
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserFirstName { get; set; }
        public string UserLastName { get; set; }
        public DateTime Date { get; set; }
        public bool Accepted { get; set; }
        public bool Makeup { get; set; }
        public string CancelRaison { get; set; }
        public int? Mark { get; set; }
        public bool Finished { get; set; }
        public byte[] RowVersion { get; set; }
    }
}