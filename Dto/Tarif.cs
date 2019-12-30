namespace UrbanSisters.Dto
{
    public class Tarif
    {
        public string Service { get; set; }
        public decimal Price { get; set; }
        public byte[] RowVersion { get; set; }
    }
}