using System;
using System.ComponentModel.DataAnnotations;

namespace UrbanSisters.Dto
{
    public class Availability
    {
        [Required]
        [Range(1, Int32.MaxValue)]
        public int Id { get; set; }
        
        [Required]
        [Range(1, 7)]
        public int DayOfWeek { get; set; }
        
        [Required]
        [RegularExpression("[0-2][0-9]:[0-5][0-9]")]
        public string StartTime { get; set; }
        
        [Required]
        [RegularExpression("[0-2][0-9]:[0-5][0-9]")]
        public string EndTime { get; set; }
        
        [Required]
        public byte[] RowVersion { get; set; }
    }
}