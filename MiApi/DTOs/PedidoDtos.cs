using System.ComponentModel.DataAnnotations;

namespace MiApi.DTOs
{
    public class CrearPedidoDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int IdUsuario { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int IdRestaurante { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int IdDireccionEntrega { get; set; }

        public int? IdRepartidor { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Total { get; set; }

        [Required]
        [MaxLength(30)]
        public string TipoPago { get; set; } = string.Empty;
    }

    public class ActualizarPedidoDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int IdUsuario { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int IdRestaurante { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int IdDireccionEntrega { get; set; }

        public int? IdRepartidor { get; set; }

        [Required]
        [MaxLength(30)]
        public string Estado { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Total { get; set; }

        [Required]
        [MaxLength(30)]
        public string TipoPago { get; set; } = string.Empty;
    }

    public class CambiarEstadoPedidoDto
    {
        [Required]
        [MaxLength(30)]
        public string Estado { get; set; } = string.Empty;
    }

    public class AsignarRepartidorDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int IdRepartidor { get; set; }
    }

    public class CalificarPedidoDto
    {
        [Range(1, 5)]
        public int? CalificacionRestaurante { get; set; }

        [Range(1, 5)]
        public int? CalificacionRepartidor { get; set; }
    }

    public class PedidoRespuestaDto
    {
        public int IdPedido { get; set; }

        public string NumeroPedido { get; set; } = string.Empty;

        public DateTime FechaPedido { get; set; }

        public string Estado { get; set; } = string.Empty;

        public decimal Total { get; set; }

        public string TipoPago { get; set; } = string.Empty;

        public int IdUsuario { get; set; }

        public string Usuario { get; set; } = string.Empty;

        public int IdRestaurante { get; set; }

        public string Restaurante { get; set; } = string.Empty;

        public int IdDireccionEntrega { get; set; }

        public int? IdRepartidor { get; set; }

        public string? Repartidor { get; set; }

        public int? CalificacionRestaurante { get; set; }

        public int? CalificacionRepartidor { get; set; }

        public int CantidadDetalles { get; set; }

        public bool TienePago { get; set; }
    }
}