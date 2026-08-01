using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiApi.Models
{
    public class UsuarioRol
    {
        public int IdUsuario { get; set; }

        public int IdRol { get; set; }

        public DateTime FechaAsignacion { get; set; }

        public Usuarios Usuario { get; set; } = null!;

        public Rol Rol { get; set; } = null!;
    }
}