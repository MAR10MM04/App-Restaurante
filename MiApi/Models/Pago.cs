using System;
using System.ComponentModel.DataAnnotations;

namespace MiApi.Models
{
    public class Pago
    {
        public int IdPago { get; set; }

        public int IdPedido { get; set; }

        public decimal Monto { get; set; }

        public string MetodoPago { get; set; } = string.Empty;

        public DateTime FechaPago { get; set; }

        public Pedido Pedido { get; set; } = null!;

    }
}