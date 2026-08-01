using System.ComponentModel.DataAnnotations;

namespace MiApi.DTOs
{
    public class CrearPagoDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int IdPedido { get; set; }

        [Required]
        [MaxLength(30)]
        public string MetodoPago { get; set; } = string.Empty;
    }

    public class ActualizarPagoDto
    {
        [Required]
        [MaxLength(30)]
        public string MetodoPago { get; set; } = string.Empty;
    }

    public class PagoRespuestaDto
    {
        public int IdPago { get; set; }

        public int IdPedido { get; set; }

        public string NumeroPedido { get; set; } = string.Empty;

        public decimal Monto { get; set; }

        public string MetodoPago { get; set; } = string.Empty;

        public DateTime FechaPago { get; set; }

        public string EstadoPedido { get; set; } = string.Empty;

        public int IdUsuario { get; set; }

        public string Usuario { get; set; } = string.Empty;

        public int IdRestaurante { get; set; }

        public string Restaurante { get; set; } = string.Empty;
    }
}