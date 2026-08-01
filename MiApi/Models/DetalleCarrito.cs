using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MiApi.Models
{
    public class DetalleCarrito
    {
        public int IdDetalleCarrito { get; set; }

        public int IdCarrito { get; set; }

        public int IdProducto { get; set; }

        public int Cantidad { get; set; }

        public decimal PrecioUnitario { get; set; }

        public Carritos Carrito { get; set; } = null!;

        public Producto Producto { get; set; } = null!;
    }
}