using System.ComponentModel.DataAnnotations;

namespace MiApi.Models
{
    public class Pedido
    {
        [Key]
        public int IdPedido { get; set; }

        public int IdUsuario { get; set; }

        public int IdRestaurante { get; set; }

        public int IdDireccionEntrega { get; set; }

        public int? IdRepartidor { get; set; }

        public string NumeroPedido { get; set; } = string.Empty;

        public DateTime FechaPedido { get; set; }

        public string Estado { get; set; } = string.Empty;

        public decimal Total { get; set; }

        public string TipoPago { get; set; } = string.Empty;

        public int? CalificacionRestaurante { get; set; }

        public int? CalificacionRepartidor { get; set; }

        public Usuarios Usuario { get; set; } = null!;

        public Restaurante Restaurante { get; set; } = null!;

        public Direccion DireccionEntrega { get; set; } = null!;

        public Repartidor? Repartidor { get; set; }

        public ICollection<DetallePedido> Detalles { get; set; }
            = new List<DetallePedido>();

        public Pago? Pago { get; set; }
    }
}