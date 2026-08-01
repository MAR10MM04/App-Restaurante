using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiApi.Models
{

public class Restaurante
{
    public int IdRestaurante { get; set; }

    public int IdUsuarioPropietario { get; set; }

    public string Nombre { get; set; } 

    public Usuarios UsuarioPropietario { get; set; } 

    public string? Descripcion { get; set; }
     public string? Direccion { get; set; }
     public string? Telefono { get; set; }
     public string? HorarioApertura { get; set; }
     public string? HorarioCierre { get; set; }
     public string? Imagen { get; set; }
     public double? Latitud { get; set; }
        public double? Longitud { get; set; }
     
    public ICollection<Producto> Productos { get; set; }
        = new List<Producto>();

    public ICollection<Pedido> Pedidos { get; set; }
        = new List<Pedido>();

    public ICollection<Carritos> Carritos { get; set; }
        = new List<Carritos>();
    
    }
    }
