namespace UrbanSisters.Dto
{
    public class NewRelookeuse
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Description { get; set; }
        public bool IsPro { get; set; }
        public byte[] RowVersion { get; set; }
        public JwtToken NewToken { get; set; }
    }
}