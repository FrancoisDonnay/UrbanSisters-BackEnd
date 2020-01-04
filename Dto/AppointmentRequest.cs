using System;
using System.ComponentModel.DataAnnotations;

namespace UrbanSisters.Dto
{
    public class AppointmentRequest
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int RelookeuseId { get; set; }
        [Required]
        public bool MakeUp { get; set; }
    }
}