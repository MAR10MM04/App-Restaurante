using System.ComponentModel.DataAnnotations;

namespace MiApi.DTOs
{
    public class CrearUsuarioDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Telefono { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
        public string Contrasena { get; set; } = string.Empty;

        
        
    }
        public class LoginRespuestaDto
    {
        public string Token { get; set; } = string.Empty;

        public int IdUsuario { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public List<string> Roles { get; set; } = new();
    }
     public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Contrasena { get; set; } = string.Empty;
    }

    public class ActualizarUsuarioDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Telefono { get; set; }

        [Required(ErrorMessage = "El estado es obligatorio.")]
        [MaxLength(20)]
        public string Estado { get; set; } = string.Empty;
    }

    public class UsuarioRespuestaDto
    {
        public int IdUsuario { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? Telefono { get; set; }

        public DateTime FechaRegistro { get; set; }

        public string Estado { get; set; } = string.Empty;

        public List<string> Roles { get; set; } = new();

        public int CantidadRestaurantes { get; set; }

        public int CantidadPedidos { get; set; }

        public int CantidadDirecciones { get; set; }

        public int CantidadCarritos { get; set; }

        public bool TienePerfilRepartidor { get; set; }
    }

    public class CambiarContrasenaDto
    {
        [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
        public string NuevaContrasena { get; set; } = string.Empty;
    }

    public class CambiarEstadoUsuarioDto
    {
        [Required(ErrorMessage = "El estado es obligatorio.")]
        [MaxLength(20)]
        public string Estado { get; set; } = string.Empty;
    }

    public class AsignarRolDto
    {
        [Required(ErrorMessage = "El nombre del rol es obligatorio.")]
        [MaxLength(50)]
        public string NombreRol { get; set; } = string.Empty;
    }
}