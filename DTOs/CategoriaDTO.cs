﻿using System.ComponentModel.DataAnnotations;

namespace MinhaAPI.DTOs
{
    public class CategoriaDTO
    {
        public int CategoriaId { get; set; }

        [Required]
        [StringLength(80)]
        public string Nome { get; set; }


        [Required]
        [StringLength(300)]
        public string? ImagemUrl { get; set; }

    }
}
