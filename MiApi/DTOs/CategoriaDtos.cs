using System.ComponentModel.DataAnnotations;

namespace MiApi.DTOs
{
    public class CrearCategoriaDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

    
    }

    public class ActualizarCategoriaDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [MaxLength(500)]
        public string Descripcion { get; set; } = string.Empty;
    }

    public class CategoriaRespuestaDto
    {
        public int IdCategoria { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        public int CantidadProductos { get; set; }
    }

    public class ProductoCategoriaDto
    {
        public int IdProducto { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public decimal Precio { get; set; }

        public string? Imagen { get; set; }

        public bool Disponible { get; set; }

        public int IdRestaurante { get; set; }

        public string Restaurante { get; set; } = string.Empty;
    }

    public class CategoriaDetalleDto
    {
        public int IdCategoria { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        public int CantidadProductos { get; set; }

        public List<ProductoCategoriaDto> Productos { get; set; }
            = new();
    }
}