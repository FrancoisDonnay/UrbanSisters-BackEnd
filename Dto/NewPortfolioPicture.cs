using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using UrbanSisters.Dto.CustomAnotation;

namespace UrbanSisters.Dto
{
    public class NewPortfolioPicture
    {
        [Required]
        [File(Extensions = "png", MaxSize = 500000)]
        public IFormFile File { get; set; }
    }
}