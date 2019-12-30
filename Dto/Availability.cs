namespace UrbanSisters.Dto
{
    public class Availability
    {
        public int Id { get; set; }
        public int DayOfWeek { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public byte[] RowVersion { get; set; }
    }
}