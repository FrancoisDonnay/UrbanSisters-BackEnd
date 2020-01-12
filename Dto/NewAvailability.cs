using System.ComponentModel.DataAnnotations;

namespace UrbanSisters.Dto
{
    public class NewAvailability
    {
        [Required]
        [Range(1, 7)]
        public int DayOfWeek { get; set; }
        
        [Required]
        [RegularExpression("[0-1][0-9]:[0-5][0-9]")]
        public string StartTime { get; set; }
        
        [Required]
        [RegularExpression("[0-1][0-9]:[0-5][0-9]")]
        public string EndTime { get; set; }
    }
}