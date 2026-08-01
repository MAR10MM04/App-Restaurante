using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace MiApi.DTOs
{
    public class CrearProductoFormDto
    {
        [Required]
        public int IdRestaurante { get; set; }

        [Required]
        public int IdCategoria { get; set; }

        [Required]
        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        [Required]
        public decimal Precio { get; set; }

        public bool Disponible { get; set; } = true;

     

        // Imagen desde el dispositivo
        public IFormFile? Imagen { get; set; }
    }
}