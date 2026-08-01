using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MiApi.Models
{
    public class Carritos
    {
        public int IdCarrito { get; set; }

        public int IdUsuario { get; set; }

        public int IdRestaurante { get; set; }

        public DateTime FechaCreacion { get; set; }

        public string Estado { get; set; } = "Activo";

        public Usuarios Usuario { get; set; } = null!;

        public Restaurante Restaurante { get; set; } = null!;

        public ICollection<DetalleCarrito> Detalles { get; set; }
            = new List<DetalleCarrito>();

    }
}