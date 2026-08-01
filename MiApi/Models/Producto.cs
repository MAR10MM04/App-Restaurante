using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MiApi.Models
{
    public class Producto
    {
     public int IdProducto { get; set; }

    public int IdRestaurante { get; set; }

    public int IdCategoria { get; set; }

    public string Nombre { get; set; } 

    public decimal Precio { get; set; }

    public Restaurante Restaurante { get; set; }

    public Categoria Categoria { get; set; } 
     public string? Descripcion { get; set; }
      public string? Imagen { get; set; }
      public bool Disponible { get; set; } = true;
      
    public ICollection<DetallePedido> DetallesPedido { get; set; }
        = new List<DetallePedido>();

    public ICollection<DetalleCarrito> DetallesCarrito { get; set; }
        = new List<DetalleCarrito>();

    }
}