using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiApi.Models
{
    public class Direccion
    {
        public int IdDireccion { get; set; }

        public int IdUsuario { get; set; }

        public string Colonia { get; set; } = string.Empty;

        public double Latitud { get; set; }

        public double Longitud { get; set; }

        public Usuarios Usuario { get; set; } = null!;

        public ICollection<Pedido> Pedidos { get; set; }
            = new List<Pedido>();
    }
}