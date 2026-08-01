using System.ComponentModel.DataAnnotations;

namespace MiApi.DTOs
{
    public class CrearProductoDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int IdRestaurante { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int IdCategoria { get; set; }

        [Required]
        [MaxLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Descripcion { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Precio { get; set; }

        [MaxLength(500)]
        public string? Imagen { get; set; }

        public bool Disponible { get; set; } = true;

      
    }

    public class ActualizarProductoDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int IdRestaurante { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int IdCategoria { get; set; }

        [Required]
        [MaxLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Descripcion { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Precio { get; set; }

        [MaxLength(500)]
        public string? Imagen { get; set; }

        public bool Disponible { get; set; }
        

 

    public class CambiarDisponibilidadProductoDto
    {
        public bool Disponible { get; set; }
    }

    public class ProductoRespuestaDto
    {
        public int IdProducto { get; set; }

        public int IdRestaurante { get; set; }

        public string Restaurante { get; set; } = string.Empty;

        public int IdCategoria { get; set; }

        public string Categoria { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public decimal Precio { get; set; }

        public string? Imagen { get; set; }

        public bool Disponible { get; set; }

       
    }
}}