using System.ComponentModel.DataAnnotations;

namespace MiApi.DTOs
{
    public class CrearCarritoDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int IdUsuario { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int IdRestaurante { get; set; }
    }

    public class CambiarEstadoCarritoDto
    {
        [Required]
        [MaxLength(20)]
        public string Estado { get; set; } = string.Empty;
    }

    public class CarritoRespuestaDto
    {
        public int IdCarrito { get; set; }

        public int IdUsuario { get; set; }

        public string Usuario { get; set; } = string.Empty;

        public int IdRestaurante { get; set; }

        public string Restaurante { get; set; } = string.Empty;

        public DateTime FechaCreacion { get; set; }

        public string Estado { get; set; } = string.Empty;

        public int CantidadProductos { get; set; }

        public int CantidadDetalles { get; set; }

        public decimal Total { get; set; }
    }

    public class CarritoDetalleRespuestaDto
    {
        public int IdCarrito { get; set; }

        public int IdUsuario { get; set; }

        public string Usuario { get; set; } = string.Empty;

        public int IdRestaurante { get; set; }

        public string Restaurante { get; set; } = string.Empty;

        public DateTime FechaCreacion { get; set; }

        public string Estado { get; set; } = string.Empty;

        public int CantidadProductos { get; set; }

        public decimal Total { get; set; }

        public List<ProductoCarritoDto> Detalles { get; set; }
            = new();
    }

    public class ProductoCarritoDto
    {
        public int IdDetalleCarrito { get; set; }

        public int IdProducto { get; set; }

        public string Producto { get; set; } = string.Empty;

        public string? Imagen { get; set; }

        public int Cantidad { get; set; }

        public decimal PrecioUnitario { get; set; }

        public decimal Subtotal { get; set; }

        public bool Disponible { get; set; }
    }
}