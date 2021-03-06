﻿using System;
using System.ComponentModel.DataAnnotations;

namespace UrbanSisters.Dto
{
    public class Tarif
    {
        [Required]
        public string Service { get; set; }
        
        [Required]
        [Range(0.0, Double.MaxValue)]
        public decimal Price { get; set; }
        
        [Required]
        public byte[] RowVersion { get; set; }
    }
}