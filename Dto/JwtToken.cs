namespace UrbanSisters.Dto
{
    public class JwtToken
    {
        public string access_token { get; set; }
        public long expire_at { get; set; }
    }
}