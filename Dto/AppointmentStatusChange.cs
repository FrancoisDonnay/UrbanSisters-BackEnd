using System;
using System.ComponentModel.DataAnnotations;

namespace UrbanSisters.Dto
{
    public class AppointmentStatusChange
    {
        [Required]
        public bool Accepted { get; set; }
        public string CancelMessage { get; set; }
        
        [Required]
        public byte[] RowVersion { get; set; }
    }
}