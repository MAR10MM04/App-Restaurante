using System.ComponentModel.DataAnnotations;

namespace MiApi.DTOs
{
    public class CrearDetalleCarritoDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int IdCarrito { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int IdProducto { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Cantidad { get; set; }
    }

    public class ActualizarDetalleCarritoDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int Cantidad { get; set; }
    }

    public class DetalleCarritoRespuestaDto
    {
        public int IdDetalleCarrito { get; set; }

        public int IdCarrito { get; set; }

        public int IdProducto { get; set; }

        public string Producto { get; set; } = string.Empty;

        public string? Imagen { get; set; }

        public int Cantidad { get; set; }

        public decimal PrecioUnitario { get; set; }

        public decimal Subtotal { get; set; }

        public bool Disponible { get; set; }
    }
}