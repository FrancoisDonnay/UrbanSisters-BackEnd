namespace UrbanSisters.Model
{
    public partial class PortfolioPicture
    {
        public int Id { get; set; }
        public int RelookeuseId { get; set; }
        public string Picture { get; set; }

        public virtual Relookeuse Relookeuse { get; set; }
    }
}
