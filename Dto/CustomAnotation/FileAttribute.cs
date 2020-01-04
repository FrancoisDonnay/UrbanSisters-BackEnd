using System;
using System.ComponentModel.DataAnnotations;  
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace UrbanSisters.Dto.CustomAnotation
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]  
    public class FileAttribute : ValidationAttribute  
    {
        public string Extensions { get; set; }
        public long MaxSize { get; set; }
        
        public override bool IsValid(object value)  
        {
            if (!(value is IFormFile))
            {
                return false;
            }
            
            IFormFile file = (IFormFile) value;

            bool valid = true;

            if (Extensions != null)
            {
                valid = Extensions.Split(new [] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList().Any(extension => file.FileName.EndsWith("."+extension));
            }

            if (valid && MaxSize > 0)
            {
                valid = file.Length < MaxSize;
            }

            return valid;
        }
    } 
}