using System.ComponentModel.DataAnnotations;

namespace MiApi.DTOs
{
    public class CrearDireccionDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int IdUsuario { get; set; }

        [Required]
        [MaxLength(150)]
        public string Colonia { get; set; } = string.Empty;

        [Required]
        [Range(-90, 90)]
        public double Latitud { get; set; }

        [Required]
        [Range(-180, 180)]
        public double Longitud { get; set; }
    }

    public class ActualizarDireccionDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int IdUsuario { get; set; }

        [Required]
        [MaxLength(150)]
        public string Colonia { get; set; } = string.Empty;

        [Required]
        [Range(-90, 90)]
        public double Latitud { get; set; }

        [Required]
        [Range(-180, 180)]
        public double Longitud { get; set; }
    }

    public class ActualizarCoordenadasDto
    {
        [Required]
        [Range(-90, 90)]
        public double Latitud { get; set; }

        [Required]
        [Range(-180, 180)]
        public double Longitud { get; set; }
    }

    public class DireccionRespuestaDto
    {
        public int IdDireccion { get; set; }

        public int IdUsuario { get; set; }

        public string Usuario { get; set; } = string.Empty;

        public string Colonia { get; set; } = string.Empty;

        public double Latitud { get; set; }

        public double Longitud { get; set; }

        public int CantidadPedidos { get; set; }
    }
}