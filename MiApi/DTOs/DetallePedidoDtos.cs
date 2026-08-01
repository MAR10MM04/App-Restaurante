using System.ComponentModel.DataAnnotations;

namespace MiApi.DTOs
{
    public class CrearDetallePedidoDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int IdPedido { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int IdProducto { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Cantidad { get; set; }
    }

    public class ActualizarDetallePedidoDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int Cantidad { get; set; }
    }

    public class DetallePedidoRespuestaDto
    {
        public int IdDetallePedido { get; set; }

        public int IdPedido { get; set; }

        public string NumeroPedido { get; set; } = string.Empty;

        public int IdProducto { get; set; }

        public string Producto { get; set; } = string.Empty;

        public string? Imagen { get; set; }

        public int Cantidad { get; set; }

        public decimal PrecioUnitario { get; set; }

        public decimal Subtotal { get; set; }
    }
}