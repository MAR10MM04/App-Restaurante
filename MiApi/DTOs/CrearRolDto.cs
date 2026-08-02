using System.ComponentModel.DataAnnotations;

namespace MiApi.DTOs
{
    public class CrearRolDto
    {
        [Required]
        [MaxLength(50)]
        public string Nombre { get; set; } = string.Empty;

    
    }

    public class RolRespuestaDto
    {
        public int IdRol { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }
    }
}