using System.Collections.Generic;
using UrbanSisters.Model;

namespace UrbanSisters.Dto
{
    public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsRelookeuse { get; set; }
        
        public byte[] RowVersion { get; set; }
    }
}
