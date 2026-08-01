using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiApi.Models
{
    public class Rol
    {
        public int IdRol { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public ICollection<UsuarioRol> UsuariosRoles
            = new List<UsuarioRol>();
    }
}