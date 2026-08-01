using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MiApi.Models
{
    public class Repartidor
    {
         public int IdRepartidor { get; set; }

        public int IdUsuario { get; set; }

        public string Estado { get; set; } = "Disponible";

        public Usuarios Usuario { get; set; } = null!;

        public ICollection<Pedido> Pedidos { get; set; }
            = new List<Pedido>();

    }
}