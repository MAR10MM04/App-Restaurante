using System.ComponentModel.DataAnnotations;

namespace MiApi.DTOs
{
    public class CrearRepartidorDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int IdUsuario { get; set; }
    }

    public class ActualizarRepartidorDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int IdUsuario { get; set; }

        [Required]
        [MaxLength(30)]
        public string Estado { get; set; } = string.Empty;
    }

    public class CambiarEstadoRepartidorDto
    {
        [Required]
        [MaxLength(30)]
        public string Estado { get; set; } = string.Empty;
    }

    public class RepartidorRespuestaDto
    {
        public int IdRepartidor { get; set; }

        public int IdUsuario { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string? Telefono { get; set; }

        public string Estado { get; set; } = string.Empty;

        public int CantidadPedidos { get; set; }

        public int PedidosPendientes { get; set; }

        public int PedidosEntregados { get; set; }
    }
}