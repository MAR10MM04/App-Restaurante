using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiApi.Models
{
    public class Usuarios
    {
    public int Id_Usuario { get; set; }
    public string Nombre { get; set; }
    public decimal Email { get; set; }
      public string Telefono { get; set; }
     public string Password { get; set; }
       public string Fecha_Registro { get; set; }
     public string tipo_usario {get; set;}


    }
}