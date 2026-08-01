using System.ComponentModel.DataAnnotations;

namespace MiApi.DTOs
{
    public class CrearRestauranteDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int IdUsuarioPropietario { get; set; }

        [Required]
        [MaxLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Descripcion { get; set; }

        [MaxLength(255)]
        public string? Direccion { get; set; }

        [MaxLength(20)]
        public string? Telefono { get; set; }

        [MaxLength(20)]
        public string? HorarioApertura { get; set; }

        [MaxLength(20)]
        public string? HorarioCierre { get; set; }

        [MaxLength(500)]
        public string? Imagen { get; set; }

        [Range(-90, 90)]
        public double? Latitud { get; set; }

        [Range(-180, 180)]
        public double? Longitud { get; set; }
    }

    public class ActualizarRestauranteDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int IdUsuarioPropietario { get; set; }

        [Required]
        [MaxLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Descripcion { get; set; }

        [MaxLength(255)]
        public string? Direccion { get; set; }

        [MaxLength(20)]
        public string? Telefono { get; set; }

        [MaxLength(20)]
        public string? HorarioApertura { get; set; }

        [MaxLength(20)]
        public string? HorarioCierre { get; set; }

        [MaxLength(500)]
        public string? Imagen { get; set; }

        [Range(-90, 90)]
        public double? Latitud { get; set; }

        [Range(-180, 180)]
        public double? Longitud { get; set; }
    }

    public class RestauranteRespuestaDto
    {
        public int IdRestaurante { get; set; }

        public int IdUsuarioPropietario { get; set; }

        public string Propietario { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public string? Direccion { get; set; }

        public string? Telefono { get; set; }

        public string? HorarioApertura { get; set; }

        public string? HorarioCierre { get; set; }

        public string? Imagen { get; set; }

        public double? Latitud { get; set; }

        public double? Longitud { get; set; }

        public int CantidadProductos { get; set; }

        public int CantidadPedidos { get; set; }

        public int CantidadCarritos { get; set; }
    }
}